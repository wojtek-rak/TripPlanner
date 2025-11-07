using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripPlanner.Data.Entities;

namespace TripPlanner.Data;

public class TripPlanDayConfiguration : IEntityTypeConfiguration<TripPlanDay>
{
    public void Configure(EntityTypeBuilder<TripPlanDay> builder)
    {
        builder.ToTable("TripPlanDays");

        // Composite PK already defined via [PrimaryKey] attribute; keep for clarity
        builder.HasKey(x => new { x.TripId, x.DayNumber });

        builder.Property(x => x.DayNumber)
               .IsRequired();

        // Store as varchar(max)
        builder.Property(x => x.Summary)
               .HasColumnType("varchar(100000)")
               .IsUnicode(false);

        builder.Property(x => x.Description)
               .HasColumnType("varchar(100000)")
               .IsUnicode(false);

        // Relationship to Trip (no navigation required on Trip)
        builder.HasOne<Trip>()
               .WithMany() // optional: add a collection navigation on Trip if desired
               .HasForeignKey(x => x.TripId)
               .OnDelete(DeleteBehavior.Cascade);

        // Guard against invalid day numbers
        builder.HasCheckConstraint("CK_TripPlanDay_DayNumber_Positive", "[DayNumber] > 0");
    }
}