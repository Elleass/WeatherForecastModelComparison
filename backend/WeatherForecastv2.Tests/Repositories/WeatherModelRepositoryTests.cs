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

public class WeatherModelRepositoryTests
{
    [Fact]
    public async Task GetModelsAsync_Returns_All_Models_Ordered_By_Name()
    {
        using var context = TestDbContextFactory.Create();

        context.WeatherModel.AddRange(
            new WeatherModel { Id = 2, Name = "GFS" },
            new WeatherModel { Id = 1, Name = "ECMWF" }
        );
        context.SaveChanges();

        var repo = new WeatherModelRepository(context);

        var result = await repo.GetModelsAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("ECMWF"); // ordered alphabetically
        result[1].Name.Should().Be("GFS");
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Model_When_Exists()
    {
        using var context = TestDbContextFactory.Create();

        context.WeatherModel.Add(new WeatherModel
        {
            Id = 10,
            Name = "ICON"
        });
        context.SaveChanges();

        var repo = new WeatherModelRepository(context);

        var result = await repo.GetByIdAsync(10);

        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
        result.Name.Should().Be("ICON");
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Not_Exists()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new WeatherModelRepository(context);

        var result = await repo.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_Returns_Model_When_Exists()
    {
        using var context = TestDbContextFactory.Create();

        context.WeatherModel.Add(new WeatherModel
        {
            Id = 5,
            Name = "ARPEGE"
        });
        context.SaveChanges();

        var repo = new WeatherModelRepository(context);

        var result = await repo.GetByNameAsync("ARPEGE");

        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
        result.Name.Should().Be("ARPEGE");
    }

    [Fact]
    public async Task GetByNameAsync_Returns_Null_When_Not_Exists()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new WeatherModelRepository(context);

        var result = await repo.GetByNameAsync("NonExistingModel");

        result.Should().BeNull();
    }
}
