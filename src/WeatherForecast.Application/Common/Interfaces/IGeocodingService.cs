namespace WeatherForecast.Application.Common.Interfaces
{
    /// <summary>
    /// Serwis geokodowania - konwertuje nazwy miast na współrzędne
    /// </summary>
    public interface IGeocodingService
    {
        /// <summary>
        /// Pobiera współrzędne geograficzne dla miasta
        /// </summary>
        /// <param name="city">Nazwa miasta</param>
        /// <returns>Tuple (latitude, longitude) lub null jeśli nie znaleziono</returns>
        Task<(double lat, double lng)?> GetCoordinatesAsync(string city);
    }
}