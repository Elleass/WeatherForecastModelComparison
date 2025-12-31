import { useMemo } from 'react';
import { useForecast } from '../hooks/useForecast';
import { useLocation } from '../hooks/useLocation';
import { useWeatherModels } from '../hooks/useWeatherModels';
import { ForecastChartSection } from '../components/ForecastChartSection';
import ModelComparisonSection from '../components/ModelComparisonSection';
import SearchBar from '../components/SearchBar';
import DarkModeButton from '../components/DarkModeButton';

import '../App.css'
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

  // Prefer API-provided models; fallback to extracting id/name from forecastData
  const models = useMemo(() => {
    if (Array.isArray(modelList) && modelList.length) return modelList;
    return Array.from(
      new Map(
        (forecastData || []).map(f => [
          f.weatherModelId,
          f.weatherModel?.name ?? String(f.weatherModelId),
        ])
      ).entries()
    ).map(([id, name]) => ({ id, name }));
  }, [modelList, forecastData]);

  if (!city) return <div>No city in path</div>;

  return (
    <main>
          <div className="forecast-hero">
            <DarkModeButton />
            <SearchBar/>

      <h1>Location: {city}</h1>
      <h3>Coordinates: {locationData ? `${locationData.lat}, ${locationData.lng}` : '—'}</h3>

      {loadingLocation && <div>Loading location…</div>}
      {errLocation && <div style={{ color: 'crimson' }}>{errLocation}</div>}

      {loadingForecast && <div>Loading forecast…</div>}
      {errForecast && <div style={{ color: 'crimson' }}>{errForecast}</div>}

      {modelListLoading && <div>Loading models…</div>}
      {modelListError && <div style={{ color: 'crimson' }}>{modelListError}</div>}

<div className="forecast-chart-section">
      <ForecastChartSection
        forecastData={forecastData}
        fallbackModels={models}
      />
    </div>

    </div>
<div className="model-comparison-container">
      <ModelComparisonSection
        forecastData={forecastData}
        models={models}
      />
      </div>
      <div className="background-texture"></div>
            <div className="background-texture-color"></div>

    </main>
  );
}