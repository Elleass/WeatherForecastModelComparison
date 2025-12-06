using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WeatherForecastv2.Models;
using WeatherForecastv2.Repositories;
using WeatherForecastv2.Data;
using WeatherForecastv2.Tests.TestHelpers;
using Xunit;

namespace WeatherForecastv2.Tests.Repositories;

public class LocationRepositoryTests
{
    [Fact]
    public async Task GetByCityName_Returns_Location_When_Exists()
    {
        using var context = TestDbContextFactory.Create();

        context.Location.Add(new Location
        {
            Name = "Berlin",
            Latitude = 52.52,
            Longitude = 13.40
        });
        context.SaveChanges();

        var repo = new LocationRepository(context);

        var result = await repo.GetByCityNameAsync("Berlin");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Berlin");
        result.Latitude.Should().Be(52.52);
        result.Longitude.Should().Be(13.40);
    }

    [Fact]
    public async Task GetByCityName_Returns_Null_When_Not_Exists()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new LocationRepository(context);

        var result = await repo.GetByCityNameAsync("UnknownCity");

        result.Should().BeNull();
    }
}
