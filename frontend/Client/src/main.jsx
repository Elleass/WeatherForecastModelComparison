import React, { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import './index.css';
import App from './App.jsx';
import Home from './pages/Home.jsx';
import Location from './pages/Location.jsx';
import LocationLayout from './pages/LocationLayout.jsx';

const container = document.getElementById('root');


createRoot(container).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />

        <Route element={<LocationLayout />}>
          <Route path="location" element={<Location />} />
          <Route path="location/:city" element={<Location />} />
        </Route>
      </Routes>
    </BrowserRouter>
  </StrictMode>
);