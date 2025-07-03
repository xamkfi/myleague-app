import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import type { Club } from '../../api/common/clubService';
import { getClubs } from '../../api/common/clubService';
import type { FloorballTeam } from '../../types/floorball/floorballTypes';
import { findClubBySlug, getTeamSlug } from '../../utils/slugUtils';
import { useDivisions } from '../../hooks/useDivisions';
import './ClubPage.scss';
import { useFloorballTeamsData } from '../../hooks/useTeamsData';

function ClubPage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const { divisions } = useDivisions();
  const [clubs, setClubs] = useState<Club[]>([]);
  const { 
    teams, 
    setParams: setTeamParams, 
    isLoading: teamsLoading } = useFloorballTeamsData();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        
        // Fetch all clubs first (needed for slug resolution)
        const clubsData = await getClubs();
        setClubs(clubsData);
        
        // Find club by slug or ID (backwards compatibility)
        if (slug) {
          let foundClub = findClubBySlug(clubsData, slug);
          
          // If not found by slug, try to find by ID (backwards compatibility)
          if (!foundClub) {
            foundClub = clubsData.find(club => club.id === slug);
          }
          
          if (foundClub) {
            // Tell the teams-hook to (re)load teams for this club
            setTeamParams({
              clubId: foundClub.id
            });
          }
        }
        
        setLoading(false);
             } catch {
         setError('Failed to load club information. Please try again later.');
         setLoading(false);
       }
    };

    fetchData();
  }, [slug]);

  if (loading) {
    return (
      <PageTemplate title="Loading...">
        <div style={{ padding: '2rem', textAlign: 'center' }}>
          <h2>Loading club information...</h2>
        </div>
      </PageTemplate>
    );
  }

  if (error) {
    return (
      <PageTemplate title="Error">
        <div style={{ padding: '2rem', textAlign: 'center' }}>
          <h2>Error</h2>
          <p>{error}</p>
        </div>
      </PageTemplate>
    );
  }

  const club = slug ? findClubBySlug(clubs, slug) : undefined;

  if (!club) {
    return (
      <PageTemplate title="Club Not Found">
        <div style={{ padding: '2rem', textAlign: 'center' }}>
          <h2>Club not found</h2>
          <p>The club you are looking for does not exist.</p>
        </div>
      </PageTemplate>
    );
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  const handleTeamClick = (team: FloorballTeam) => {
    const teamSlug = getTeamSlug(team, teams);
    navigate(`/team/${teamSlug}`);
  };

  const getDivisionDisplayName = (divisionId: string) => {
    const division = divisions.find(d => d.id === divisionId);
    return division?.name || 'Unknown Division';
  };



  return (
    <PageTemplate title={club.name}>
      <div className="club-page">
        <div className="club-info">
          <div className="club-logo">
            {club.logoUrl ? (
              <img src={club.logoUrl} alt={`${club.name} logo`} />
            ) : (
              <div className="logo-placeholder">
                {club.name} Logo
              </div>
            )}
          </div>
          <h2>{club.name}</h2>
          <ul>
            <li><strong>Founded:</strong> {formatDate(club.foundingDate)}</li>
            <li><strong>Location:</strong> {club.city}, {club.country}</li>
            <li><strong>Website:</strong> <a href={club.websiteUrl} target="_blank" rel="noopener noreferrer">{club.websiteUrl}</a></li>
            <li><strong>Contact:</strong> <a href={`mailto:${club.contactEmail}`}>{club.contactEmail}</a></li>
          </ul>
        </div>

        {/* Teams Section */}
        <div className="club-teams">
          <h3>Teams</h3>
          
          {teamsLoading ? (
            <div className="teams-loading">
              <p>Loading teams...</p>
            </div>
          ) : teams.length > 0 ? (
            <div className="teams-grid">
              {teams.map((team) => (
                <div 
                  key={team.id} 
                  className="team-card"
                  onClick={() => handleTeamClick(team)}
                >
                  <div className="team-card-header">
                    <h4>{team.name}</h4>
                    <div className="jersey-colors">
                      <span 
                        className="color-primary"
                        style={{ backgroundColor: team.primaryJerseyColor.toLowerCase() }}
                        data-label="Primary"
                        title={`Primary: ${team.primaryJerseyColor}`}
                      ></span>
                      {team.secondaryJerseyColor && (
                        <span 
                          className="color-secondary"
                          style={{ backgroundColor: team.secondaryJerseyColor.toLowerCase() }}
                          data-label="Secondary"
                          title={`Secondary: ${team.secondaryJerseyColor}`}
                        ></span>
                      )}
                    </div>
                  </div>
                  
                  <div className="team-card-body">
                    <div className="team-info">
                      <span className="division">{getDivisionDisplayName(team.divisionId)}</span>
                    </div>
                    <div className="home-arena">
                      <small>🏟️ {team.homeArena}</small>
                    </div>
                    <div className="team-status">
                      {team.hasActiveMembers ? (
                        <span className="active">✅ Active roster</span>
                      ) : (
                        <span className="inactive">⚠️ No active members</span>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="no-teams">
              <p>This club doesn't have any floorball teams yet.</p>
            </div>
          )}
        </div>
      </div>
    </PageTemplate>
  );
}

export default ClubPage; 