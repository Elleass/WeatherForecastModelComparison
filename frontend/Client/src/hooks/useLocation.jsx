// hooks/useLocation.js
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';

export function useLocation() {
  const { city } = useParams();
  const [data, setData] = useState(null);
  const [err, setErr] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!city) return;
    const apiUrl = `/api/location/${encodeURIComponent(city)}`;
    let mounted = true;
    const controller = new AbortController();

    (async () => {
      setLoading(true);
      setErr(null);
      try {
        const res = await fetch(apiUrl, {
          headers: { Accept: 'application/json' },
          signal: controller.signal,
        });
        if (!res.ok) {
          const text = await res.text().catch(() => res.statusText);
          throw new Error(`HTTP ${res.status}: ${text}`);
        }
        const json = await res.json();
        if (mounted) setData(json);
      } catch (e) {
        if (mounted && e.name !== 'AbortError') setErr(e.message);
      } finally {
        if (mounted) setLoading(false);
      }
    })();

    return () => { mounted = false; controller.abort(); };
  }, [city]);

  return { city, data, err, loading };
}