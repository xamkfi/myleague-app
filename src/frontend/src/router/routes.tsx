import { lazy, Suspense } from 'react';
import type { RouteObject } from 'react-router-dom';
import { Navigate } from 'react-router-dom';
import ProtectedRoute from '../components/ProtectedRoute/ProtectedRoute';

// Public pages
const HomePage = lazy(() => import('../pages/HomePage/HomePage'));
const NewsPage = lazy(() => import('../pages/NewsPage/NewsPage'));
const RulesPage = lazy(() => import('../pages/RulesPage/RulesPage'));
const MAHLPage = lazy(() => import('../pages/MAHLPage/MAHLPage'));
const AgeGroupsPage = lazy(() => import('../pages/AgeGroupsPage/AgeGroupsPage'));
const RegisterPage = lazy(() => import('../pages/RegisterPage/RegisterPage'));
const TournamentsPage = lazy(() => import('../pages/TournamentsPage/TournamentsPage'));
const SportsPage = lazy(() => import('../pages/SportsPage/SportsPage'));
const ClubPage = lazy(() => import('../pages/ClubPage/ClubPage'));
const PlayerPage = lazy(() => import('../pages/PlayerPage/PlayerPage'));
const SingleNewsPage = lazy(() => import('../pages/SingleNewsPage/SingleNewsPage'));
const PersonUserPage = lazy(() => import('../pages/PersonUserPage/PersonUserPage'));
const FloorballTeamPage = lazy(() => import('../pages/FloorballTeamPage/FloorballTeamPage'));
const FloorballTeamPlayerUserPage = lazy(() => import('../pages/FloorballTeamPlayerUserPage/FloorballTeamPlayerUserPage'));
const MatchPage = lazy(() => import('../pages/MatchPage/MatchPage'));
const LeaguePage = lazy(() => import('../pages/LeaguePage/LeaguePage'));
const FloorballPage = lazy(() => import('../pages/FloorballPage/FloorballPage'));
const ClubsPage = lazy(() => import('../pages/ClubsPage/ClubsPage'));

// Admin pages
const LoginPage = lazy(() => import('../pages/AdminPage/LoginPage/LoginPage'));
const AdminPage = lazy(() => import('../pages/AdminPage/AdminPage'));
const UsersPage = lazy(() => import('../pages/AdminPage/UsersPage/UsersPage'));
const PersonsPage = lazy(() => import('../pages/AdminPage/PersonsPage/PersonsPage'));
const PersonForm = lazy(() => import('../pages/AdminPage/PersonsPage/components/PersonForm/PersonForm'));
const NewsCreateEditPage = lazy(() => import('../pages/AdminPage/NewsPage/NewsCreateEditPage'));
const NewsManagementPage = lazy(() => import('../pages/AdminPage/NewsPage/NewsManagementPage'));
const DivisionsPage = lazy(() => import('../pages/AdminPage/DivisionsPage/DivisionsPage'));
const DivisionFormPage = lazy(() => import('../pages/AdminPage/DivisionsPage/DivisionFormPage'));
const ClubsManagementPage = lazy(() => import('../pages/AdminPage/ClubPage/ClubsManagementPage'));
const CreateClubPage = lazy(() => import('../pages/AdminPage/ClubPage/CreateClubPage'));
const EditClubPage = lazy(() => import('../pages/AdminPage/ClubPage/EditClubPage'));
const ClubDetailsPage = lazy(() => import('../pages/AdminPage/ClubPage/ClubDetailsPage'));

// Floorball management pages
const FloorballManagementPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballManagementPage'));
const FloorballTeamsPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/FloorballTeamsPage'));
const CreateTeamPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/CreateTeamPage'));
const EditTeamPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/EditTeamPage'));
const EditRosterPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/EditRosterPage'));
const AddPlayerToRosterPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballTeamsPage/AddPlayerToRosterPage'));
const FloorballPlayersPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballPlayersPage/FloorballPlayersPage'));
const CreatePlayerPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballPlayersPage/CreatePlayerPage/CreatePlayerPage'));
const CreatePersonPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballPlayersPage/CreatePersonPage/CreatePersonPage'));
const FloorballRefereesPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballRefereesPage/FloorballRefereesPage'));
const CreateRefereePage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballRefereesPage/CreateRefereePage/CreateRefereePage'));
const FloorballSeasonsPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballSeasonsPage/FloorballSeasonsPage'));
const CreateSeasonPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballSeasonsPage/CreateSeasonPage/CreateSeasonPage'));
const EditSeasonPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/FloorballSeasonsPage/EditSeasonPage/EditSeasonPage'));
const MatchManagementPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/MatchManagementPage/MatchManagementPage'));
const CreateMatchPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/CreateMatchPage/CreateMatchPage'));
const EditMatchPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/EditMatchPage/EditMatchPage'));
const ManageMatchPage = lazy(() => import('../pages/AdminPage/FloorballManagementPage/ManageMatchPage/ManageMatchPage'));

function SuspenseWrapper({ children }: { children: React.ReactNode }) {
  return <Suspense fallback={null}>{children}</Suspense>;
}

export const routes: RouteObject[] = [
  {
    path: '/',
    element: <SuspenseWrapper><HomePage /></SuspenseWrapper>
  },
  // Admin login (public)
  {
    path: '/admin/login',
    element: <SuspenseWrapper><LoginPage /></SuspenseWrapper>
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
            path: ':seasonId/edit',
            element: <ProtectedRoute><SuspenseWrapper><EditSeasonPage /></SuspenseWrapper></ProtectedRoute>
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
];
