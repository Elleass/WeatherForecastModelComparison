import React from 'react';

/**
 * Dumb selector UI:
 * - models: [{ id, name }]
 * - selectedModels: Set of ids
 * - onToggleModel: (id, checked) => void
 */
export default function MultiModelSelector({ models = [], selectedModels = new Set(), onToggleModel }) {
  return (
    <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap', marginBottom: 12 }}>
      {models.map(m => (
        <label key={m.id} style={{ userSelect: 'none' }}>
          <input
            type="checkbox"
            checked={selectedModels.has(m.id)}
            onChange={e => onToggleModel(m.id, e.target.checked)}
          />{' '}
          {m.name ?? m.id}
        </label>
      ))}
    </div>
  );
}