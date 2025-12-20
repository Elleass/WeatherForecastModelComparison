import { useEffect, useMemo, useState } from 'react';
import Chart from './Chart';
import { ModelSelector } from './ModelSelector';

export function ForecastChartSection({ forecastData, fallbackModels }) {
  // Use fallback model info if API list not provided
  const [selectedModel, setSelectedModel] = useState(null);

  // Build unique models from forecast data as fallback [{id,name}]
  const forecastModels = useMemo(() => {
    return Array.from(
      new Map(
        (forecastData || []).map(f => [
          f.weatherModelId,
          f.weatherModel?.name ?? String(f.weatherModelId),
        ])
      ).entries()
    ).map(([id, name]) => ({ id, name }));
  }, [forecastData]);

  const models = fallbackModels?.length ? fallbackModels : forecastModels;

  useEffect(() => {
    if (selectedModel == null && models.length) {
      setSelectedModel(models[0].id);
    }
  }, [models, selectedModel]);

  const selectedModelObj = useMemo(
  () => models.find(m => m.id === selectedModel) || null,
  [models, selectedModel]
);

const series = useMemo(
  () =>
    selectedModel == null
      ? []
      : (forecastData || []).filter(f => f.weatherModelId === selectedModel),
  [forecastData, selectedModel]
);
  return (
    <section>
      <ModelSelector
        models={models}
        selected={selectedModel}
        onChange={setSelectedModel}
        disabled={!models.length}
      />
    <h3>Selected model: {selectedModelObj?.name ?? '—'}</h3> 
         <Chart data={series} />
    </section>
  );
}