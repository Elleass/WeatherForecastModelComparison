export const FORECAST_SERIES = [
  { key: 'temperature',      label: 'Temp',          color: '#ff7300', apiKey: 'temperature2m', axis: 'left', unit: '°C' },
  { key: 'apparentTemp',     label: 'Feels',         color: '#387908', apiKey: 'apparentTemperature', axis: 'left', unit: '°C' },
  { key: 'precipitation',    label: 'Precip',        color: '#0088fe', apiKey: 'precipitation', axis: 'left', unit: 'mm' },
  { key: 'precipProbability',label: 'Precip %',      color: '#aa3366', apiKey: 'precipitationProbability', axis: 'left', unit: '%' },
  { key: 'windSpeed',        label: 'Wind',          color: '#00bcd4', apiKey: 'windSpeed10m', axis: 'left', unit: 'm/s' },
  { key: 'humidity',         label: 'Humidity',      color: '#795548', apiKey: 'humidity2m', axis: 'left', unit: '%' },
  { key: 'pressureSurface',  label: 'Pressure',      color: '#607d8b', apiKey: 'pressureSurface', axis: 'right', unit: 'hPa' }, // RIGHT axis
  { key: 'cloudCover',       label: 'Cloud Cover',   color: '#9e9e9e', apiKey: 'cloudCover', axis: 'left', unit: '%' },
  { key: 'visibility',       label: 'Visibility',    color: '#ff9800', apiKey: 'visibility', axis: 'right', unit: 'km' },
  { key: 'uvIndex',          label: 'UV',            color: '#e91e63', apiKey: 'uvIndex', axis: 'left' },
];