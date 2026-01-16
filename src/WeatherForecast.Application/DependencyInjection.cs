using Microsoft.Extensions.DependencyInjection;
using WeatherForecast.Application.Forecast.Facade;
using WeatherForecast.Application.Forecast.Handlers;
using WeatherForecast.Application.Forecast.Handlers.Implementations;

namespace WeatherForecast.Application
{
    /// <summary>
    /// Extension methods dla rejestracji Application services
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // ═══════════════════════════════════════════════════════
            // FACADE
            // ═══════════════════════════════════════════════════════
            services.AddScoped<IForecastFacade, ForecastFacade>();

            // ═══════════════════════════════════════════════════════
            // HANDLERS (Chain of Responsibility)
            // ═══════════════════════════════════════════════════════
            // WAŻNE: Scoped - każdy request dostaje nowy zestaw handlerów
            services.AddScoped<ValidationHandler>();
            services.AddScoped<GeocodingHandler>();
            services.AddScoped<CacheCheckHandler>();
            services.AddScoped<ApiFetchHandler>();
            services.AddScoped<CacheSaveHandler>();

            return services;
        }
    }
}