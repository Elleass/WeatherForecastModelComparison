using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WeatherForecastv2.Data;
using WeatherForecastv2.Models;
using Xunit;

namespace WeatherForecastv2.Tests.TestHelpers;

public class WeatherForecastContextTests
{
    [Fact]
    public void Can_Insert_And_Retrieve_Location()
    {
        using var context = TestDbContextFactory.Create();
        context.Database.EnsureCreated();

        var location = new Location
        {
            Name = "TestCity",
            Latitude = 10.1234567,
            Longitude = 20.1234567
        };

        context.Location.Add(location);
        context.SaveChanges();

        var result = context.Location.FirstOrDefault();

        result.Should().NotBeNull();
        result!.Name.Should().Be("TestCity");
        result.Latitude.Should().Be(10.1234567);
        result.Longitude.Should().Be(20.1234567);
    }

    [Fact]
    public void Can_Insert_Forecast_With_Relationships()
    {
        using var context = TestDbContextFactory.Create();
        context.Database.EnsureCreated();

        var model = new WeatherModel { Name = "GFS" };
        var location = new Location { Name = "London" };

        context.Add(model);
        context.Add(location);
        context.SaveChanges();

        var forecast = new Forecast
        {
            LocationId = location.Id,
            WeatherModelId = model.Id,
            Temperature2m = 5.55
        };

        context.Add(forecast);
        context.SaveChanges();

        var dbForecast = context.Forecast
            .Include(f => f.Location)
            .Include(f => f.WeatherModel)
            .FirstOrDefault(f => f.Id == forecast.Id);

        dbForecast.Should().NotBeNull();
        dbForecast!.Location.Should().NotBeNull();
        dbForecast.WeatherModel.Should().NotBeNull();

        dbForecast.Location!.Name.Should().Be("London");
        dbForecast.WeatherModel!.Name.Should().Be("GFS");
    }

    [Fact]
    public void Deleting_Location_Should_Cascade_Delete_Forecasts()
    {
        using var context = TestDbContextFactory.Create();
        context.Database.EnsureCreated();

        var location = new Location { Name = "Warsaw" };
        var model = new WeatherModel { Name = "ECMWF" };

        context.Add(location);
        context.Add(model);
        context.SaveChanges();

        var forecast = new Forecast
        {
            LocationId = location.Id,
            WeatherModelId = model.Id
        };

        context.Add(forecast);
        context.SaveChanges();

        // Act — delete the location
        context.Remove(location);
        context.SaveChanges();

        // Assert — forecast should be deleted
        context.Forecast.Any().Should().BeFalse("cascade delete should remove dependent forecasts");
    }

    [Fact]
    public void Decimal_Precision_Is_Configured_Correctly()
    {
        using var context = TestDbContextFactory.Create();
        context.Database.EnsureCreated();

        var entity = context.Model.FindEntityType(typeof(Forecast))!;
        var tempProp = entity.FindProperty(nameof(Forecast.Temperature2m))!;

        tempProp.GetPrecision().Should().Be(5);
        tempProp.GetScale().Should().Be(2);
    }
}
