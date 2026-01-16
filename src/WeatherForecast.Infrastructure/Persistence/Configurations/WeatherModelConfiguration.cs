using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Infrastructure.Persistence.Configurations
{
    public class WeatherModelConfiguration : IEntityTypeConfiguration<WeatherModel>
    {
        public void Configure(EntityTypeBuilder<WeatherModel> builder)
        {
            builder.ToTable("WeatherModels");
            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(w => w.Provider)
                .HasMaxLength(100);

            builder.Property(w => w.IsActive)
                .HasDefaultValue(true);    // Domyœlnie aktywny

            builder.Property(w => w.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");  // SQLite function

            // Unique - nie mo¿e byæ dwóch modeli o tej samej nazwie
            builder.HasIndex(w => w.Name)
                .IsUnique();
        }
    }
}