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

namespace WeatherForecastv2.Tests.Repositories
{
    public class ForecastRepositoryTests
    {
        private WeatherForecastContext CreateContext() => TestDbContextFactory.Create();

        [Fact]
        public async Task GetRecentForecastAsync_returns_only_recent_and_orders_by_validdate()
        {
            using var ctx = CreateContext();
            var repo = new ForecastRepository(ctx);

            var model = new WeatherModel { Id = 1, Name = "gfs" };
            var location = new Location { Id = 1, Name = "TestCity" };
            ctx.Add(model);
            ctx.Add(location);

            var now = DateTime.UtcNow;
            var older = new Forecast { Id = 1, LocationId = 1, WeatherModelId = 1, FetchDate = now.AddHours(-10), ValidDate = now.AddHours(1) };
            var recent1 = new Forecast { Id = 2, LocationId = 1, WeatherModelId = 1, FetchDate = now.AddHours(-1), ValidDate = now.AddHours(2) };
            var recent2 = new Forecast { Id = 3, LocationId = 1, WeatherModelId = 1, FetchDate = now.AddHours(-0.5), ValidDate = now.AddHours(3) };

            ctx.AddRange(older, recent1, recent2);
            await ctx.SaveChangesAsync();

            var cutoff = now.AddHours(-6);
            var result = await repo.GetRecentForecastAsync(1, cutoff);

            result.Count.Should().Be(2);
            result.Select(f => f.Id).Should().BeInAscendingOrder(); // ordered by ValidDate
        }

        [Fact]
        public async Task GetForecastByLocationAndModels_filters_by_model_names_and_orders()
        {
            using var ctx = CreateContext();
            var repo = new ForecastRepository(ctx);

            var m1 = new WeatherModel { Id = 1, Name = "gfs" };
            var m2 = new WeatherModel { Id = 2, Name = "ecmwf" };
            var loc = new Location { Id = 1, Name = "L" };
            ctx.AddRange(m1, m2, loc);

            var f1 = new Forecast { Id = 1, LocationId = 1, WeatherModelId = 1, ValidDate = DateTime.Parse("2025-01-01T01:00:00Z") };
            var f2 = new Forecast { Id = 2, LocationId = 1, WeatherModelId = 2, ValidDate = DateTime.Parse("2025-01-01T02:00:00Z") };
            var f3 = new Forecast { Id = 3, LocationId = 1, WeatherModelId = 1, ValidDate = DateTime.Parse("2025-01-01T03:00:00Z") };

            ctx.AddRange(f1, f2, f3);
            await ctx.SaveChangesAsync();

            var result = await repo.GetForecastByLocationAndModels(1, new List<string> { "gfs" });

            result.Should().HaveCount(2);
            result.Select(f => f.Id).Should().BeInAscendingOrder();
            result.All(f => f.WeatherModel != null).Should().BeTrue();
        }

        [Fact]
        public async Task SaveForecastAsync_and_HasRecentForecastAsync_behave_correctly()
        {
            using var ctx = CreateContext();
            var repo = new ForecastRepository(ctx);

            var model = new WeatherModel { Id = 1, Name = "gfs" };
            var loc = new Location { Id = 1, Name = "X" };
            ctx.AddRange(model, loc);
            await ctx.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var toSave = new List<Forecast>
            {
                new Forecast { LocationId = 1, WeatherModelId = 1, FetchDate = now, ValidDate = now.AddHours(1) },
                new Forecast { LocationId = 1, WeatherModelId = 1, FetchDate = now, ValidDate = now.AddHours(2) }
            };

            await repo.SaveForecastAsync(toSave);

            var hasRecent = await repo.HasRecentForecastAsync(1, now.AddMinutes(-1));
            hasRecent.Should().BeTrue();

            var saved = await repo.GetRecentForecastAsync(1, now.AddMinutes(-1));
            saved.Should().HaveCount(2);
        }

        [Fact]
        public async Task DeleteOldForecastsAsync_deletes_older_than_cutoff()
        {
            using var ctx = CreateContext();
            var repo = new ForecastRepository(ctx);

            var model = new WeatherModel { Id = 1, Name = "gfs" };
            var loc = new Location { Id = 1, Name = "X" };
            ctx.AddRange(model, loc);

            var now = DateTime.UtcNow;
            var old = new Forecast { Id = 1, LocationId = 1, WeatherModelId = 1, FetchDate = now.AddDays(-10) };
            var recent = new Forecast { Id = 2, LocationId = 1, WeatherModelId = 1, FetchDate = now.AddDays(-1) };
            ctx.AddRange(old, recent);
            await ctx.SaveChangesAsync();

            await repo.DeleteOldForecastsAsync(now.AddDays(-5));

            var all = await ctx.Forecast.ToListAsync();
            all.Should().HaveCount(1);
            all.Single().Id.Should().Be(2);
        }

        [Fact]
        public async Task GetLatestForecastAsync_returns_latest_by_fetchdate()
        {
            using var ctx = CreateContext();
            var repo = new ForecastRepository(ctx);

            var model = new WeatherModel { Id = 1, Name = "gfs" };
            var loc = new Location { Id = 1, Name = "X" };
            ctx.AddRange(model, loc);

            var now = DateTime.UtcNow;
            var f1 = new Forecast { Id = 1, LocationId = 1, WeatherModelId = 1, FetchDate = now.AddHours(-2) };
            var f2 = new Forecast { Id = 2, LocationId = 1, WeatherModelId = 1, FetchDate = now.AddHours(-1) };
            ctx.AddRange(f1, f2);
            await ctx.SaveChangesAsync();

            var latest = await repo.GetLatestForecastAsync(1, 1);
            latest.Should().NotBeNull();
            latest!.Id.Should().Be(2);
        }
    }
}
