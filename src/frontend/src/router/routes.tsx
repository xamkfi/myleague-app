import React from 'react';
import type { RouteObject } from 'react-router-dom';
import HomePage from '../pages/HomePage/HomePage';
import NewsPage from '../pages/NewsPage/NewsPage';
import RulesPage from '../pages/RulesPage/RulesPage';
import MAHLPage from '../pages/MAHLPage/MAHLPage';
import AgeGroupsPage from '../pages/AgeGroupsPage/AgeGroupsPage';
import RegisterPage from '../pages/RegisterPage/RegisterPage';
import TournamentsPage from '../pages/TournamentsPage/TournamentsPage';
import SportsPage from '../pages/SportsPage/SportsPage';
import ClubPage from '../pages/ClubPage/ClubPage';
import PlayerPage from '../pages/PlayerPage/PlayerPage';
import AdminPage from '../pages/AdminPage/AdminPage';
import PersonsPage from '../pages/AdminPage/PersonsPage/PersonsPage';
import PersonForm from '../pages/AdminPage/PersonsPage/components/PersonForm/PersonForm';

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
    path: '/club/:id',
    element: <ClubPage />
  },
  {
    path: '/pelaaja/:id',
    element: <PlayerPage />
  },
  {
    path: '/admin',
    element: <AdminPage />
  },
  {
    path: '/admin/persons',
    children: [
      {
        index: true,
        element: <PersonsPage />
      },
      {
        path: 'new',
        element: <PersonForm />
      },
      {
        path: ':id/edit',
        element: <PersonForm />
      }
    ]
  }
]; 