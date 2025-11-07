using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Data.Entities;

namespace TripPlanner.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Trip> Trips { get; set; } = null!;
        public DbSet<TripPlanDay> TripPlanDays { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new TripConfiguration()); 
            modelBuilder.ApplyConfiguration(new TripPlanDayConfiguration()); 
            modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration()); 
        }
    }
}
