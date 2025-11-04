using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using EventReservations.Data;
using EventReservations.Models;
using EventReservations.Repositories;
using FluentAssertions;

public class ReservationRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly ReservationRepository _repository;

    public ReservationRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDB")
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ReservationRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddReservationToDatabase()
    {
        // Arrange
        var reservation = new Reservation { ReservationId = 1, UserId = 1, EventId = 2, Status = "Pending", ReservationDate = DateTime.UtcNow };

        // Act
        await _repository.AddAsync(reservation);
        var result = await _context.Reservations.FirstOrDefaultAsync(r => r.ReservationId == 1);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
    }
}

