using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DataContext : IdentityDbContext<AppUser>
        {
            public DataContext(DbContextOptions options) : base(options)
            {

            }

            // tables in DB
            public DbSet<Value> Values { get; set; }
            public DbSet<Activity> Activities { get; set; }

            // Seed data
            // Dobar samo za jednostavne primere, bolje u klasi Seed
            protected override void OnModelCreating(ModelBuilder builder)
            {
                // ubaceno zbog IdentityDbContext
                base.OnModelCreating(builder);

                builder.Entity<Value>()
                    .HasData(
                        new Value { Id = 1, Name = "Value 101" },
                        new Value { Id = 2, Name = "Value 102" },
                        new Value { Id = 3, Name = "Value 103" }
                    );
            }
        }
}