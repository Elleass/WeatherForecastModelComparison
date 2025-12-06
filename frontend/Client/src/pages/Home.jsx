import { useEffect, useState} from 'react'
import { redirect } from 'react-router';
import { useNavigate } from 'react-router-dom';
function SearchBar() {
  const navigate = useNavigate(); //define hooks on the top;

  function handleSubmit(e) {
    e.preventDefault();
    const raw = e.target.elements.input?.value ?? '';
    const q = raw.trim();
    if (!q) return;
    navigate(`/location/${encodeURIComponent(q)}`);
  }

  return (
    <form onSubmit={handleSubmit} role="search">
      <input name="input" type="search" placeholder="Search..." />
      <button type="submit">Submit</button>
    </form>
  );
}

function Home() {
  return (
    <main>
      <SearchBar />
      <h1>Home</h1>
    </main>
  );
}



export default Home
