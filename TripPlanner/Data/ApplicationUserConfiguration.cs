using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TripPlanner.Data
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Keep the Identity default table name to avoid creating a new table
            builder.ToTable("AspNetUsers");

            // ApiKey: allow reasonably long keys, store as non-required text
            builder.Property(u => u.ApiKey)
                   .HasMaxLength(1000)
                   .IsUnicode(false)
                   .HasColumnName("ApiKey");

            // Endpoint: store as URL, allow up to 1000 chars
            builder.Property(u => u.Endpoint)
                   .HasMaxLength(1000)
                   .HasColumnName("Endpoint");

            // The following two are stored as dedicated columns.
            builder.Property(u => u.OpenAIModel)
                   .HasMaxLength(200)
                   .HasColumnName("OpenAIModel");

            builder.Property(u => u.IsAzureOpenAI)
                   .HasColumnName("IsAzureOpenAI");

            // These two are computed from existing columns, do not map separately.
            builder.Ignore(u => u.OpenAIApiKey);
            builder.Ignore(u => u.OpenAIEndpoint);
        }
    }
}