using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Infrastructure.Persistence.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.ToTable("Locations");
            builder.HasKey(l => l.Id);

            // Name jest wymagane (NOT NULL) i max 200 znaków
            builder.Property(l => l.Name)
                .IsRequired()              // NOT NULL
                .HasMaxLength(200);        // VARCHAR(200)

            // Wspó³rzêdne geograficzne z dok³adnoœci¹ 5 miejsc po przecinku
            // 52.23040 (Warszawa)  HasPrecision(10, 5)
            builder.Property(l => l.Latitude)
                .HasPrecision(10, 5);      // -90.00000 do 90.00000

            builder.Property(l => l.Longitude)
                .HasPrecision(10, 5);      // -180.00000 do 180.00000

            // UNIQUE INDEX - nie mo¿e byæ dwóch miast o tej samej nazwie
            builder.HasIndex(l => l.Name)
                .IsUnique();               // UNIQUE constraint
        }
    }
}