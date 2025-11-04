using AutoMapper;
using EventReservations.Profiles;
using EventReservations.Repositories;
using EventReservations.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using EventReservations.Data;
using Stripe;
using System.Reflection;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registra Repositories (AddScoped)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
// Registra Services (AddScoped)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEventService, EventReservations.Services.EventService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));


// JWT config
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

// Authentication
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
        RoleClaimType = ClaimTypes.Role // <-- importante para roles
    };
});

// Registrar el servicio de tokens
builder.Services.AddSingleton<IJwtService, JwtService>();

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];


builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (app.Environment.IsEnvironment("Testing"))
{
    app.Use(async (context, next) =>
    {
        // Autenticado siempre con Rol = User para testing
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

app.UseHttpsRedirection();
app.UseAuthentication(); // importante: antes de UseAuthorization
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
