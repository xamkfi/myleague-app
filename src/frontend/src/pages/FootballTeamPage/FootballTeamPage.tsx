import { useEffect, useState } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import type { FootballMatchDto, FootballTeam } from '../../types/football/footballTypes';
import { footballTeamNameSearchService } from '../../api/football/footballTeamNameSearchService';
import { footballTeamService } from '../../api/football/footballTeamService';
import { findTeamBySlug, createClubSlug } from '../../utils/slugUtils';
import { teamMarkLabel } from '../../utils/teamMarkLabel';
import './FootballTeamPage.scss';
import { footballMatchService } from '../../api/football/footballMatchService';
import { footballStatisticsService, type FootballTeamSeasonStatisticsDto, type FootballSeasonStatisticsSummaryDto, type FootballPlayerSeasonStatisticsDto } from '../../api/football/footballStatistics';
import { footballSeasonService, type FootballSeasonDto } from '../../api/football/footballSeasonService';
import CompetitionHero from '../../components/CompetitionHero/CompetitionHero';
import ResultsSection from './components/ResultsSection';
import { useTranslation } from 'react-i18next';
import RosterSection from './components/RosterSection';
import SummarySection from './components/SummarySection';
import Statistics from './components/Statistics';
import FootballLeagueStanding from '../FootballLeaguePage/components/FootballLeagueStanding';
import TeamNavbar from '../../components/TeamNavbar/TeamNavbar';
import { isGuid } from '../../utils/sportRoutes';
import { isNotFoundError } from '../../api/utils/isNotFoundError';

function seasonIncludesTeam(season: FootballSeasonDto, teamId: string): boolean {
  return season.seasonDivisions?.some((seasonDivision) => seasonDivision.teamIds?.includes(teamId)) ?? false;
}

function pickSeasonForTeam(
  seasons: FootballSeasonDto[],
  teamId: string,
  divisionId?: string | null,
  teamCategory?: string | null,
): FootballSeasonDto | null {
  const memberOf = seasons.filter((season) => seasonIncludesTeam(season, teamId));
  const inDivision = divisionId
    ? seasons.filter((season) =>
        season.seasonDivisions?.some((seasonDivision) => seasonDivision.divisionId === divisionId))
    : [];
  const pool = memberOf.length > 0 ? memberOf : inDivision;
  const sameCategory = teamCategory
    ? pool.filter((season) => season.teamCategory === teamCategory)
    : [];
  const matching = sameCategory.length > 0 ? sameCategory : pool;
  const active = matching.find((season) => season.isActive);
  if (active) {
    return active;
  }

  return matching
    .slice()
    .sort((left, right) => new Date(right.startDate).getTime() - new Date(left.startDate).getTime())[0] ?? null;
}

async function getCurrentSeason(
  teamId: string,
  divisionId?: string | null,
  teamCategory?: string | null,
): Promise<FootballSeasonDto | null> {
  try {
    const [activeSeasonsResponse, allSeasonsResponse] = await Promise.all([
      footballSeasonService.getActive(),
      footballSeasonService.getAll(),
    ]);

    return pickSeasonForTeam(activeSeasonsResponse.data ?? [], teamId, divisionId, teamCategory)
      ?? pickSeasonForTeam(allSeasonsResponse.data ?? [], teamId, divisionId, teamCategory);
  } catch (error) {
    console.error('Error fetching current season:', error);
    return null;
  }
}

async function resolveSeason(
  teamId: string,
  divisionId: string | null | undefined,
  requestedSeasonId: string | null,
  teamCategory?: string | null,
): Promise<FootballSeasonDto | null> {
  if (isGuid(requestedSeasonId)) {
    try {
      const requested = await footballSeasonService.getById(requestedSeasonId);
      if (requested.data) {
        return requested.data;
      }
    } catch {
      // Fall back to the current division season when the query id is stale.
    }
  }

  return getCurrentSeason(teamId, divisionId, teamCategory);
}

function FootballTeamPage() {
  const { slug } = useParams<{ slug: string }>();
  const [searchParams] = useSearchParams();
  const requestedSeasonId = searchParams.get('season');
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [team, setTeam] = useState<FootballTeam | null>(null);
  const [matches, setMatches] = useState<FootballMatchDto[] | null>(null);
  const [teamStatistics, setTeamStatistics] = useState<FootballTeamSeasonStatisticsDto | null>(null);
  const [seasonSummary, setSeasonSummary] = useState<FootballSeasonStatisticsSummaryDto | null>(null);
  const [currentSeason, setCurrentSeason] = useState<FootballSeasonDto | null>(null);
  const [fetchedTabs, setFetchedTabs] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<string>('summary');
  const [matchesLoading, setMatchesLoading] = useState(false);
  const [matchesError, setMatchesError] = useState<string | null>(null);
  const [playerStatistics, setPlayerStatistics] = useState<FootballPlayerSeasonStatisticsDto[] | null>(null);
  const [statisticsLoading, setStatisticsLoading] = useState(false);
  const [statisticsError, setStatisticsError] = useState<string | null>(null);
  const [seasonSummaryLoading, setSeasonSummaryLoading] = useState(false);
  const [seasonSummaryError, setSeasonSummaryError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    const fetchTeamData = async () => {
      if (!slug) {
        setError('No team specified');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);

        // Fetch all teams to enable slug resolution
        const teamsResponse = await footballTeamNameSearchService.getTeamNames("");
        const allTeams = teamsResponse.data || [];
        
        // Find team by slug
        const foundTeam = findTeamBySlug(allTeams, slug);

        if (foundTeam) {
          const teamResponse = await footballTeamService.getById(foundTeam.id);

          const currentSeasonData = await resolveSeason(
            foundTeam.id,
            teamResponse.divisionId,
            requestedSeasonId,
            teamResponse.teamCategory,
          );
          setCurrentSeason(currentSeasonData);
          setTeamStatistics(null);
          setPlayerStatistics(null);
          setSeasonSummary(null);
          setFetchedTabs(new Set());
          setTeam(
            currentSeasonData
              ? await footballTeamService.getById(foundTeam.id, currentSeasonData.id)
              : teamResponse,
          );
        } else {
          setError('Team not found');
        }

        setLoading(false);
      } catch {
        setError('Failed to load team information. Please try again later.');
        setLoading(false);
      }
    };
    fetchTeamData();
  }, [slug, requestedSeasonId]);

  // Fetch matches with pagination when team changes or page changes
  useEffect(() => {
    const fetchMatches = async () => {
      if (!team) return;

      try {
        setMatchesLoading(true);
        setMatchesError(null);

        const response = await footballMatchService.getAll({
          teamId: team.id,
          page: currentPage,
          pageSize: 10,
          sortOrder: 'asc'
        });

        setMatches(response.data || []);
        setTotalPages(response.pagination.totalPages || 1);
      } catch (error) {
        console.error('Failed to fetch matches:', error);
        setMatchesError('Failed to load matches');
        setMatches([]);
      } finally {
        setMatchesLoading(false);
      }
    };
    fetchMatches();
  }, [team, currentPage]);

  // Function to fetch data for specific tabs
  const fetchTabData = async (tabId: string) => {
    if (!team || !currentSeason) return;

    try {
      if (tabId === 'stats' || tabId === 'roster') {
        if (tabId === 'stats') {
          setStatisticsLoading(true);
          setStatisticsError(null);
        }
        const fetchTeamStats = tabId === 'stats' && !teamStatistics;
        const fetchPlayerStats = !playerStatistics;

        if (fetchTeamStats && fetchPlayerStats) {
          const [teamStats, playerStats] = await Promise.all([
            footballStatisticsService.getTeamStatistics(currentSeason.id, team.id),
            footballStatisticsService.getTeamPlayerStatistics(currentSeason.id, team.id),
          ]);
          setTeamStatistics(teamStats);
          setPlayerStatistics(playerStats);
        } else if (fetchTeamStats) {
          const teamStats = await footballStatisticsService.getTeamStatistics(currentSeason.id, team.id);
          setTeamStatistics(teamStats);
        } else if (fetchPlayerStats) {
          const playerStats = await footballStatisticsService.getTeamPlayerStatistics(currentSeason.id, team.id);
          setPlayerStatistics(playerStats);
        }
      } else if (tabId === 'standings') {
        setSeasonSummaryLoading(true);
        setSeasonSummaryError(null);
        
        // Fetch season summary data (includes standings)
        const seasonSummaryData = await footballStatisticsService.getSeasonStatistics(currentSeason.id);
        setSeasonSummary(seasonSummaryData);
      }
      
    } catch (error) {
      console.error(`Failed to fetch ${tabId} data:`, error);
      if ((tabId === 'stats' || tabId === 'roster') && isNotFoundError(error)) {
        setTeamStatistics(null);
        setPlayerStatistics([]);
      } else if (tabId === 'stats' || tabId === 'roster') {
        setStatisticsError('Failed to load team statistics');
      } else if (tabId === 'standings') {
        setSeasonSummaryError('Failed to load season summary');
      }
    } finally {
      if (tabId === 'stats') {
        setStatisticsLoading(false);
      } else if (tabId === 'standings') {
        setSeasonSummaryLoading(false);
      }
    }
  };

  if (loading) {
    return (
      <PageTemplate title={t('common.loading')}>
        <div className="football-team-page">
          <div className="loading-state">
            <h2>{t('teamUserPage.loadingInfo')}</h2>
          </div>
        </div>
      </PageTemplate>
    );
  }

  if (error) {
    return (
      <PageTemplate title={t('common.error')}>
        <div className="football-team-page">
          <div className="error-state">
            <h2>{t('common.error')}</h2>
            <p>{error}</p>
            <button onClick={() => navigate(-1)} className="back-button">
              ← {t('common.goBack')}
            </button>
          </div>
        </div>
      </PageTemplate>
    );
  }

  if (!team) {
    return (
      <PageTemplate title={t('teamUserPage.notFoundTitle')}>
        <div className="football-team-page">
          <div className="not-found-state">
            <h2>{t('teamUserPage.notFound')}</h2>
            <p>{t('teamUserPage.notFoundDesc')}</p>
            <button onClick={() => navigate(-1)} className="back-button">
              ← {t('common.goBack')}
            </button>
          </div>
        </div>
      </PageTemplate>
    );
  }

  const handleBackToClub = () => {
    const clubSlug = createClubSlug(team.club);
    navigate(`/club/${clubSlug}`);
  };

  const handleTabChange = (tabId: string) => {
    setActiveTab(tabId);
    
    if ((tabId === 'stats' || tabId === 'standings' || tabId === 'roster') && !fetchedTabs.has(tabId)) {
      fetchTabData(tabId);
      setFetchedTabs(prev => new Set([...prev, tabId]));
    }
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const renderTabContent = () => {
    switch (activeTab) {
      case 'summary':
        return (
          <SummarySection
            team={team}
            matches={matches || []}
          ></SummarySection>
        );

      case 'results':
        return (
          <ResultsSection
            matchesLoading={matchesLoading}
            matchesError={matchesError}
            matches={matches}
            team={team}
            currentPage={currentPage}
            totalPages={totalPages}
            handlePageChange={handlePageChange}
          ></ResultsSection>
        );

      case 'roster':
        return (
          <div className="roster-section">
            <RosterSection
              team={team}
              playerStatistics={playerStatistics}
            />
          </div>
        );

      case 'stats':
        return (
          <Statistics
            teamStatistics={teamStatistics}
            playerStatistics={playerStatistics}
            roster={team.roster}
            loading={statisticsLoading}
            error={statisticsError}
            seasonName={currentSeason?.name}
          />
        );

      case 'standings':
        return (
          <FootballLeagueStanding 
            seasonSummary={seasonSummary}
            loading={seasonSummaryLoading}
            error={seasonSummaryError}
          />
        );

      default:
        return (
          <div className="default-section">
            <h3>Select a tab to view content</h3>
          </div>
        );
    }
  };

  return (
    <PageTemplate title={team.name}>
      <div className="football-team-page">
        <nav className="football-team-page__crumb" aria-label={team.club.name}>
          <button type="button" className="football-team-page__crumb-link" onClick={handleBackToClub}>
            {team.club.name}
          </button>
          <span aria-hidden="true">›</span>
          <span className="football-team-page__crumb-current">{team.name}</span>
        </nav>

        <CompetitionHero
          title={team.name}
          logoUrl={team.logoUrl || team.club.logoUrl}
          markLabel={teamMarkLabel(team.name)}
          meta={currentSeason ? (
            <button
              type="button"
              className="football-team-page__season"
              onClick={() => navigate(`/football/league/${currentSeason.id}`)}
            >
              {currentSeason.name}
            </button>
          ) : null}
        />

        <TeamNavbar currentTab={activeTab} onTabChange={handleTabChange} />

        <div
          className="tab-content-container"
          role="tabpanel"
          id={`tabpanel-${activeTab}`}
          aria-labelledby={`tab-${activeTab}`}
        >
          {renderTabContent()}
        </div>
      </div>
    </PageTemplate>
  );
}

export default FootballTeamPage; 
