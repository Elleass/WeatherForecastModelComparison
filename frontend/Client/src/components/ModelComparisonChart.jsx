import { useMemo } from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  Legend,
  ResponsiveContainer,
  CartesianGrid,
  Label,
} from 'recharts';
import { FORECAST_SERIES } from '../config/chartConfig';
import { toChartData } from '../config/toChartData';
import CustomToolTip from './CustomToolTip';

export default function ModelComparisonChart({
  data = [],
  visibleKeys = new Set(),
  title = '',
  leftDomainTicks = { domain: ['auto', 'auto'], ticks: null },
  rightDomainTicks = { domain: ['auto', 'auto'], ticks: null },
  leftUnit = null,
  rightUnit = null,
  syncId = null,
}) {
  const chartData = useMemo(() => toChartData(data), [data]);

  const visibleSeries = useMemo(
    () => FORECAST_SERIES.filter(s => visibleKeys.has(s.key)),
    [visibleKeys]
  );

  const leftDomain = leftDomainTicks?.domain ?? ['auto', 'auto'];
  const leftTicks = leftDomainTicks?.ticks ?? null;
  const rightDomain = rightDomainTicks?.domain ?? ['auto', 'auto'];
  const rightTicks = rightDomainTicks?.ticks ?? null;

  return (
    <div style={{ marginTop: 16 }}>
      {title && <h4 style={{ margin: '8px 0' }}>{title}</h4>}
      <ResponsiveContainer width="100%" height={300}>
        <LineChart syncId={syncId ?? undefined} data={chartData} margin={{ top: 8, right: 16, left: 0, bottom: 8 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="time">
            <Label value="Time" offset={-20} position="bottom" />
          </XAxis>

          <YAxis
            yAxisId="left"
            domain={leftDomain}
            ticks={leftTicks}
          >
            {leftUnit ? <Label value={leftUnit} angle={-90} position="insideLeft" offset={-6} /> : null}
          </YAxis>

          {visibleSeries.some(s => s.axis === 'right') && (
            <YAxis
              yAxisId="right"
              orientation="right"
              domain={rightDomain}
              ticks={rightTicks}
              width={60}
            >
              {rightUnit ? <Label value={rightUnit} angle={90} position="insideRight" offset={-6} /> : null}
            </YAxis>
          )}

          <Tooltip content={<CustomToolTip />} wrapperStyle={{ zIndex: 9999, pointerEvents: 'none' }} />
          <Legend />

          {FORECAST_SERIES.map(s =>
            visibleKeys.has(s.key) ? (
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
  );
}