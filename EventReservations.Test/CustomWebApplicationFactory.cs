using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;  // Añade paquete NuGet: Moq
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EventReservations.Services;  // Para IPaymentService
using EventReservations.Models;   // Para Payment

namespace EventReservations.Test
{
    // Clase para contener el rol de test (se registra como singleton)
    public class TestAuthOptions
    {
        public string Role { get; set; } = "Admin";
    }

    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public string TestRole { get; set; } = "Admin";  // Rol predeterminado; cambia en tests

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                // Remueve la autenticación existente para evitar conflictos con la real
                var authDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthenticationService));
                if (authDescriptor != null)
                {
                    services.Remove(authDescriptor);
                }

                // Registra TestAuthOptions como singleton con el rol dinámico
                services.AddSingleton(new TestAuthOptions { Role = TestRole });

                // Agrega autenticación fake (ASP.NET Core creará TestAuthHandler automáticamente)
                services.AddAuthentication("TestAuth")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "TestAuth", options => { });

                // Configura "TestAuth" como esquema predeterminado
                services.Configure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = "TestAuth";
                    options.DefaultChallengeScheme = "TestAuth";
                });

                // Mockea IPaymentService para evitar llamadas reales a Stripe
                var mockPaymentService = new Mock<IPaymentService>();
                mockPaymentService.Setup(s => s.ProcessPaymentAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(new Payment
                    {
                        PaymentId = 1,
                        ReservationId = 1,
                        Status = "Succeeded",
                        Amount = 50.00m,
                        PaymentDate = DateTime.UtcNow,
                        StripePaymentIntentId = "pi_test_123"
                    });
                services.AddSingleton(mockPaymentService.Object);

                // Registra otros servicios si faltan (ej. IMapper, repositorios) - ajusta según tu setup
                // services.AddScoped<IReservationService, ReservationService>();  // Si necesitas
            });

            // Opcional: Añade logging para depurar
            builder.ConfigureLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Debug));
        }
    }

    // Handler que autentica automáticamente con el rol inyectado
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly TestAuthOptions _testAuthOptions;

        public TestAuthHandler(TestAuthOptions testAuthOptions, Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
            Microsoft.Extensions.Logging.ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder,
            Microsoft.AspNetCore.Authentication.ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _testAuthOptions = testAuthOptions;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, _testAuthOptions.Role)  // Usa ClaimTypes.Role para roles estándar
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestAuth");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}