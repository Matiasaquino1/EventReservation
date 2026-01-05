using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using EventReservations.Data;

namespace EventReservationApp.Tests
{
    public abstract class TestBase
    {
        protected ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // nuevo por test
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}


