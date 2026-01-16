using Microsoft.Extensions.Logging;

namespace WeatherForecast.Application.Forecast.Handlers
{
    /// <summary>
    /// Bazowa klasa dla handlerów
    /// Implementuje logikę łańcucha (SetNext, wywołanie następnego)
    /// </summary>
    public abstract class BaseForecastHandler : IForecastHandler
    {
        private IForecastHandler? _nextHandler;
        protected readonly ILogger Logger;

        protected BaseForecastHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Ustawia następny handler w łańcuchu
        /// </summary>
        public IForecastHandler SetNext(IForecastHandler handler)
        {
            _nextHandler = handler;
            return handler;  // Fluent API - można chainować
        }

        /// <summary>
        /// Główna metoda obsługi
        /// 1. Wywołuje ProcessAsync (konkretna logika handlera)
        /// 2. Jeśli błąd - przerywa łańcuch
        /// 3. Jeśli OK - wywołuje następny handler
        /// </summary>
        public async Task<ForecastContext> HandleAsync(ForecastContext context)
        {
            // Jeśli już jest błąd - przerwij łańcuch
            if (context.HasError)
            {
                Logger.LogWarning("Skipping {Handler} due to existing error:  {Error}",
                    GetType().Name, context.ErrorMessage);
                return context;
            }

            Logger.LogInformation("Executing {Handler}", GetType().Name);

            // Wywołaj konkretną logikę handlera
            await ProcessAsync(context);

            // Jeśli wystąpił błąd - przerwij łańcuch
            if (context.HasError)
            {
                Logger.LogError("Handler {Handler} failed: {Error}",
                    GetType().Name, context.ErrorMessage);
                return context;
            }

            // Wywołaj następny handler (jeśli istnieje)
            if (_nextHandler != null)
            {
                Logger.LogDebug("Passing to next handler:  {NextHandler}",
                    _nextHandler.GetType().Name);
                return await _nextHandler.HandleAsync(context);
            }

            // Koniec łańcucha
            Logger.LogInformation("End of handler chain");
            context.CompletedAt = DateTime.UtcNow;
            return context;
        }

        /// <summary>
        /// Konkretna logika handlera - implementowana przez klasy dziedziczące
        /// </summary>
        protected abstract Task ProcessAsync(ForecastContext context);
    }
}