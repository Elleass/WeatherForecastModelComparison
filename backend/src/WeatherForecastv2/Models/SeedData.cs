using Microsoft.EntityFrameworkCore;
using WeatherForecastv2.Data;

namespace WeatherForecastv2.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new WeatherForecastContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<WeatherForecastContext>>()))
            {
                if (context == null || context.Model == null)
                {
                    throw new ArgumentNullException("Null WeatherForecastContext");
                }

                if (context.WeatherModel.Any())
                {
                    return;
                }

                context.WeatherModel.AddRange(
                    new WeatherModel
                    {

                    }

                    );
                context.SaveChanges();
            }
        }
    }
}
