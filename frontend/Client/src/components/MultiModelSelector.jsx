import { useMemo, useState } from 'react';
import Chart from './Chart'; // expects Chart({ data, title })

export default function MultiModelSelector({ forecastData = [], models = [] }) {
  // models: [{ id, name }]
  const [selectedModels, setSelectedModels] = useState(new Set());

  const modelsById = useMemo(
    () => Object.fromEntries(models.map(m => [m.id, m])),
    [models]
  );

  const toggleModel = (id, checked) => {
    setSelectedModels(prev => {
      const next = new Set(prev);
      if (checked) next.add(id);
      else next.delete(id);
      return next;
    });
  };

  return (
    <section>
      <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
        {models.map(m => (
          <label key={m.id}>
            <input
              type="checkbox"
              checked={selectedModels.has(m.id)}
              onChange={e => toggleModel(m.id, e.target.checked)}
            />{' '}
            {m.name ?? m.id}
          </label>
        ))}
      </div>

      {/* One chart per selected model */}
      {Array.from(selectedModels).map(modelId => {
        const series = forecastData.filter(f => f.weatherModelId === modelId);
        const title = modelsById[modelId]?.name ?? modelId;
        return (
          <div key={modelId} style={{ marginTop: 16 }}>
            <h4>{title}</h4>
            <Chart data={series} title={title} />
          </div>
        );
      })}
    </section>
  );
}