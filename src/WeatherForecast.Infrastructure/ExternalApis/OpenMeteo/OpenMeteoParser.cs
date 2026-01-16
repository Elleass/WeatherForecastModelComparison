using System.Text.Json;
using WeatherForecast.Application.Common.Interfaces;
using WeatherForecast.Domain.Builders;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Infrastructure.ExternalApis.OpenMeteo.Models;

namespace WeatherForecast.Infrastructure.ExternalApis.OpenMeteo
{
    /// <summary>
    /// Parser odpowiedzi JSON z API Open-Meteo
    /// Konwertuje JSON → List<Forecast> używając ForecastBuilder
    /// </summary>
    public class OpenMeteoParser : IOpenMeteoParser
    {
        private readonly IForecastBuilder _builder;

        public OpenMeteoParser(IForecastBuilder builder)
        {
            _builder = builder;
        }

        /// <summary>
        /// Główna metoda parsowania
        /// </summary>
        /// <param name="jsonResponse">Raw JSON string z API</param>
        /// <param name="locationId">ID lokalizacji</param>
        /// <param name="modelId">ID modelu pogodowego</param>
        /// <returns>Lista sparsowanych prognoz</returns>
        public List<Forecast> Parse(string jsonResponse, int locationId, int modelId)
        {
            // KROK 1: Deserializuj JSON → ForecastApiResponse
            var payload = JsonSerializer.Deserialize<ForecastApiResponse>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (payload?.Hourly == null)
            {
                throw new InvalidOperationException("OpenMeteo response missing 'hourly' data");
            }

            var hourlyData = payload.Hourly;
            var forecasts = new List<Forecast>();
            var fetchTime = DateTime.UtcNow;

            // KROK 2: Wyciągnij tablice danych z JSON
            var temps = ExtractDoubleArray(hourlyData, "temperature_2m");
            var apparentTemps = ExtractDoubleArray(hourlyData, "apparent_temperature");
            var precip = ExtractDoubleArray(hourlyData, "precipitation");
            var precipProb = ExtractIntArray(hourlyData, "precipitation_probability");
            var windSpeed = ExtractDoubleArray(hourlyData, "wind_speed_10m");
            var humidity = ExtractDoubleArray(hourlyData, "relative_humidity_2m");
            var pressure = ExtractDoubleArray(hourlyData, "surface_pressure");
            var cloudCover = ExtractIntArray(hourlyData, "cloud_cover");
            var visibility = ExtractDoubleArray(hourlyData, "visibility");
            var uvIndex = ExtractIntArray(hourlyData, "uv_index");

            // KROK 3: Znajdź minimalny rozmiar (niektóre pola mogą brakować)
            int count = Math.Min(hourlyData.Time.Count, temps.Count);

            if (count == 0)
            {
                throw new InvalidOperationException("No forecast data points found");
            }

            // KROK 4: Iteruj i buduj obiekty Forecast używając Builder
            for (int i = 0; i < count; i++)
            {
                var forecast = _builder
                    .WithLocation(locationId)
                    .WithModel(modelId)
                    .WithDateTime(fetchTime, DateTime.Parse(hourlyData.Time[i]))
                    .WithTemperature(
                        GetDouble(temps, i),
                        GetDouble(apparentTemps, i))
                    .WithPrecipitation(
                        GetDouble(precip, i),
                        GetInt(precipProb, i))
                    .WithWind(GetDouble(windSpeed, i))
                    .WithAtmosphere(
                        GetDouble(humidity, i),
                        GetDouble(pressure, i),
                        GetInt(cloudCover, i),
                        GetDouble(visibility, i))
                    .WithUvIndex(GetInt(uvIndex, i))
                    .Build();

                forecasts.Add(forecast);

                // Reset builder dla następnej iteracji
                _builder.Reset();
            }

            return forecasts;
        }

        // Metody pomocnicze - ekstrakcja danych z JSON

        /// <summary>
        /// Wyciąga tablicę double z dynamicznego JSON
        /// </summary>
        private List<double> ExtractDoubleArray(HourlyData hourlyData, string fieldName)
        {
            // Próbuj znaleźć pole w AdditionalData (Dictionary)
            if (hourlyData.AdditionalData.TryGetValue(fieldName, out var jsonElement) &&
                jsonElement.ValueKind == JsonValueKind.Array)
            {
                var list = new List<double>();

                // Iteruj po elementach tablicy JSON
                foreach (var el in jsonElement.EnumerateArray())
                {
                    // Sprawdź czy to liczba i deserializuj
                    if (el.ValueKind == JsonValueKind.Number && el.TryGetDouble(out var value))
                    {
                        list.Add(value);
                    }
                    // Pomiń null/inne typy
                }

                return list;
            }

            // Pole nie istnieje  zwróć pustą listę
            return new List<double>();
        }

        /// <summary>
        /// Wyciąga tablicę int z dynamicznego JSON
        /// </summary>
        private List<int> ExtractIntArray(HourlyData hourlyData, string fieldName)
        {
            if (hourlyData.AdditionalData.TryGetValue(fieldName, out var jsonElement) &&
                jsonElement.ValueKind == JsonValueKind.Array)
            {
                var list = new List<int>();

                foreach (var el in jsonElement.EnumerateArray())
                {
                    if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var value))
                    {
                        list.Add(value);
                    }
                }

                return list;
            }

            return new List<int>();
        }

        /// <summary>
        /// Bezpieczne pobranie double z listy (z fallbackiem na 0)
        /// </summary>
        private double GetDouble(List<double> list, int index)
        {
            // Sprawdź czy indeks w zakresie i czy wartość jest liczbą (nie NaN, Infinity)
            if (index < list.Count && double.IsFinite(list[index]))
            {
                return list[index];
            }

            // Fallback - zwróć 0 zamiast crashować
            return 0.0;
        }

        /// <summary>
        /// Bezpieczne pobranie int z listy
        /// </summary>
        private int GetInt(List<int> list, int index)
        {
            return index < list.Count ? list[index] : 0;
        }
    }
}