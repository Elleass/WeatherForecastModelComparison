namespace WeatherForecast.Application.Forecast.Handlers
{
    /// <summary>
    /// WZORZEC ŁAŃCUCH ODPOWIEDZIALNOŚCI
    /// Interfejs dla handlerów przetwarzających żądanie prognozy
    /// </summary>
    public interface IForecastHandler
    {
        /// <summary>
        /// Ustawia następny handler w łańcuchu
        /// </summary>
        IForecastHandler SetNext(IForecastHandler handler);

        /// <summary>
        /// Przetwarza kontekst i przekazuje dalej
        /// </summary>
        Task<ForecastContext> HandleAsync(ForecastContext context);
    }
}