import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';

function useCityFromPath() {
  const { city } = useParams(); // requires your route to define :city
  return city ?? null;
}

function Search({ city }) {
  const [data, setData] = useState(null);
  const [err, setErr] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!city) return;
    const apiUrl = `/api/location/${encodeURIComponent(city)}`; // note leading /
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

    return () => {
      mounted = false;
      controller.abort();
    };
  }, [city]);

  return (
    <section>
      <h2>Location lookup</h2>
      {loading && <div>Loading…</div>}
      {err && <div style={{ color: 'crimson' }}>Error: {err}</div>}
      {data && <pre>{JSON.stringify(data, null, 2)}</pre>}
    </section>
  );
}

function FetchForecast({ city }) {
  const [data, setData] = useState(null);
  const [err, setErr] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!city) return;
    const apiUrl = `/api/forecast/${encodeURIComponent(city)}`; // note leading /
    console.log(apiUrl);
    // const apiUrl = `/api/forecast/Kraków`;
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

    return () => {
      mounted = false;
      controller.abort();
    };
  }, [city]);

  return data;
}

export default function LocationLayout() {
  const city = useCityFromPath();
  return (
    <main>
      <Search city={city} />
      <FetchForecast city={city} />
    </main>
  );
}