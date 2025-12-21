import { useEffect, useMemo, useState } from 'react';
import ModelComparisonChart from './ModelComparisonChart';
import MultiModelSelector from './MultiModelSelector';
import { ForecastTypeSelector } from './ForecastTypeSelector';
import { FORECAST_SERIES } from '../config/chartConfig';
import { toChartData } from '../config/toChartData';

/**
 * Compute integer domain and ticks (same util as before) — returns { domain, ticks }
 */
function computeIntegerDomainAndTicks(chartData, keys, tickCount = 5) {
  if (!keys || !keys.length) return { domain: ['auto', 'auto'], ticks: null };

  let min = Infinity;
  let max = -Infinity;
  for (const row of chartData || []) {
    for (const k of keys) {
      const v = row[k];
      if (v == null || Number.isNaN(Number(v))) continue;
      const n = Number(v);
      if (n < min) min = n;
      if (n > max) max = n;
    }
  }
  if (min === Infinity) return { domain: ['auto', 'auto'], ticks: null };

  const rawRange = max - min || 1;
  const approxStep = Math.max(1, Math.ceil(rawRange / Math.max(1, tickCount - 1)));

  const magnitude = Math.pow(10, Math.floor(Math.log10(approxStep)));
  const residual = approxStep / magnitude;
  const niceMultiplier = residual <= 1 ? 1 : residual <= 2 ? 2 : residual <= 5 ? 5 : 10;
  const step = magnitude * niceMultiplier;

  const niceMin = Math.floor(min / step) * step;
  const niceMax = Math.ceil(max / step) * step;

  const ticks = [];
  const maxIter = Math.round((niceMax - niceMin) / step) + 1;
  for (let i = 0; i < maxIter; i++) {
    ticks.push(Math.round(niceMin + i * step));
  }

  return { domain: [niceMin, niceMax], ticks };
}

export default function ModelComparisonSection({ forecastData = [], models = [] }) {
  const [selectedModels, setSelectedModels] = useState(new Set());
  const [visibleSeries, setVisibleSeries] = useState(() => new Set(['temperature']));

  const modelsById = useMemo(() => Object.fromEntries((models || []).map(m => [m.id, m])), [models]);

  useEffect(() => {
    if (!selectedModels.size && models.length) {
      setSelectedModels(new Set([models[0].id]));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [models]);

  const toggleModel = (id, checked) => {
    setSelectedModels(prev => {
      const next = new Set(prev);
      if (checked) next.add(id);
      else next.delete(id);
      return next;
    });
  };

  const toggleSeries = (key, checked) => {
    setVisibleSeries(prev => {
      const next = new Set(prev);
      if (checked) next.add(key);
      else next.delete(key);
      return next;
    });
  };

  // Group forecast data by modelId once
  const seriesByModel = useMemo(() => {
    const map = new Map();
    (forecastData || []).forEach(f => {
      const id = f.weatherModelId;
      if (!map.has(id)) map.set(id, []);
      map.get(id).push(f);
    });
    return map;
  }, [forecastData]);

  const selectedModelIds = useMemo(() => Array.from(selectedModels), [selectedModels]);

  // visible series config from FORECAST_SERIES
  const visibleSeriesConfig = useMemo(
    () => FORECAST_SERIES.filter(s => visibleSeries.has(s.key)),
    [visibleSeries]
  );

  const leftKeys = useMemo(
    () => visibleSeriesConfig.filter(s => (s.axis ?? 'left') === 'left').map(s => s.key),
    [visibleSeriesConfig]
  );
  const rightKeys = useMemo(
    () => visibleSeriesConfig.filter(s => s.axis === 'right').map(s => s.key),
    [visibleSeriesConfig]
  );

  // Compute shared units for axes (if all visible left-series share same non-empty unit, show it)
  const leftUnit = useMemo(() => {
    const units = new Set(visibleSeriesConfig.filter(s => (s.axis ?? 'left') === 'left').map(s => s.unit).filter(Boolean));
    return units.size === 1 ? units.values().next().value : null;
  }, [visibleSeriesConfig]);

  const rightUnit = useMemo(() => {
    const units = new Set(visibleSeriesConfig.filter(s => s.axis === 'right').map(s => s.unit).filter(Boolean));
    return units.size === 1 ? units.values().next().value : null;
  }, [visibleSeriesConfig]);

  // Collect all rows from selected models and map to chart rows (keys from FORECAST_SERIES)
  const allSelectedChartRows = useMemo(() => {
    const out = [];
    for (const id of selectedModelIds) {
      const rows = seriesByModel.get(id) || [];
      rows.forEach(r => {
        const obj = { time: r.validDate ?? r.time ?? r.date ?? r.x };
        FORECAST_SERIES.forEach(s => {
          obj[s.key] = r[s.apiKey] ?? r[s.key] ?? null;
        });
        out.push(obj);
      });
    }
    return out;
  }, [selectedModelIds, seriesByModel]);

  // Compute shared domains/ticks across selected models (for left/right)
  const leftDomainTicks = useMemo(() => computeIntegerDomainAndTicks(allSelectedChartRows, leftKeys, 5), [allSelectedChartRows, leftKeys]);
  const rightDomainTicks = useMemo(() => computeIntegerDomainAndTicks(allSelectedChartRows, rightKeys, 5), [allSelectedChartRows, rightKeys]);

  // syncId for charts so tooltips/active items are synced across all generated charts
  const SYNC_ID = 'model-comparison-sync';

  return (
    <section>
      <h3>Compare models</h3>

      <MultiModelSelector
        models={models}
        selectedModels={selectedModels}
        onToggleModel={toggleModel}
      />

      <ForecastTypeSelector
        options={FORECAST_SERIES}
        selectedKeys={visibleSeries}
        onToggle={toggleSeries}
      />
      <div className="comparison-chart-container">


        {selectedModelIds.length === 0 && <div>No models selected</div>}

        {selectedModelIds.map(modelId => {
          const series = seriesByModel.get(modelId) || [];
          const title = modelsById[modelId]?.name ?? `Model ${modelId}`;
          return (

            <ModelComparisonChart
              key={modelId}
              data={series}
              visibleKeys={visibleSeries}
              title={title}
              leftDomainTicks={leftDomainTicks}
              rightDomainTicks={rightDomainTicks}
              leftUnit={leftUnit}
              rightUnit={rightUnit}
              syncId={SYNC_ID}
            />
          );
        })}
      </div>

    </section>
  );
}