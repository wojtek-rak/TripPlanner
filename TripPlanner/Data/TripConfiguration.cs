using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TripPlanner.Data
{
    public class TripConfiguration : IEntityTypeConfiguration<Trip>
    {
        public void Configure(EntityTypeBuilder<Trip> builder)
        {
            builder.ToTable("Trips");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                   .ValueGeneratedOnAdd();

            // Title, City, Country used by UI/services
            builder.Property(t => t.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(t => t.City)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(t => t.Country)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(t => t.StartDate)
                   .IsRequired();

            builder.Property(t => t.EndDate)
                   .IsRequired();

            builder.Property(t => t.Notes)
                   .HasMaxLength(2000);

            builder.Property(t => t.CoverUrl)
                   .HasMaxLength(1000);

            builder.Property(t => t.IsOwner)
                   .IsRequired()
                   .HasDefaultValue(false);

            // User FK
            builder.Property(t => t.UserId)
                   .IsRequired()
                   .HasMaxLength(450);

            builder.HasIndex(t => t.UserId);

            builder.HasOne(t => t.User)
                   .WithMany(u => u.Trips)
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}