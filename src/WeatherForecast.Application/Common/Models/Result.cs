namespace WeatherForecast.Application.Common.Models
{
    /// <summary>
    /// Generyczny wynik operacji - sukces lub błąd
    /// Używany zamiast rzucania wyjątków
    /// </summary>
    /// <typeparam name="T">Typ danych zwracanych w przypadku sukcesu</typeparam>
    public class Result<T>
    {
        // Prywatny konstruktor - wymusza użycie factory methods
        private Result(bool isSuccess, T? value, string? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        /// <summary>
        /// Czy operacja zakończyła się sukcesem? 
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Czy operacja zakończyła się błędem?
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Dane zwrócone w przypadku sukcesu
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Komunikat błędu (jeśli IsFailure = true)
        /// </summary>
        public string? Error { get; }

        // ═══════════════════════════════════════════════════════
        // FACTORY METHODS - tworzenie wyników
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Tworzy wynik sukcesu z danymi
        /// </summary>
        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null);
        }

        /// <summary>
        /// Tworzy wynik błędu z komunikatem
        /// </summary>
        public static Result<T> Failure(string error)
        {
            return new Result<T>(false, default, error);
        }

        // ═══════════════════════════════════════════════════════
        // HELPER METHODS - wygodne operacje
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Mapuje wartość do innego typu (jeśli sukces)
        /// </summary>
        public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            if (IsFailure)
                return Result<TNew>.Failure(Error!);

            return Result<TNew>.Success(mapper(Value!));
        }

        /// <summary>
        /// Wykonuje akcję jeśli sukces
        /// </summary>
        public Result<T> OnSuccess(Action<T> action)
        {
            if (IsSuccess)
                action(Value!);

            return this;
        }

        /// <summary>
        /// Wykonuje akcję jeśli błąd
        /// </summary>
        public Result<T> OnFailure(Action<string> action)
        {
            if (IsFailure)
                action(Error!);

            return this;
        }

        /// <summary>
        /// Zwraca wartość lub domyślną jeśli błąd
        /// </summary>
        public T GetValueOrDefault(T defaultValue)
        {
            return IsSuccess ? Value! : defaultValue;
        }

        public override string ToString()
        {
            return IsSuccess
                ? $"Success: {Value}"
                : $"Failure: {Error}";
        }
    }
}