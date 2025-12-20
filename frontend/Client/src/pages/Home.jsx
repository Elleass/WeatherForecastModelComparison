import { useEffect, useState} from 'react'
import { redirect } from 'react-router';
import { useNavigate } from 'react-router-dom';
import SearchBar from '../components/SearchBar';

function Home() {
  return (
    <main>
      <SearchBar />
      <h1>Home</h1>
    </main>
  );
}



export default Home
