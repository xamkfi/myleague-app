import React from 'react';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { routes } from './router/routes';
import './App.scss';
import PlayerPage from './pages/PlayerPage/PlayerPage'

// Initialize i18n
import './i18n/i18n';

const router = createBrowserRouter(routes);

function App() {
  return (
    <RouterProvider router={router} />
    // <PlayerPage playerId={1}/>
  );
}

export default App;
