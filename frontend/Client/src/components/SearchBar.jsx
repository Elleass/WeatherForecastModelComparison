import { useEffect, useState} from 'react'
import { useNavigate } from 'react-router-dom';
import './SearchBar.css';
import RightArrow from '../images/right-arrow.svg'

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
    <form className="searchbar-form" onSubmit={handleSubmit} role="search">
      <input className="searchbar space-mono-bold" name="input" type="search" placeholder="Search..." />
      <button className="submit-button"type="submit"><img src={RightArrow}/></button>
    </form>
  );
}

export default SearchBar;