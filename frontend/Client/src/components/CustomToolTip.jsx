import React from 'react';
import dayjs from 'dayjs';
import { FORECAST_SERIES } from '../config/chartConfig';

// Helper map for quick lookup
const SERIES_MAP = Object.fromEntries(FORECAST_SERIES.map(s => [s.key, s]));

export default function CustomToolTip({ active, payload, label }) {
  if (!active || !payload || !payload.length) return null;

  // fallback to payload-based x value if label missing
  const raw = payload[0]?.payload || {};
  const fallback = raw.time ?? raw.validDate ?? raw.date ?? raw.x;
  let labelToShow = label ?? fallback ?? '';

  const maybeDate = dayjs(labelToShow);
  if (maybeDate.isValid()) {
    labelToShow = maybeDate.format('YYYY-MM-DD HH:mm');
  }

  return (
    <div style={{
      background: '#fff',
      color: '#111',
      border: '1px solid rgba(0,0,0,0.12)',
      padding: 8,
      borderRadius: 6,
      boxShadow: '0 2px 8px rgba(0,0,0,0.12)',
      fontSize: 12,
      pointerEvents: 'none',
      minWidth: 140,
    }}>
      <div style={{ fontWeight: 700, marginBottom: 6 }}>{String(labelToShow)}</div>

      {payload.map(item => {
        const color = item.color || item.stroke || '#000';
        // lookup series config for unit and friendly name
        const series = SERIES_MAP[item.dataKey];
        const name = item.name ?? series?.label ?? item.dataKey;
        const unit = series?.unit ? ` ${series.unit}` : '';
        return (
          <div key={item.dataKey} style={{ display: 'flex', gap: 8, alignItems: 'center', marginTop: 4 }}>
            <div style={{ width: 10, height: 10, borderRadius: 6, background: color }} />
            <div style={{ flex: 1 }}>
              <div style={{ fontSize: 13 }}>
                {name}: <strong>{String(item.value)}{unit}</strong>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}