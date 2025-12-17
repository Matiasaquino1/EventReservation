using AutoMapper;
using EventReservations.Data;
using EventReservations.Enums;
using EventReservations.Models;
using EventReservations.Profiles;
using EventReservations.Repositories;
using EventReservations.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Stripe;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;

try
{
    // Config Serilog antes del host
    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}"
        )
        .WriteTo.File("logs/app_log.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}"
        )
        .MinimumLevel.Information()
        .CreateLogger();

    Log.Information("Iniciando aplicación...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog como logger principal
    builder.Host.UseSerilog();

    // DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Repositories
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IEventRepository, EventRepository>();
    builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
    builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

    // Services
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IEventService, EventReservations.Services.EventService>();
    builder.Services.AddScoped<IReservationService, ReservationService>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();

    builder.Services.AddAutoMapper(typeof(MappingProfile));

    // JWT config
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role
        };
    });

    // Servicio de tokens
    builder.Services.AddSingleton<IJwtService, JwtService>();

    // Stripe
    StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

    //Cors config 
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(
                "http://localhost:5173",   // frontend local (React/Vite)
                "http://localhost:4200"    // opcional: Angular
                                           // "https://tudominio.com" // 
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });

    builder.Services.AddAuthorization();
    builder.Services.AddControllers();

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });

    // Validación global
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new
                {
                    Field = x.Key,
                    Errors = x.Value!.Errors.Select(e => e.ErrorMessage)
                });

            return new BadRequestObjectResult(new
            {
                statusCode = 400,
                message = "Error de validación de datos.",
                details = errors
            });
        };
    });

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Event Reservation API",
            Version = "v1",
            Description = "API para reservas de eventos con autenticación JWT, pagos Stripe y gestión de usuarios/organizadores."
        });

        var securitySchema = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Ingrese 'Bearer <token>'",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };
        
        options.AddSecurityDefinition("Bearer", securitySchema);

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                securitySchema,
                new string[] {}
            }
        });

        var xmlFile = "EventReservations.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (System.IO.File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });

    var app = builder.Build();

    // Middleware global de manejo de errores (reemplaza UseExceptionHandler("/error"))
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var error = exceptionHandlerPathFeature?.Error;

            Log.Error(error, "Error no controlado");

            await context.Response.WriteAsJsonAsync(new { error = "Error interno del servidor" });
        });
    });

    app.UseStatusCodePages();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Reservation API v1");
            options.EnablePersistAuthorization();
            options.RoutePrefix = string.Empty;
        });
    }

    // Simulación de usuario en entorno Testing
    if (app.Environment.IsEnvironment("Testing"))
    {
        app.Use(async (context, next) =>
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, "User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            context.User = new ClaimsPrincipal(identity);
            await next();
        });
    }

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var adminEmail = config["DefaultAdmin:Email"];
        var adminPassword = config["DefaultAdmin:Password"];
        var adminName = config["DefaultAdmin:Name"];

        // 👇 Enum en vez de string
        if (!context.Users.Any(u => u.Role == UserRole.Admin))
        {
            var admin = new User
            {
                Name = adminName,
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),

                // 👇 Enum, no string
                Role = UserRole.Admin,
                Created = DateTime.UtcNow
            };

            context.Users.Add(admin);
            context.SaveChanges();

            Console.WriteLine("Admin inicial creado.");
        }
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Aplicación iniciada correctamente");
    app.Run();



}
catch (Exception ex)
{
    Log.Fatal(ex, "Fallo crítico al iniciar la aplicación");
}
finally
{
    Log.CloseAndFlush();
}

// testing con WebApplicationFactory
public partial class Program { }

