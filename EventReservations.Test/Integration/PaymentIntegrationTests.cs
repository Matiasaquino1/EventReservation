using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EventReservations.Test
{
    public class PaymentIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public PaymentIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();  // Crea el cliente con el servidor de pruebas
        }

        [Fact]
        public async Task ProcessPayment_ShouldReturnSuccess()
        {
            // Arrange: Body con datos de PaymentRequestDto
            var content = new StringContent(
                "{\"ReservationId\": 1, \"Amount\": 50.00, \"Currency\": \"usd\", \"PaymentMethodId\": \"pm_card_visa\"}",
                Encoding.UTF8,
                "application/json"
            );

            // Act: Hace la petición POST a /api/payments/process
            var response = await _client.PostAsync("/api/payments/process", content);

            // Assert: Espera 200 OK (según el controlador)
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Opcional: Verifica el contenido de la respuesta
            var responseBody = await response.Content.ReadAsStringAsync();
            responseBody.Should().Contain("Pago procesado correctamente");  // Basado en el mensaje del controlador
        }
    }
}


