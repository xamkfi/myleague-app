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

export const routes: RouteObject[] = [
  {
    path: '/',
    element: <HomePage />
  },
  {
    path: '/admin/clubs',
    children: [
      {
        index: true,
        element: <ClubsManagementPage />
      },
      {
        path: 'create',
        element: <CreateClubPage />
      },
      {
        path: ':id',
        element: <ClubDetailsPage />
      },
      {
        path: ':id/edit',
        element: <EditClubPage />
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
    path: '/admin/divisions',
    children: [
      {
        index: true,
        element: <DivisionsPage />,
      },
      {
        path: 'create',
        element: <DivisionFormPage />,
      },
      {
        path: ':divisionId/edit',
        element: <DivisionFormPage />,
      },
    ],
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
        children: [
          {
            index: true,
            element: <FloorballTeamsPage/>
          },
          {
            path: 'new',
            element: <CreateTeamPage/>
          },
          {
            path: ':id/edit',
            element: <EditTeamPage/>
          },
          {
            path: ':id/roster',
            element: <EditRosterPage/>
          }
        ]
      },
      {
        path: 'players',
        children: [
          {
            index: true,
            element: <FloorballPlayersPage />
          },
          {
            path: 'create',
            element: <CreatePlayerPage />
          },
          {
            path: 'create-person',
            element: <CreatePersonPage />
          }
        ]
      },
      {
        path: 'referees',
        children: [
          {
            index: true,
            element: <FloorballRefereesPage />
          },
          {
            path: 'create',
            element: <CreateRefereePage />
          }
        ]
      },
      {
        path: 'seasons',
        children: [
          {
            index: true,
            element: <FloorballSeasonsPage />
          },
          {
            path: 'create',
            element: <CreateSeasonPage />
          },
          {
            path: ':seasonId/edit',
            element: <EditSeasonPage />
          }
        ]
      },
      {
        path: 'matches',
        children: [
          { index: true, element: <MatchOverviewPage /> },
          { path: 'create', element: <CreateMatchPage /> },
          { path: ':matchId/edit', element: <EditMatchPage /> },
          { path: 'completed', element: <CompletedMatchesPage /> },
          { path: 'scheduled', element: <ScheduledMatchesPage /> },
          { path: 'in-progress', element: <InProgressMatchesPage /> },
          { path: 'cancelled', element: <CancelledMatchesPage /> },
          { path: 'manage/:matchId', element: <ManageMatchPage /> }
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