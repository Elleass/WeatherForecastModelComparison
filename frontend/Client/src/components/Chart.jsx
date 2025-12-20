import { useMemo, useState } from 'react';
import { LineChart, Line, XAxis, YAxis, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { FORECAST_SERIES } from '../config/chartConfig';
import { toChartData } from '../config/toChartData';
import { ForecastTypeSelector } from './ForecastTypeSelector';

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

  return (
    <>
      <ForecastTypeSelector
        options={FORECAST_SERIES}
        selectedKeys={visible}
        onToggle={handleToggle}
      />

      <ResponsiveContainer width="100%" height={320}>
        <LineChart data={chartData} margin={{ top: 16, right: 16, left: 0, bottom: 8 }}>
          <XAxis dataKey="time" />
          <YAxis />
          <Tooltip />
          <Legend />
          {FORECAST_SERIES.map(s =>
            visible.has(s.key) ? (
              <Line
                key={s.key}
                type="monotone"
                dataKey={s.key}
                stroke={s.color}
                dot={false}
              />
            ) : null
          )}
        </LineChart>
      </ResponsiveContainer>
    </>
  );
}