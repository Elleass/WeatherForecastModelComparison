using FluentAssertions;
using Microsoft.Extensions.Configuration;
using WeatherForecastv2.Data;
using WeatherForecastv2.Services;
using WeatherForecastv2.Tests.TestHelpers;
using Xunit;

namespace WeatherForecastv2.Tests.Services;

public class GeoServiceTests
{
    private static IGeocodingService CreateService(
        WeatherForecastContext context,
        HttpClient httpClient)
    {
        var config = new ConfigurationBuilder().Build();
        return new GeoService(context, config, httpClient);
    }

    [Fact]
    public async Task GetCoordinatesAsync_Returns_Cached_Location_When_Exists()
    {
        using var context = TestDbContextFactory.Create();

        context.Location.Add(new WeatherForecastv2.Models.Location
        {
            Name = "Warsaw",
            Latitude = 52.23,
            Longitude = 21.01
        });
        context.SaveChanges();

        var httpClient = MockHttpMessageHandler.CreateClient("{}");

        var service = CreateService(context, httpClient);

        var (lat, lng) = await service.GetCoordinatesAsync("Warsaw");

        lat.Should().Be(52.23);
        lng.Should().Be(21.01);
    }

    [Fact]
    public async Task GetCoordinatesAsync_Fetches_From_Api_When_Not_In_Db()
    {
        using var context = TestDbContextFactory.Create();

        string jsonResponse = """
        {
            "results": [
                { "latitude": 10.5, "longitude": 20.7 }
            ]
        }
        """;

        var httpClient = MockHttpMessageHandler.CreateClient(jsonResponse);

        var service = CreateService(context, httpClient);

        var (lat, lng) = await service.GetCoordinatesAsync("NewCity");

        lat.Should().Be(10.5);
        lng.Should().Be(20.7);

        var saved = context.Location.FirstOrDefault(l => l.Name == "NewCity");
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCoordinatesAsync_Throws_When_Api_Returns_Empty()
    {
        using var context = TestDbContextFactory.Create();

        var httpClient = MockHttpMessageHandler.CreateClient("""{ "results": [] }""");

        var service = CreateService(context, httpClient);

        Func<Task> act = async () => await service.GetCoordinatesAsync("GhostTown");

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Coordinates not found*");
    }

    [Fact]
    public async Task GetCoordinatesAsync_Throws_When_City_Is_Null_Or_Empty()
    {
        using var context = TestDbContextFactory.Create();

        var httpClient = MockHttpMessageHandler.CreateClient("{}");

        var service = CreateService(context, httpClient);

        Func<Task> act = async () => await service.GetCoordinatesAsync("");

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
