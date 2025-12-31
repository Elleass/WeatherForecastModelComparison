export function ForecastTypeSelector({ options, selectedKeys, onToggle }) {
  return (
    <div className="type-selector-container" style={{ display: 'flex', flexWrap: 'wrap', gap: 12 }}>
      {options.map(opt => (
        <label className="checkbox-container" key={opt.key}>
          <input
            type="checkbox"
            checked={selectedKeys.has(opt.key)}
            onChange={e => onToggle(opt.key, e.target.checked)}
          />{' '}
          {opt.label}
          <span className="checkmark"/>

        </label>
      ))}
    </div>
  );
}