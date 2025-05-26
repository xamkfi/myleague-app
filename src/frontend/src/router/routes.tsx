import React from 'react';
import type { RouteObject } from 'react-router-dom';
import HomePage from '../pages/HomePage';
import NewsPage from '../pages/NewsPage';
import RulesPage from '../pages/RulesPage';
import MAHLPage from '../pages/MAHLPage';
import AgeGroupsPage from '../pages/AgeGroupsPage';
import RegisterPage from '../pages/RegisterPage';
import TournamentsPage from '../pages/TournamentsPage';
import SportsPage from '../pages/SportsPage';
import ClubPage from '../pages/ClubPage';

export const routes: RouteObject[] = [
  {
    path: '/',
    element: <HomePage />
  },
  {
    path: '/uutiset',
    element: <NewsPage />
  },
  {
    path: '/saannot',
    element: <RulesPage />
  },
  {
    path: '/mahl',
    element: <MAHLPage />
  },
  {
    path: '/ikaryhmat',
    element: <AgeGroupsPage />
  },
  {
    path: '/ilmoittaudu',
    element: <RegisterPage />
  },
  {
    path: '/turnaukset',
    element: <TournamentsPage />
  },
  {
    path: '/lajit',
    element: <SportsPage />
  },
  {
    path: '/club/:slug',
    element: <ClubPage />
  }
]; 