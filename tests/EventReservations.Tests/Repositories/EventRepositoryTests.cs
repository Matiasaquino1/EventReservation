using Xunit;
using FluentAssertions;
using EventReservations.Models;
using EventReservations.Repositories;

namespace EventReservationApp.Tests.Repositories
{
    public class EventRepositoryTests : TestBase
    {
        [Fact]
        public async Task AddAsync_ShouldPersistEvent()
        {
            using var context = CreateInMemoryContext();
            var repo = new EventRepository(context);

            var ev = new Event { Title = "Tech Meetup", Status = "Pending", Location = "Resistencia" };
            await repo.AddAsync(ev);
            await context.SaveChangesAsync();

            context.Events.Should().ContainSingle(e => e.Title == "Tech Meetup");
        }
    }
}
