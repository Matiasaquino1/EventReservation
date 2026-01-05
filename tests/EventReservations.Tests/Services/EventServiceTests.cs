using Xunit;
using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using EventReservations.Models;
using EventReservations.Services;
using EventReservations.Repositories;
using System.Collections.Generic;

namespace EventReservationApp.Tests.Services
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _eventRepoMock;
        private readonly EventService _service;

        public EventServiceTests()
        {
            _eventRepoMock = new Mock<IEventRepository>();
            _service = new EventService(_eventRepoMock.Object);
        }

        [Fact]
        public async Task GetAllEventsAsync_ShouldReturnEvents()
        {
            var events = new List<Event> { new Event { EventId = 1, Title = "Test Event" } };
            _eventRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(events);

            var result = await _service.GetAllAsync();

            result.Should().HaveCount(1);
            result.Should().ContainSingle(e => e.Title == "Test Event");
        }

        [Fact]
        public async Task CreateEventAsync_ShouldCallRepositoryOnce()
        {
            var newEvent = new Event { Title = "Concert" };

            await _service.CreateEventAsync(newEvent);

            _eventRepoMock.Verify(r => r.AddAsync(newEvent), Times.Once);
        }

        [Fact]
        public async Task ForceConfirmEventAsync_ShouldUpdateStatus()
        {
            // Arrange
            var mockRepo = new Mock<IEventRepository>();
            var testEvent = new Event { EventId = 1, Status = "Pending" };

            mockRepo.Setup(r => r.ForceConfirmEventAsync(1))
                    .ReturnsAsync(testEvent);

            var service = new EventService(mockRepo.Object);

            // Act
            var result = await service.ForceConfirmEventAsync(1);

            // Assert
            result.Status.Should().Be("Confirmed");
            mockRepo.Verify(r => r.UpdateAsync(It.Is<Event>(e => e.Status == "Confirmed")), Times.Once);
        }


        [Fact]
        public async Task GetEventById_ShouldReturnNull_WhenNotFound()
        {
            _eventRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Event?)null);

            var result = await _service.GetEventAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteEvent_ShouldCallRepositoryDeleteOnce()
        {
            await _service.DeleteEventAsync(2);
            _eventRepoMock.Verify(r => r.DeleteAsync(2), Times.Once);
        }
    }
}
