import dayjs from 'dayjs';
import { FORECAST_SERIES } from './chartConfig';

export function toChartData(data = []) {
  return data.map(d => {
    const row = { time: dayjs(d.validDate).format('DD.MM HH:mm') };
    FORECAST_SERIES.forEach(s => {
      // map API field names to your keys
      switch (s.key) {
        case 'temperature':        row.temperature = d.temperature2m; break;
        case 'apparentTemp':       row.apparentTemp = d.apparentTemperature; break;
        case 'precipitation':      row.precipitation = d.precipitation; break;
        case 'precipType':         row.precipType = d.precipitationType; break;
        case 'precipProbability':  row.precipProbability = d.precipitationProbability; break;
        case 'windSpeed':          row.windSpeed = d.windSpeed10m; break;
        case 'humidity':           row.humidity = d.humidity2m; break;
        case 'pressureSurface':    row.pressureSurface = d.pressureSurface; break;
        case 'cloudCover':         row.cloudCover = d.cloudCover; break;
        case 'visibility':         row.visibility = d.visibility; break;
        case 'uvIndex':            row.uvIndex = d.uvIndex; break;
        default: break;
      }
    });
    return row;
  });
}