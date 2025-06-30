import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import type { FloorballMatchDto, FloorballTeam } from '../../types/floorball/floorballTypes';
import { floorballTeamService } from '../../api/floorball/floorballTeamService';
import { findTeamBySlug, createClubSlug } from '../../utils/slugUtils';
import './FloorballTeamPage.scss';
import { divisionService } from '../../api/common/divisionService';
import type { DivisionType } from '../../types/common/divisionType';
import type { FloorballMatch } from '../../api/admin/News/GetMatchesService';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import TeamNavbar from './components/teamNavbar';

function FloorballTeamPage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();

  const [team, setTeam] = useState<FloorballTeam | null>(null);
  const [division, setDivision] = useState<DivisionType | null>(null)
  const [matches, setMatches] = useState<FloorballMatchDto[] | null>(null)
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<string>('results');
  const [matchesLoading, setMatchesLoading] = useState(false);
  const [matchesError, setMatchesError] = useState<string | null>(null);
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
        const teamsResponse = await floorballTeamService.getAll({});
        const allTeams = teamsResponse.data || [];

        // Find team by slug
        const foundTeam = findTeamBySlug(allTeams, slug);

        if (foundTeam) {
          setTeam(foundTeam);

          // Fetch division the team is in
          const divisionResponse = await divisionService.getById(foundTeam.divisionId)
          setDivision(divisionResponse.data)

          // Note: Match fetching moved to separate useEffect for pagination

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
          pageSize: 10
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

  if (loading) {
    return (
      <PageTemplate title="Loading...">
        <div className="floorball-team-page">
          <div className="loading-state">
            <h2>Loading team information...</h2>
          </div>
        </div>
      </PageTemplate>
    );
  }

  if (error) {
    return (
      <PageTemplate title="Error">
        <div className="floorball-team-page">
          <div className="error-state">
            <h2>Error</h2>
            <p>{error}</p>
            <button onClick={() => navigate(-1)} className="back-button">
              ← Go Back
            </button>
          </div>
        </div>
      </PageTemplate>
    );
  }

  if (!team) {
    return (
      <PageTemplate title="Team Not Found">
        <div className="floorball-team-page">
          <div className="not-found-state">
            <h2>Team not found</h2>
            <p>The team you are looking for does not exist.</p>
            <button onClick={() => navigate(-1)} className="back-button">
              ← Go Back
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
    console.log('Active tab changed to:', tabId);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const renderTabContent = () => {
    switch (activeTab) {
      case 'results':
        return (
          <div className="results-section">
            {matchesLoading ? (
              <div className="loading-state">Loading matches...</div>
            ) : matchesError ? (
              <div className="error-state">
                <p>{matchesError}</p>
                <button onClick={() => setCurrentPage(1)} className="retry-button">
                  Retry
                </button>
              </div>
            ) : matches && matches.length > 0 ? (
              <>
                <div className="matches-grid">
                  {matches.map((match) => (
                    <div key={match.id} className="match-row">
                                             <div className="match-date">
                         {new Date(match.scheduledDateTime).toLocaleDateString('en-GB', {
                           day: '2-digit',
                           month: '2-digit',
                         })} {new Date(match.scheduledDateTime).toLocaleTimeString('en-GB', {
                           hour: '2-digit',
                           minute: '2-digit'
                         })}
                       </div>
                       
                       <div className="teams-section">
                         <div className="team home-team">
                           <span className="team-name">{match.homeTeamName}</span>
                           <span className="team-score">{match.homeScore}</span>
                         </div>
                         <div className="team away-team">
                           <span className="team-name">{match.awayTeamName}</span>
                           <span className="team-score">{match.awayScore}</span>
                         </div>
                       </div>

                       <div className="match-status">
                         {match.status === 'Completed' ? (
                           <span className={`result-badge ${
                             (match.homeTeamId === team?.id && match.homeScore > match.awayScore) ||
                             (match.awayTeamId === team?.id && match.awayScore > match.homeScore) 
                               ? 'win' : 'loss'
                           }`}>
                             {(match.homeTeamId === team?.id && match.homeScore > match.awayScore) ||
                              (match.awayTeamId === team?.id && match.awayScore > match.homeScore) 
                                ? 'W' : 'L'}
                           </span>
                         ) : (
                           <span className="status-badge">{match.status}</span>
                         )}
                       </div>
                    </div>
                  ))}
                </div>
                
                {totalPages > 1 && (
                  <div className="pagination">
                    <button 
                      onClick={() => handlePageChange(currentPage - 1)}
                      disabled={currentPage === 1}
                      className="pagination-btn"
                    >
                      Previous
                    </button>
                    
                    <span className="page-info">
                      Page {currentPage} of {totalPages}
                    </span>
                    
                    <button 
                      onClick={() => handlePageChange(currentPage + 1)}
                      disabled={currentPage === totalPages}
                      className="pagination-btn"
                    >
                      Next
                    </button>
                  </div>
                )}
              </>
            ) : (
              <div className="no-matches">
                <p>No matches found for this team.</p>
              </div>
            )}
          </div>
        );
      
      case 'roster':
        return (
          <div className="roster-section">
            <h3>🚧 Team Roster</h3>
            <p>Roster information coming soon...</p>
          </div>
        );
      
      case 'stats':
        return (
          <div className="stats-section">
            <h3>🚧 Team Statistics</h3>
            <p>Team statistics coming soon...</p>
          </div>
        );
      
      case 'standings':
        return (
          <div className="standings-section">
            <h3>🚧 League Standings</h3>
            <p>League standings coming soon...</p>
          </div>
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
            ← Back to {team.club.name}
          </button>
        </div>

        {/* Team Header */}
        <div className="team-header">
          <div className="header-content">

            <div className="team-branding">
              <div className="team-logo">
                <div className="logo-placeholder">
                  {/* Team logo would go here */}
                </div>
              </div>
              <div className="jersey-colors">
                <div className="color-info">
                  <span>Jersey Colors:</span>
                  <div className="colors">
                    <div
                      className="color-swatch primary"
                      style={{ backgroundColor: team.primaryJerseyColor.toLowerCase() }}
                      title={`Primary: ${team.primaryJerseyColor}`}
                    ></div>
                    {team.secondaryJerseyColor && (
                      <div
                        className="color-swatch secondary"
                        style={{ backgroundColor: team.secondaryJerseyColor.toLowerCase() }}
                        title={`Secondary: ${team.secondaryJerseyColor}`}
                      ></div>
                    )}
                  </div>
                </div>
              </div>
            </div>

            <div className="team-info">
              <h1>{team.name}</h1>
              <div className="team-meta">
                <span className="division-badge">
                  {division?.name}
                </span>
                <span className="club-badge">
                  Club: {team.club.name}
                </span>
                <span className="arena">Arena: {team.homeArena}</span>
              </div>
            </div>

            
          </div>

          <div className="header-navigation">
            <TeamNavbar onTabChange={handleTabChange} />
          </div>
        </div>


        {/* Tab Content */}
        <div className="tab-content">
          {renderTabContent()}
        </div>

        
      </div>
    </PageTemplate>
  );
}

export default FloorballTeamPage; 
