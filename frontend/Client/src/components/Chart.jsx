import { useMemo, useState } from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  Legend,
  ResponsiveContainer,
  CartesianGrid,
} from 'recharts';
import { FORECAST_SERIES } from '../config/chartConfig';
import { toChartData } from '../config/toChartData';
import { ForecastTypeSelector } from './ForecastTypeSelector';
import CustomToolTip from './CustomToolTip';

/**
 * Compute integer domain and tick array for given keys
 * - chartData: array of rows
 * - keys: array of numeric keys to consider
 * - tickCount: desired number of ticks (approx)
 *
 * Returns { domain: [min, max], ticks: [...] } with integer values.
 */
function computeIntegerDomainAndTicks(chartData, keys, tickCount = 5) {
  if (!keys || !keys.length) return { domain: ['auto', 'auto'], ticks: null };

  let min = Infinity;
  let max = -Infinity;
  for (const row of chartData) {
    for (const k of keys) {
      const v = row[k];
      if (v == null || Number.isNaN(Number(v))) continue;
      const n = Number(v);
      if (n < min) min = n;
      if (n > max) max = n;
    }
  }
  if (min === Infinity) return { domain: ['auto', 'auto'], ticks: null };

  // If values are already integers and small range, keep it simple
  // Compute raw range and integer step (ceil to integer)
  const rawRange = max - min;
  // ensure at least 1 to avoid zero step
  const approxStep = Math.max(1, Math.ceil(rawRange / (tickCount - 1)));

  // Optional: round step to "nice" integer (1,2,5,10,...) for prettier ticks
  // We'll coerce approxStep to nearest nice integer multiplier:
  const magnitude = Math.pow(10, Math.floor(Math.log10(approxStep)));
  const residual = approxStep / magnitude;
  const niceMultiplier = residual <= 1 ? 1 : residual <= 2 ? 2 : residual <= 5 ? 5 : 10;
  const step = magnitude * niceMultiplier;

  // Snap min down and max up to multiples of step (use Math.floor/ceil)
  const niceMin = Math.floor(min / step) * step;
  const niceMax = Math.ceil(max / step) * step;

  // Build ticks array from niceMin to niceMax by step (ensure integers)
  const ticks = [];
  for (let v = niceMin; v <= niceMax + 1e-9; v += step) {
    // use Math.round to avoid floating point artifacts
    ticks.push(Math.round(v));
  }

  return { domain: [niceMin, niceMax], ticks };
}

export default function Chart({ data = [] }) {
  const chartData = useMemo(() => toChartData(data), [data]);
  const [visible, setVisible] = useState(() => new Set(['temperature'])); // default visible

  const handleToggle = (key, checked) => {
    setVisible(prev => {
      const next = new Set(prev);
      if (checked) next.add(key);
      else next.delete(key);
      return next;
    });
  };

  // determine which visible series live on left vs right axis
  const visibleSeries = useMemo(() => FORECAST_SERIES.filter(s => visible.has(s.key)), [visible]);

  const leftKeys = useMemo(() => visibleSeries.filter(s => (s.axis ?? 'left') === 'left').map(s => s.key), [visibleSeries]);
  const rightKeys = useMemo(() => visibleSeries.filter(s => s.axis === 'right').map(s => s.key), [visibleSeries]);

  const leftDomainTicks = useMemo(() => computeIntegerDomainAndTicks(chartData, leftKeys, 5), [chartData, leftKeys]);
  const rightDomainTicks = useMemo(() => computeIntegerDomainAndTicks(chartData, rightKeys, 5), [chartData, rightKeys]);

  return (
    <>
      <ForecastTypeSelector
        options={FORECAST_SERIES}
        selectedKeys={visible}
        onToggle={handleToggle}
      />

      <div style={{ width: '100%', height: 320, minWidth: 0 }}>
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={chartData} margin={{ top: 16, right: 48, left: 0, bottom: 8 }}>
            <CartesianGrid strokeDasharray="3 3" />

            <XAxis dataKey="time" />
            {/* left axis with explicit ticks (integers) if available */}
            <YAxis
              yAxisId="left"
              domain={leftDomainTicks.domain}
              ticks={leftDomainTicks.ticks}
            />
            {/* right axis only if right keys exist */}
            {rightKeys.length > 0 && (
              <YAxis
                yAxisId="right"
                orientation="right"
                domain={rightDomainTicks.domain}
                ticks={rightDomainTicks.ticks}
                width={60}
              />
            )}

            <Tooltip content={<CustomToolTip />} wrapperStyle={{ zIndex: 9999 }} />
            <Legend />

            {FORECAST_SERIES.map(s =>
              visible.has(s.key) ? (
                <Line
                  key={s.key}
                  type="monotone"
                  dataKey={s.key}
                  name={s.label}
                  stroke={s.color}
                  dot={false}
                  yAxisId={(s.axis ?? 'left') === 'right' ? 'right' : 'left'}
                />
              ) : null
            )}
          </LineChart>
        </ResponsiveContainer>
      </div>
    </>
  );
}