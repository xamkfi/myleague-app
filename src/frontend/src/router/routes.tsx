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
import PersonUserPage from '../pages/PersonUserPage/PersonUserPage';
import SingleNewsPage from '../pages/SingleNewsPage/SingleNewsPage';
import NewsCreateEditPage from '../pages/AdminPage/NewsPage/NewsCreateEditPage';
import DivisionsPage from '../pages/AdminPage/DivisionsPage/DivisionsPage';
import DivisionFormPage from '../pages/AdminPage/DivisionsPage/DivisionFormPage';
import FloorballManagementPage from '../pages/AdminPage/FloorballManagementPage/FloorballManagementPage';
import FloorballTeamsPage from '../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/FloorballTeamsPage';
import CreateTeamPage from '../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/CreateTeamPage';
import EditTeamPage from '../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/EditTeamPage';
import EditRosterPage from '../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/EditRosterPage';
import AddPlayerToRosterPage from '../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/AddPlayerToRosterPage';
import FloorballPlayersPage from '../pages/AdminPage/FloorballManagementPage/FloorballPlayersPage/FloorballPlayersPage';
import CreatePlayerPage from '../pages/AdminPage/FloorballManagementPage/FloorballPlayersPage/CreatePlayerPage/CreatePlayerPage';
import CreatePersonPage from '../pages/AdminPage/FloorballManagementPage/FloorballPlayersPage/CreatePersonPage/CreatePersonPage';
import FloorballRefereesPage from '../pages/AdminPage/FloorballManagementPage/FloorballRefereesPage/FloorballRefereesPage';
import CreateRefereePage from '../pages/AdminPage/FloorballManagementPage/FloorballRefereesPage/CreateRefereePage/CreateRefereePage';
import FloorballSeasonsPage from '../pages/AdminPage/FloorballManagementPage/FloorballSeasonsPage/FloorballSeasonsPage';
import CreateSeasonPage from '../pages/AdminPage/FloorballManagementPage/FloorballSeasonsPage/CreateSeasonPage/CreateSeasonPage';
import EditSeasonPage from '../pages/AdminPage/FloorballManagementPage/FloorballSeasonsPage/EditSeasonPage/EditSeasonPage';
import MatchOverviewPage from '../pages/AdminPage/FloorballManagementPage/MatchOverviewPage/MatchOverviewPage';
import CreateMatchPage from '../pages/AdminPage/FloorballManagementPage/CreateMatchPage/CreateMatchPage';
import EditMatchPage from '../pages/AdminPage/FloorballManagementPage/EditMatchPage/EditMatchPage';
import CompletedMatchesPage from '../pages/AdminPage/FloorballManagementPage/CompletedMatchesPage/CompletedMatchesPage';
import ScheduledMatchesPage from '../pages/AdminPage/FloorballManagementPage/ScheduledMatchesPage/ScheduledMatchesPage';
import InProgressMatchesPage from '../pages/AdminPage/FloorballManagementPage/InProgressMatchesPage/InProgressMatchesPage';
import CancelledMatchesPage from '../pages/AdminPage/FloorballManagementPage/CancelledMatchesPage/CancelledMatchesPage';
import ManageMatchPage from '../pages/AdminPage/FloorballManagementPage/ManageMatchPage/ManageMatchPage';
import FloorballTeamPage from '../pages/FloorballTeamPage/FloorballTeamPage';
import NewsManagementPage from '../pages/AdminPage/NewsPage/NewsManagementPage';
import FloorballTeamPlayerUserPage from '../pages/FloorballTeamPlayerUserPage/FloorballTeamPlayerUserPage';
import MatchPage from '../pages/MatchPage/MatchPage';
import LeaguePage from '../pages/LeaguePage/LeaguePage';
import ClubsManagementPage from '../pages/AdminPage/ClubPage/ClubsManagementPage';
import CreateClubPage from '../pages/AdminPage/ClubPage/CreateClubPage';
import EditClubPage from '../pages/AdminPage/ClubPage/EditClubPage';
import ClubDetailsPage from '../pages/AdminPage/ClubPage/ClubDetailsPage';
import FloorballPage from '../pages/FloorballPage/FloorballPage';
import ClubsPage from '../pages/ClubsPage/ClubsPage';
import LoginPage from '../pages/AdminPage/LoginPage/LoginPage';
import ProtectedRoute from '../components/ProtectedRoute/ProtectedRoute';

export const routes: RouteObject[] = [
  {
    path: '/',
    element: <HomePage />
  },
  // Admin login (public)
  {
    path: '/admin/login',
    element: <LoginPage />
  },
  // Protected admin routes
  {
    path: '/admin/clubs',
    children: [
      {
        index: true,
        element: <ProtectedRoute><ClubsManagementPage /></ProtectedRoute>
      },
      {
        path: 'create',
        element: <ProtectedRoute><CreateClubPage /></ProtectedRoute>
      },
      {
        path: ':id',
        element: <ProtectedRoute><ClubDetailsPage /></ProtectedRoute>
      },
      {
        path: ':id/edit',
        element: <ProtectedRoute><EditClubPage /></ProtectedRoute>
      }
    ]
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
    path: '/sports',
    element: <SportsPage />
  },
  {
    path: '/sports/floorball',
    element: <FloorballPage />
  },
  {
    path: '/clubs',
    element: <ClubsPage />
  },
  {
    path: '/club/:slug',
    element: <ClubPage />
  },
  {
    path: '/team/:slug',
    element: <FloorballTeamPage />
  },
  {
    path: '/pelaaja/:id',
    element: <PlayerPage />
  },
  {
    path: '/admin',
    element: <ProtectedRoute><AdminPage /></ProtectedRoute>
  },
  {
    path: '/admin/persons',
    children: [
      {
        index: true,
        element: <ProtectedRoute><PersonsPage /></ProtectedRoute>
      },
      {
        path: 'new',
        element: <ProtectedRoute><PersonForm /></ProtectedRoute>
      },
      {
        path: ':id/edit',
        element: <ProtectedRoute><PersonForm /></ProtectedRoute>
      }
    ]
  },
  {
    path: '/admin/divisions',
    children: [
      {
        index: true,
        element: <ProtectedRoute><DivisionsPage /></ProtectedRoute>,
      },
      {
        path: 'create',
        element: <ProtectedRoute><DivisionFormPage /></ProtectedRoute>,
      },
      {
        path: ':divisionId/edit',
        element: <ProtectedRoute><DivisionFormPage /></ProtectedRoute>,
      },
    ],
  },
  {
    path: '/admin/floorball',
    children: [
      {
        index: true,
        element: <ProtectedRoute><FloorballManagementPage/></ProtectedRoute>
      },
      {
        path: 'teams',
        children: [
          {
            index: true,
            element: <ProtectedRoute><FloorballTeamsPage/></ProtectedRoute>
          },
          {
            path: 'new',
            element: <ProtectedRoute><CreateTeamPage/></ProtectedRoute>
          },
          {
            path: ':id/edit',
            element: <ProtectedRoute><EditTeamPage/></ProtectedRoute>
          },
          {
            path: ':id/roster',
            element: <ProtectedRoute><EditRosterPage/></ProtectedRoute>
          },
          {
            path: ':id/roster/add',
            element: <ProtectedRoute><AddPlayerToRosterPage/></ProtectedRoute>
          }
        ]
      },
      {
        path: 'players',
        children: [
          {
            index: true,
            element: <ProtectedRoute><FloorballPlayersPage /></ProtectedRoute>
          },
          {
            path: 'create',
            element: <ProtectedRoute><CreatePlayerPage /></ProtectedRoute>
          },
          {
            path: 'create-person',
            element: <ProtectedRoute><CreatePersonPage /></ProtectedRoute>
          }
        ]
      },
      {
        path: 'referees',
        children: [
          {
            index: true,
            element: <ProtectedRoute><FloorballRefereesPage /></ProtectedRoute>
          },
          {
            path: 'create',
            element: <ProtectedRoute><CreateRefereePage /></ProtectedRoute>
          }
        ]
      },
      {
        path: 'seasons',
        children: [
          {
            index: true,
            element: <ProtectedRoute><FloorballSeasonsPage /></ProtectedRoute>
          },
          {
            path: 'create',
            element: <ProtectedRoute><CreateSeasonPage /></ProtectedRoute>
          },
          {
            path: ':seasonId/edit',
            element: <ProtectedRoute><EditSeasonPage /></ProtectedRoute>
          }
        ]
      },
      {
        path: 'matches',
        children: [
          { index: true, element: <ProtectedRoute><MatchOverviewPage /></ProtectedRoute> },
          { path: 'create', element: <ProtectedRoute><CreateMatchPage /></ProtectedRoute> },
          { path: ':matchId/edit', element: <ProtectedRoute><EditMatchPage /></ProtectedRoute> },
          { path: 'completed', element: <ProtectedRoute><CompletedMatchesPage /></ProtectedRoute> },
          { path: 'scheduled', element: <ProtectedRoute><ScheduledMatchesPage /></ProtectedRoute> },
          { path: 'in-progress', element: <ProtectedRoute><InProgressMatchesPage /></ProtectedRoute> },
          { path: 'cancelled', element: <ProtectedRoute><CancelledMatchesPage /></ProtectedRoute> },
          { path: 'manage/:matchId', element: <ProtectedRoute><ManageMatchPage /></ProtectedRoute> }
        ]
      }
    ]
  },
  {
    path: '/person/:id',
    element: <PersonUserPage />
  },
  {
    path: '/floorballplayer/:id',
    element: <FloorballTeamPlayerUserPage />
  },
  {
    path: '/admin/news',
    children: [
      {
        index: true,
        element: <ProtectedRoute><NewsManagementPage /></ProtectedRoute>
      },
      {
        path: 'create',
        element: <ProtectedRoute><NewsCreateEditPage /></ProtectedRoute>
      },
      {
        path: 'edit/:id',
        element: <ProtectedRoute><NewsCreateEditPage /></ProtectedRoute>
      }
    ]
  },
  {
    path: '/match/:id',
    element: <MatchPage/>
  },
  {
    path: '/league/:id',
    element: <LeaguePage/>
  }
];
