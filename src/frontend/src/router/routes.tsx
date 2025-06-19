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
import TeamPage from '../pages/TeamPage/TeamPage';
import PersonUserPage from '../pages/PersonUserPage/PersonUserPage';
import SingleNewsPage from '../pages/SingleNewsPage/SingleNewsPage';
import NewsCreateEditPage from '../pages/AdminPage/NewsPage/NewsCreateEditPage';
import FloorballManagementPage from '../pages/AdminPage/FloorballManagementPage/FloorballManagementPage';
import FloorballTeamsPage from '../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/FloorballTeamsPage';
import FloorballPlayersPage from '../pages/AdminPage/FloorballManagementPage/FloorballPlayersPage/FloorballPlayersPage';
import FloorballSeasonsPage from '../pages/AdminPage/FloorballManagementPage/FloorballSeasonsPage/FloorballSeasonsPage';
import NewsManagementPage from '../pages/AdminPage/NewsPage/NewsManagementPage';

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
    path: '/uutiset/:id',
    element: <SingleNewsPage />
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
    path: '/team',
    element: <TeamPage/>
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
  },
  {
    path: '/admin/floorball',
    children: [
      {
        index: true,
        element: <FloorballManagementPage/>
      },
      {
        path: 'teams',
        element: <FloorballTeamsPage/>
      },
      {
        path: 'players',
        element: <FloorballPlayersPage />
      },
      {
        path: 'seasons',
        element: <FloorballSeasonsPage />
      }
    ]
  },
  {
    path: '/person/:id',
    element: <PersonUserPage />
  },
  {
    path: '/admin/news',
    element: <NewsCreatePage/>
  },
    children: [
      {
        index: true,
        element: <NewsManagementPage />
      },
      {
        path: 'create',
        element: <NewsCreateEditPage />
      },
      {
        path: 'edit/:id',
        element: <NewsCreateEditPage />
      }
    ]
  }
  

]; 