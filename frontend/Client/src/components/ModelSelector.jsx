export function ModelSelector({ models, selected, onChange, disabled }) {
  return (
    <label>
      Model:{' '}
      <select
        value={selected ?? ''}
        onChange={e => onChange(Number(e.target.value))}
        disabled={disabled || !models.length}
      >
        {models.map(m => (
          <option key={m.id} value={m.id}>
            {m.name ?? m.id}
          </option>
        ))}
      </select>
    </label>
  );
}