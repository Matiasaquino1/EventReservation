using EventReservations.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EventReservations.Data 
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.SetCommandTimeout(180);
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.Entity is Event || e.Entity is Reservation))
            {
                foreach (var prop in entry.Properties.Where(p => p.Metadata.ClrType == typeof(DateTime)))
                {
                    if (prop.CurrentValue is DateTime dt && dt.Kind != DateTimeKind.Utc)
                        prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
    }





}

