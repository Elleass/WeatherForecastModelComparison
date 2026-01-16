using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecast.Domain.Enums
{
    public enum ForecastSource
    {
        Cache,
        ExternalApi,
        Error
    }
}