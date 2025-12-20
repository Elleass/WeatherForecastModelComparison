export function ForecastTypeSelector({ options, selectedKeys, onToggle }) {
  return (
    <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12 }}>
      {options.map(opt => (
        <label key={opt.key}>
          <input
            type="checkbox"
            checked={selectedKeys.has(opt.key)}
            onChange={e => onToggle(opt.key, e.target.checked)}
          />{' '}
          {opt.label}
        </label>
      ))}
    </div>
  );
}