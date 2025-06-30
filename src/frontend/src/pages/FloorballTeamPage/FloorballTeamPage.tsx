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

function FloorballTeamPage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();

  const [team, setTeam] = useState<FloorballTeam | null>(null);
  const [division, setDivision] = useState<DivisionType | null>(null)
  const [matches, setMatches] = useState<FloorballMatchDto[] | null>(null)
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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

          // Fetch matches that the team is participating in
          const matchResponse = await floorballMatchService.getByTeam(foundTeam.id)
          setMatches(matchResponse.data)
          console.log(matches)

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

        {/* Team Header */}
        <div className="team-header">
          <div className="team-info">
            <h1>{team.name}</h1>
            <div className="team-meta">
              <span className="division-badge">
                {/* TODO: Get division name from divisionId */}
                {division?.name}
              </span>
              <span className="arena">🏟️ {team.homeArena}</span>
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

        {/* Coming Soon Content */}
        <div className="coming-soon">
          <div className="placeholder-content">
            <h2>🚧 Team Page Under Construction</h2>
            <p>
              This team page is currently being developed by another team member. 
              Soon you'll be able to see:
            </p>
            <ul>
              <li>📋 Complete team roster with player details</li>
              <li>📊 Team statistics and performance</li>
              <li>📅 Match schedule and results</li>
              <li>📰 Team news and updates</li>
              <li>🏆 Season standings and achievements</li>
            </ul>
            
            <div className="current-info">
              <h3>Current Team Information:</h3>
              <div className="info-grid">
                <div className="info-item">
                  <strong>Club:</strong> {team.club.name}
                </div>
                <div className="info-item">
                  {/* TODO: Get division name from divisionId */}
                  <strong>Division:</strong> {division?.name}
                </div>
                <div className="info-item">
                  <strong>Home Arena:</strong> {team.homeArena}
                </div>
                <div className="info-item">
                  <strong>Primary Jersey:</strong> {team.primaryJerseyColor}
                </div>
                {team.secondaryJerseyColor && (
                  <div className="info-item">
                    <strong>Secondary Jersey:</strong> {team.secondaryJerseyColor}
                  </div>
                )}
                <div className="info-item">
                  <strong>Active Roster:</strong> 
                  {team.hasActiveMembers ? (
                    <span className="status active">✅ Yes</span>
                  ) : (
                    <span className="status inactive">❌ No active members</span>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Navigation */}
        <div className="team-navigation">
          <button onClick={handleBackToClub} className="back-button">
            ← Back to {team.club.name}
          </button>
        </div>
      </div>
    </PageTemplate>
  );
}

export default FloorballTeamPage; 
