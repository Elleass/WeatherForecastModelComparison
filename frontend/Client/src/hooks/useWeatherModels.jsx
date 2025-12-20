import { useEffect, useState } from 'react';

export function useWeatherModels() {
  const [models, setModels] = useState([]); // [{ id, name }]
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState(null);

  useEffect(() => {
    let active = true;
    setLoading(true);
    fetch('/api/weathermodels')
      .then(r => {
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
        return r.json();
      })
      .then(json => {
        if (!active) return;
        setModels(json || []);
        setErr(null);
      })
      .catch(e => {
        if (!active) return;
        setErr(e.message || 'Failed to load models');
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => { active = false; };
  }, []);

  return { models, loading, err };
}