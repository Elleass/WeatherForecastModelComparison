import { useForecast } from '../hooks/useForecast';
import { useLocation } from '../hooks/useLocation';
import { useWeatherModels } from '../hooks/useWeatherModels';
import { ForecastChartSection } from '../components/ForecastChartSection';
import MultiModelSelector from '../components/MultiModelSelector';

export default function LocationLayout() {
  const {
    city,
    data: locationData,
    err: errLocation,
    loading: loadingLocation,
  } = useLocation();

  const {
    data: forecastData = [],
    err: errForecast,
    loading: loadingForecast,
  } = useForecast();

  const {
    models: modelList,
    loading: modelListLoading,
    err: modelListError,
  } = useWeatherModels();

  if (!city) return <div>No city in path</div>;

  return (
    <main>
      <h1>Location: {city}</h1>
      <h3>Coordinates: {locationData ? `${locationData.lat}, ${locationData.lng}` : '—'}</h3>

      {loadingLocation && <div>Loading location…</div>}
      {errLocation && <div style={{ color: 'crimson' }}>{errLocation}</div>}

      {loadingForecast && <div>Loading forecast…</div>}
      {errForecast && <div style={{ color: 'crimson' }}>{errForecast}</div>}

      {modelListLoading && <div>Loading models…</div>}
      {modelListError && <div style={{ color: 'crimson' }}>{modelListError}</div>}

      <ForecastChartSection
        forecastData={forecastData}
        fallbackModels={modelList}
      />
      <MultiModelSelector/>

            <MultiModelSelector
        forecastData={forecastData}
        models={modelList}
      />


    </main>
  );
}