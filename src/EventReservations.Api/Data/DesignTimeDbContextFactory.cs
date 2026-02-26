using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using EventReservations.Data;

namespace EventReservations.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=eventreservations;Username=postgres;Password=2092")
            .Options;

        return new ApplicationDbContext(options);
    }
}
