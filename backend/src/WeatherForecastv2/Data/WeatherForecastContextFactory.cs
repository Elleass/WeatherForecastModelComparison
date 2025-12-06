using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WeatherForecastv2.Data
{
	public class WeatherForecastContextFactory : IDesignTimeDbContextFactory<WeatherForecastContext>
	{
		public WeatherForecastContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<WeatherForecastContext>();

			// Use the connection string directly for design-time
			optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=WeatherForecast;Trusted_Connection=true;MultipleActiveResultSets=true");

			return new WeatherForecastContext(optionsBuilder.Options);
		}
	}
}