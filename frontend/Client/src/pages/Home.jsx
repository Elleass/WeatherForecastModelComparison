import { useEffect, useState } from 'react'
import { redirect } from 'react-router';
import { useNavigate } from 'react-router-dom';
import SearchBar from '../components/SearchBar';
import DarkModeButton from '../components/DarkModeButton';
import BuildingsLeft from '../images/homepage-buildings-left.png';
import BuildingsRight from '../images/homepage-buildings-right.png';
import { ThemeProvider } from '../ThemeContext';

function Home() {
  return (
    <main className="home-page">
      <div className="main-page">
                <DarkModeButton/>

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
      <div className="background-texture-color"></div>
    </main>
  );
}



export default Home
