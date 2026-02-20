import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import type { FloorballMatchDto, FloorballTeam } from '../../types/floorball/floorballTypes';
import { floorballTeamNameSearchService } from '../../api/floorball/floorballTeamNameSearchService';
import { floorballTeamService } from '../../api/floorball/floorballTeamService';
import { findTeamBySlug, createClubSlug } from '../../utils/slugUtils';
import './FloorballTeamPage.scss';
import { divisionService } from '../../api/common/divisionService';
import type { DivisionType } from '../../types/common/divisionType';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import { floorballStatisticsService, type FloorballTeamSeasonStatisticsDto, type FloorballSeasonStatisticsSummaryDto } from '../../api/floorball/floorballStatistics';
import { floorballSeasonService, type FloorballSeasonDto } from '../../api/floorball/floorballSeasonService';
import TeamNavbar from './components/TeamNavbar';
import ResultsSection from './components/ResultsSection';
import { useTranslation } from 'react-i18next';
import RosterSection from './components/RosterSection';
import SummarySection from './components/SummarySection';
import Statistics from './components/Statistics';
import LeagueStanding from '../../components/LeagueStanding/LeagueStanding';

function FloorballTeamPage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [team, setTeam] = useState<FloorballTeam | null>(null);
  const [division, setDivision] = useState<DivisionType | null>(null)
  const [matches, setMatches] = useState<FloorballMatchDto[] | null>(null)
  const [teamStatistics, setTeamStatistics] = useState<FloorballTeamSeasonStatisticsDto | null>(null);
  const [seasonSummary, setSeasonSummary] = useState<FloorballSeasonStatisticsSummaryDto | null>(null);
  const [currentSeason, setCurrentSeason] = useState<FloorballSeasonDto | null>(null);
  const [fetchedTabs, setFetchedTabs] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<string>('summary');
  const [matchesLoading, setMatchesLoading] = useState(false);
  const [matchesError, setMatchesError] = useState<string | null>(null);
  const [statisticsLoading, setStatisticsLoading] = useState(false);
  const [statisticsError, setStatisticsError] = useState<string | null>(null);
  const [seasonSummaryLoading, setSeasonSummaryLoading] = useState(false);
  const [seasonSummaryError, setSeasonSummaryError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  // Function to get current season for the team's division
  const getCurrentSeason = async (divisionId: string): Promise<FloorballSeasonDto | null> => {
    try {
      const activeSeasonsResponse = await floorballSeasonService.getActive();
      const activeSeasons = activeSeasonsResponse.data || [];
      
      // Find the active season for this division
      const seasonForDivision = activeSeasons.find(season => 
        season.seasonDivisions?.some(sd => sd.divisionId === divisionId) && season.isActive
      );
      
      return seasonForDivision || null;
    } catch (error) {
      console.error('Error fetching current season:', error);
      return null;
    }
  };

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
        const teamsResponse = await floorballTeamNameSearchService.getTeamNames("");
        const allTeams = teamsResponse.data || [];
        
        // Find team by slug
        const foundTeam = findTeamBySlug(allTeams, slug);

        if (foundTeam) {
          const teamResponse = await floorballTeamService.getById(foundTeam.id);
          setTeam(teamResponse);

          // Fetch division the team is in (division is optional)
          if (teamResponse.divisionId) {
            const divisionResponse = await divisionService.getById(teamResponse.divisionId);
            setDivision(divisionResponse.data);
          } else {
            setDivision(null);
          }

          // Fetch current season for this division (division is optional)
          if (teamResponse.divisionId) {
            const currentSeasonData = await getCurrentSeason(teamResponse.divisionId);
            setCurrentSeason(currentSeasonData);
          } else {
            setCurrentSeason(null);
          }
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
  }, [slug]);

  // Fetch matches with pagination when team changes or page changes
  useEffect(() => {
    const fetchMatches = async () => {
      if (!team) return;

      try {
        setMatchesLoading(true);
        setMatchesError(null);

        const response = await floorballMatchService.getAll({
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
      if (tabId === 'stats') {
        setStatisticsLoading(true);
        setStatisticsError(null);
        const teamStats = await floorballStatisticsService.getTeamStatistics(currentSeason.id, team.id);
        setTeamStatistics(teamStats);
        
      } else if (tabId === 'standings') {
        setSeasonSummaryLoading(true);
        setSeasonSummaryError(null);
        
        // Fetch season summary data (includes standings)
        const seasonSummaryData = await floorballStatisticsService.getSeasonStatistics(currentSeason.id);
        setSeasonSummary(seasonSummaryData);
      }
      
    } catch (error) {
      console.error(`Failed to fetch ${tabId} data:`, error);
      if (tabId === 'stats') {
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
        <div className="floorball-team-page">
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
        <div className="floorball-team-page">
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
        <div className="floorball-team-page">
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
    
    // Fetch data for stats/standings tabs only when first accessed
    if ((tabId === 'stats' || tabId === 'standings') && !fetchedTabs.has(tabId)) {
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
            ></RosterSection>
          </div>
        );

      case 'stats':
        return (
          <Statistics 
            teamStatistics={teamStatistics}
            roster={team.roster}
            loading={statisticsLoading}
            error={statisticsError}
            seasonName={currentSeason?.name}
          />
        );

      case 'standings':
        return (
          <LeagueStanding 
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
      <div className="floorball-team-page">

        {/* Hero Image Background */}
        <div className="hero-image-container">
          <div className="hero-image"></div>
          


          {/* Team Header */}
          <div className="team-header">


            {/* Left-aligned navigation container */}
            <div className="left-navigation-container">
              {/* Breadcrumb Navigation */}
              <div className="breadcrumb">
                <button onClick={handleBackToClub} className="club-link">
                  {team.club.name}
                </button>
                <span className="separator">›</span>
                <span className="current">{team.name}</span>
              </div>

              {/* Navigation */}
              <div className="team-navigation">
                <button onClick={handleBackToClub} className="back-button">
                  ← {team.club.name}
                </button>
              </div>
            </div>
            
            <div className="header-content">
              <div className="team-branding">
                <div className="floorball-page-team-logo">
                  {team.logoUrl ? (
                    <img 
                      // TODO: Use real logo when possible
                      src={"http://www.mahl.fi/media/com_joomleague/clubs/small/myry21_1683621904.jpg"} 
                      alt={`${team.name} logo`}
                      onError={(e) => {
                        // If team logo fails to load, fallback to club logo
                        const target = e.target as HTMLImageElement;
                        if (team.club.logoUrl && target.src !== team.club.logoUrl) {
                          target.src = team.club.logoUrl;
                        } else {
                          // If both fail, hide the img and show placeholder
                          target.style.display = 'none';
                          const placeholder = target.nextElementSibling as HTMLElement;
                          if (placeholder) {
                            placeholder.style.display = 'flex';
                          }
                        }
                      }}
                    />
                  ) : team.club.logoUrl ? (
                    <img 
                      src={team.club.logoUrl} 
                      alt={`${team.club.name} logo`}
                      onError={(e) => {
                        // If club logo fails to load, hide and show placeholder
                        const target = e.target as HTMLImageElement;
                        target.style.display = 'none';
                        const placeholder = target.nextElementSibling as HTMLElement;
                        if (placeholder) {
                          placeholder.style.display = 'flex';
                        }
                      }}
                    />
                  ) : null}
                  <div className="logo-placeholder" style={{ display: (team.logoUrl || team.club.logoUrl) ? 'none' : 'flex' }}>
                    {team.name}
                  </div>
                </div>                
              </div>

              <div className="team-info">
                {/* Team Info */}
                <div className="team-info-container">
                  <h1>{team.name}</h1>
                  <div className="team-meta">
                    {division?.name && (
                      <span className="division-badge">
                        {division.name}
                      </span>
                    )}
                    <span className="club-badge">
                      {team.club.name}
                    </span>
                    {team.homeArena && team.homeArena !== 'TBD' && (
                      <span className="arena">🏟️ {team.homeArena}</span>
                    )}
                    {team.teamCategory && (
                      <span className="category-badge">
                        {team.teamCategory}
                      </span>
                    )}
                  </div>
                  {/* Jersey colors & roster info */}
                  <div className="team-extra-info">
                    <div className="jersey-colors">
                      <span
                        className="jersey-swatch"
                        style={{ backgroundColor: team.primaryJerseyColor.toLowerCase() }}
                        title={`Primary: ${team.primaryJerseyColor}`}
                      />
                      {team.secondaryJerseyColor && (
                        <span
                          className="jersey-swatch"
                          style={{ backgroundColor: team.secondaryJerseyColor.toLowerCase() }}
                          title={`Secondary: ${team.secondaryJerseyColor}`}
                        />
                      )}
                    </div>
                    {team.hasActiveMembers && team.roster.length > 0 && (
                      <span className="roster-count">
                        👥 {team.roster.filter(p => p.isActive).length} players
                      </span>
                    )}
                  </div>
                </div>
                  
                <div className="header-navigation">
                  <TeamNavbar currentTab={activeTab} onTabChange={handleTabChange} />
                </div>

              </div>
              
            </div>

          </div>
        </div>

        {/* Tab Content */}
        <div className="tab-content-container">
            {renderTabContent()}
        </div>


      </div>
    </PageTemplate>
  );
}

export default FloorballTeamPage; 
