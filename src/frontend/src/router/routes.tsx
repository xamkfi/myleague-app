import type { ComponentType } from 'react';
import type { RouteObject } from 'react-router-dom';
import { Navigate } from 'react-router-dom';
import ProtectedRoute from '../components/ProtectedRoute/ProtectedRoute';
import RouteErrorBoundary from '../components/RouteErrorBoundary/RouteErrorBoundary';
import SuspenseWrapper from './SuspenseWrapper';
import { lazyWithRetry } from '../utils/lazyWithRetry';

// Public pages
const HomePage = lazyWithRetry(() => import('../pages/HomePage/HomePage'));
const NewsPage = lazyWithRetry(() => import('../pages/NewsPage/NewsPage'));
const RulesPage = lazyWithRetry(() => import('../pages/RulesPage/RulesPage'));
const MAHLPage = lazyWithRetry(() => import('../pages/MAHLPage/MAHLPage'));
const AgeGroupsPage = lazyWithRetry(() => import('../pages/AgeGroupsPage/AgeGroupsPage'));
const RegisterPage = lazyWithRetry(() => import('../pages/RegisterPage/RegisterPage'));
const TournamentsPage = lazyWithRetry(() => import('../pages/TournamentsPage/TournamentsPage'));
const TournamentPage = lazyWithRetry(() => import('../pages/TournamentPage/TournamentPage'));
const SportsPage = lazyWithRetry(() => import('../pages/SportsPage/SportsPage'));
const ClubPage = lazyWithRetry(() => import('../pages/ClubPage/ClubPage'));
const PlayerPage = lazyWithRetry(() => import('../pages/PlayerPage/PlayerPage'));
const SingleNewsPage = lazyWithRetry(() =>
  import('../pages/SingleNewsPage/SingleNewsPage').then((m) => ({ default: m.default as ComponentType<unknown> }))
);
const PersonUserPage = lazyWithRetry(() => import('../pages/PersonUserPage/PersonUserPage'));
const FloorballTeamPage = lazyWithRetry(() => import('../pages/FloorballTeamPage/FloorballTeamPage'));
const FloorballTeamPlayerUserPage = lazyWithRetry(() => import('../pages/FloorballTeamPlayerUserPage/FloorballTeamPlayerUserPage'));
const MatchPage = lazyWithRetry(() => import('../pages/MatchPage/MatchPage'));
const LeaguePage = lazyWithRetry(() => import('../pages/LeaguePage/LeaguePage'));
const FloorballPage = lazyWithRetry(() => import('../pages/FloorballPage/FloorballPage'));
const ClubsPage = lazyWithRetry(() => import('../pages/ClubsPage/ClubsPage'));
const EventCalendarPage = lazyWithRetry(() => import('../pages/EventCalendarPage/EventCalendarPage'));

// Admin pages
const LoginPage = lazyWithRetry(() => import('../pages/AdminPage/LoginPage/LoginPage'));
const VerifyEmailPage = lazyWithRetry(() => import('../pages/AdminPage/VerifyEmailPage/VerifyEmailPage'));
const AdminPage = lazyWithRetry(() => import('../pages/AdminPage/AdminPage'));
const UsersPage = lazyWithRetry(() => import('../pages/AdminPage/UsersPage/UsersPage'));
const PersonsPage = lazyWithRetry(() => import('../pages/AdminPage/PersonsPage/PersonsPage'));
const PersonForm = lazyWithRetry(() =>
  import('../pages/AdminPage/PersonsPage/components/PersonForm/PersonForm').then((m) => ({ default: m.default as ComponentType<unknown> }))
);
const NewsCreateEditPage = lazyWithRetry(() => import('../pages/AdminPage/NewsPage/NewsCreateEditPage'));
const NewsManagementPage = lazyWithRetry(() => import('../pages/AdminPage/NewsPage/NewsManagementPage'));
const DivisionsPage = lazyWithRetry(() => import('../pages/AdminPage/DivisionsPage/DivisionsPage'));
const DivisionFormPage = lazyWithRetry(() => import('../pages/AdminPage/DivisionsPage/DivisionFormPage'));
const ClubsManagementPage = lazyWithRetry(() => import('../pages/AdminPage/ClubPage/ClubsManagementPage'));
const CreateClubPage = lazyWithRetry(() => import('../pages/AdminPage/ClubPage/CreateClubPage'));
const EditClubPage = lazyWithRetry(() => import('../pages/AdminPage/ClubPage/EditClubPage'));
const ClubDetailsPage = lazyWithRetry(() => import('../pages/AdminPage/ClubPage/ClubDetailsPage'));

// Floorball management pages
const FloorballManagementPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballManagementPage'));
const FloorballTeamsPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/FloorballTeamsPage'));
const CreateTeamPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/CreateTeamPage'));
const EditTeamPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/EditTeamPage'));
const EditRosterPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/EditRosterPage'));
const AddPlayerToRosterPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/AddPlayerToRosterPage'));
const FloorballPlayersPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballPlayersPage/FloorballPlayersPage'));
const CreatePlayerPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballPlayersPage/CreatePlayerPage/CreatePlayerPage'));
const CreatePersonPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballPlayersPage/CreatePersonPage/CreatePersonPage'));
const FloorballRefereesPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballRefereesPage/FloorballRefereesPage'));
const CreateRefereePage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballRefereesPage/CreateRefereePage/CreateRefereePage'));
const EditRefereePage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballRefereesPage/EditRefereePage/EditRefereePage'));
const FloorballSeasonsPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballSeasonsPage/FloorballSeasonsPage'));
const CreateSeasonPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballSeasonsPage/CreateSeasonPage/CreateSeasonPage'));
const EditSeasonPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballSeasonsPage/EditSeasonPage/EditSeasonPage'));
const FloorballTournamentsPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTournamentsPage/FloorballTournamentsPage'));
const CreateTournamentPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTournamentsPage/CreateTournamentPage/CreateTournamentPage'));
const EditTournamentPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTournamentsPage/EditTournamentPage/EditTournamentPage'));
const MatchManagementPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/MatchManagementPage/MatchManagementPage'));
const CreateMatchPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/CreateMatchPage/CreateMatchPage'));
const EditMatchPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/EditMatchPage/EditMatchPage'));
const ManageMatchPage = lazyWithRetry(() => import('../pages/AdminPage/FloorballManagementPage/ManageMatchPage/ManageMatchPage'));

export const routes: RouteObject[] = [
  {
    errorElement: <RouteErrorBoundary />,
    children: [
  {
    path: '/',
    element: <SuspenseWrapper><HomePage /></SuspenseWrapper>
  },
  // Admin login (public)
  {
    path: '/admin/login',
    element: <SuspenseWrapper><LoginPage /></SuspenseWrapper>
  },
  // Admin email verification (public – linked from invitation email)
  {
    path: '/admin/verify-email',
    element: <SuspenseWrapper><VerifyEmailPage /></SuspenseWrapper>
  },
  // Protected admin routes
  {
    path: '/admin/clubs',
    children: [
      {
        index: true,
        element: <ProtectedRoute><SuspenseWrapper><ClubsManagementPage /></SuspenseWrapper></ProtectedRoute>
      },
      {
        path: 'create',
        element: <ProtectedRoute><SuspenseWrapper><CreateClubPage /></SuspenseWrapper></ProtectedRoute>
      },
      {
        path: ':id',
        element: <ProtectedRoute><SuspenseWrapper><ClubDetailsPage /></SuspenseWrapper></ProtectedRoute>
      },
      {
        path: ':id/edit',
        element: <ProtectedRoute><SuspenseWrapper><EditClubPage /></SuspenseWrapper></ProtectedRoute>
      }
    ]
  },
  {
    path: '/admin/users',
    children: [
      {
        index: true,
        element: <ProtectedRoute><SuspenseWrapper><UsersPage /></SuspenseWrapper></ProtectedRoute>
      }
    ]
  },
  {
    path: '/uutiset',
    element: <SuspenseWrapper><NewsPage /></SuspenseWrapper>
  },
  {
    path: '/tapahtumakalenteri',
    element: <SuspenseWrapper><EventCalendarPage /></SuspenseWrapper>
  },
  {
    path: '/uutiset/:id',
    element: <SuspenseWrapper><SingleNewsPage /></SuspenseWrapper>
  },
  {
    path: '/saannot',
    element: <SuspenseWrapper><RulesPage /></SuspenseWrapper>
  },
  {
    path: '/mahl',
    element: <SuspenseWrapper><MAHLPage /></SuspenseWrapper>
  },
  {
    path: '/ikaryhmat',
    element: <SuspenseWrapper><AgeGroupsPage /></SuspenseWrapper>
  },
  {
    path: '/ilmoittaudu',
    element: <SuspenseWrapper><RegisterPage /></SuspenseWrapper>
  },
  {
    path: '/turnaukset',
    element: <SuspenseWrapper><TournamentsPage /></SuspenseWrapper>
  },
  {
    path: '/tournaments',
    element: <SuspenseWrapper><TournamentsPage /></SuspenseWrapper>
  },
  {
    path: '/tournaments/:id',
    element: <SuspenseWrapper><TournamentPage /></SuspenseWrapper>
  },
  {
    path: '/lajit',
    element: <SuspenseWrapper><SportsPage /></SuspenseWrapper>
  },
  {
    path: '/sports',
    element: <SuspenseWrapper><SportsPage /></SuspenseWrapper>
  },
  {
    path: '/sports/floorball',
    element: <SuspenseWrapper><FloorballPage /></SuspenseWrapper>
  },
  {
    path: '/clubs',
    element: <SuspenseWrapper><ClubsPage /></SuspenseWrapper>
  },
  {
    path: '/club/:slug',
    element: <SuspenseWrapper><ClubPage /></SuspenseWrapper>
  },
  {
    path: '/team/:slug',
    element: <SuspenseWrapper><FloorballTeamPage /></SuspenseWrapper>
  },
  {
    path: '/pelaaja/:id',
    element: <SuspenseWrapper><PlayerPage /></SuspenseWrapper>
  },
  {
    path: '/admin',
    element: <ProtectedRoute><SuspenseWrapper><AdminPage /></SuspenseWrapper></ProtectedRoute>
  },
  {
    path: '/admin/persons',
    children: [
      {
        index: true,
        element: <ProtectedRoute><SuspenseWrapper><PersonsPage /></SuspenseWrapper></ProtectedRoute>
      },
      {
        path: 'new',
        element: <ProtectedRoute><SuspenseWrapper><PersonForm /></SuspenseWrapper></ProtectedRoute>
      },
      {
        path: ':id/edit',
        element: <ProtectedRoute><SuspenseWrapper><PersonForm /></SuspenseWrapper></ProtectedRoute>
      }
    ]
  },
  {
    path: '/admin/divisions',
    children: [
      {
        index: true,
        element: <ProtectedRoute><SuspenseWrapper><DivisionsPage /></SuspenseWrapper></ProtectedRoute>,
      },
      {
        path: 'create',
        element: <ProtectedRoute><SuspenseWrapper><DivisionFormPage /></SuspenseWrapper></ProtectedRoute>,
      },
      {
        path: ':divisionId/edit',
        element: <ProtectedRoute><SuspenseWrapper><DivisionFormPage /></SuspenseWrapper></ProtectedRoute>,
      },
    ],
  },
  {
    path: '/admin/floorball',
    children: [
      {
        index: true,
        element: <ProtectedRoute><SuspenseWrapper><FloorballManagementPage /></SuspenseWrapper></ProtectedRoute>
      },
      {
        path: 'teams',
        children: [
          {
            index: true,
            element: <ProtectedRoute><SuspenseWrapper><FloorballTeamsPage /></SuspenseWrapper></ProtectedRoute>
          },
          {
            path: 'new',
            element: <ProtectedRoute><SuspenseWrapper><CreateTeamPage /></SuspenseWrapper></ProtectedRoute>
          },
          {
            path: ':id/edit',
            element: <ProtectedRoute><SuspenseWrapper><EditTeamPage /></SuspenseWrapper></ProtectedRoute>
          },
          {
            path: ':id/roster',
            element: <ProtectedRoute><SuspenseWrapper><EditRosterPage /></SuspenseWrapper></ProtectedRoute>
          },
          {
            path: ':id/roster/add',
            element: <ProtectedRoute><SuspenseWrapper><AddPlayerToRosterPage /></SuspenseWrapper></ProtectedRoute>
          }
        ]
      },
      {
        path: 'players',
        children: [
          {
            index: true,
            element: <ProtectedRoute><SuspenseWrapper><FloorballPlayersPage /></SuspenseWrapper></ProtectedRoute>
          },
          {
            path: 'create',
            element: <ProtectedRoute><SuspenseWrapper><CreatePlayerPage /></SuspenseWrapper></ProtectedRoute>
          },
          {
            path: 'create-person',
            element: <ProtectedRoute><SuspenseWrapper><CreatePersonPage /></SuspenseWrapper></ProtectedRoute>
          }
        ]
      },
      {
        path: 'referees',
        children: [
          {
            index: true,
            element: <ProtectedRoute><SuspenseWrapper><FloorballRefereesPage /></SuspenseWrapper></ProtectedRoute>
          },
          {
            path: 'create',
            element: <ProtectedRoute><SuspenseWrapper><CreateRefereePage /></SuspenseWrapper></ProtectedRoute>
          },
          {
            path: ':refereeId/edit',
            element: <ProtectedRoute><SuspenseWrapper><EditRefereePage /></SuspenseWrapper></ProtectedRoute>
          }
        ]
      },
      {
        path: 'seasons',
        children: [
          {
            index: true,
            element: <ProtectedRoute><SuspenseWrapper><FloorballSeasonsPage /></SuspenseWrapper></ProtectedRoute>
          },
          {
            path: 'create',
            element: <ProtectedRoute><SuspenseWrapper><CreateSeasonPage /></SuspenseWrapper></ProtectedRoute>
          },
          {
            path: ':competitionId/edit',
            element: <ProtectedRoute><SuspenseWrapper><EditSeasonPage /></SuspenseWrapper></ProtectedRoute>
          }
        ]
      },
      {
        path: 'tournaments',
        children: [
          {
            index: true,
            element: <ProtectedRoute><SuspenseWrapper><FloorballTournamentsPage /></SuspenseWrapper></ProtectedRoute>
          },
          {
            path: 'create',
            element: <ProtectedRoute><SuspenseWrapper><CreateTournamentPage /></SuspenseWrapper></ProtectedRoute>
          },
          {
            path: ':competitionId/edit',
            element: <ProtectedRoute><SuspenseWrapper><EditTournamentPage /></SuspenseWrapper></ProtectedRoute>
          }
        ]
      },
      {
        path: 'matches',
        children: [
          { index: true, element: <ProtectedRoute><SuspenseWrapper><MatchManagementPage /></SuspenseWrapper></ProtectedRoute> },
          { path: 'create', element: <ProtectedRoute><SuspenseWrapper><CreateMatchPage /></SuspenseWrapper></ProtectedRoute> },
          { path: ':matchId/edit', element: <ProtectedRoute><SuspenseWrapper><EditMatchPage /></SuspenseWrapper></ProtectedRoute> },
          { path: 'manage/:matchId', element: <ProtectedRoute><SuspenseWrapper><ManageMatchPage /></SuspenseWrapper></ProtectedRoute> },
          { path: 'completed', element: <Navigate to="/admin/floorball/matches?tab=completed" replace /> },
          { path: 'scheduled', element: <Navigate to="/admin/floorball/matches?tab=scheduled" replace /> },
          { path: 'in-progress', element: <Navigate to="/admin/floorball/matches?tab=ongoing" replace /> },
          { path: 'cancelled', element: <Navigate to="/admin/floorball/matches?tab=cancelled" replace /> },
        ]
      }
    ]
  },
  {
    path: '/person/:id',
    element: <SuspenseWrapper><PersonUserPage /></SuspenseWrapper>
  },
  {
    path: '/floorballplayer/:id',
    element: <SuspenseWrapper><FloorballTeamPlayerUserPage /></SuspenseWrapper>
  },
  {
    path: '/admin/news',
    children: [
      {
        index: true,
        element: <ProtectedRoute><SuspenseWrapper><NewsManagementPage /></SuspenseWrapper></ProtectedRoute>
      },
      {
        path: 'create',
        element: <ProtectedRoute><SuspenseWrapper><NewsCreateEditPage /></SuspenseWrapper></ProtectedRoute>
      },
      {
        path: 'edit/:id',
        element: <ProtectedRoute><SuspenseWrapper><NewsCreateEditPage /></SuspenseWrapper></ProtectedRoute>
      }
    ]
  },
  {
    path: '/match/:id',
    element: <SuspenseWrapper><MatchPage /></SuspenseWrapper>
  },
  {
    path: '/league/:id',
    element: <SuspenseWrapper><LeaguePage /></SuspenseWrapper>
  }
    ]
  }
];
