namespace WeatherForecast.Application.Forecast.Facade
{
    /// <summary>
    /// WZORZEC FASADA
    /// Uproszczony interfejs do pobierania prognoz
    /// Ukrywa złożoność Chain of Responsibility
    /// </summary>
    public interface IForecastFacade
    {
        /// <summary>
        /// Pobiera prognozy dla miasta
        /// Automatycznie:  walidacja → geocoding → cache → API
        /// </summary>
        /// <param name="city">Nazwa miasta</param>
        /// <returns>Wynik z prognozami lub błędem</returns>
        Task<ForecastResult> GetForecastAsync(string city);
    }
}