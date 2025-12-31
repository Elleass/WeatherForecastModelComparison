import React from 'react';

/**
 * Dumb selector UI:
 * - models: [{ id, name }]
 * - selectedModels: Set of ids
 * - onToggleModel: (id, checked) => void
 */
export default function MultiModelSelector({ models = [], selectedModels = new Set(), onToggleModel }) {
  return (
    <div className="type-selector-container" style={{ display: 'flex', gap: 12, flexWrap: 'wrap', marginBottom: 12 }}>
      {models.map(m => (
        <label className="checkbox-container"key={m.id} style={{ userSelect: 'none' }}>
          <input
            type="checkbox"
            checked={selectedModels.has(m.id)}
            onChange={e => onToggleModel(m.id, e.target.checked)}
          />{' '}
          <span className="checkmark"/>

          {m.name ?? m.id}

        </label>
      ))}
    </div>
  );
}