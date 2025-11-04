using Xunit;
using Moq;
using System.Threading.Tasks;
using EventReservations.Services;
using EventReservations.Repositories;
using EventReservations.Models;
using FluentAssertions;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _mockRepo;
    private readonly ReservationService _service;

    public ReservationServiceTests()
    {
        _mockRepo = new Mock<IReservationRepository>();
        _service = new ReservationService(_mockRepo.Object);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldReturnCreatedReservation()
    {
        // Arrange
        var reservation = new Reservation { ReservationId = 1, UserId = 1, EventId = 10, Status = "Pending" };
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Reservation>())).ReturnsAsync(reservation);

        // Act
        var result = await _service.CreateReservationAsync(reservation);

        // Assert
        result.Should().NotBeNull();
        result.ReservationId.Should().Be(1);
        result.Status.Should().Be("Pending");

        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Once);
    }
}

