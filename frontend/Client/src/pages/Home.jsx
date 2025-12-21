import { useEffect, useState } from 'react'
import { redirect } from 'react-router';
import { useNavigate } from 'react-router-dom';
import SearchBar from '../components/SearchBar';
import BuildingsLeft from '../images/homepage-buildings-left.png';
import BuildingsRight from '../images/homepage-buildings-right.png';

function Home() {
  return (
    <main className="home-page">
      <div className="main-page">
        <h1 className="title space-mono-regular">Weather Model <br />Forecast Comparison</h1>
        <div className="searchbar-container">
                <SearchBar />

        </div>
        <div className="building-container left">
          <img src={BuildingsLeft} alt="" />
        </div>
        <div className="building-container right">
          <img src={BuildingsRight} alt="" />


        </div>
      </div>

      <div className="background-texture"></div>
    </main>
  );
}



export default Home
