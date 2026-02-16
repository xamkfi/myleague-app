import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import type { Club } from '../../api/common/clubService';
import { getClubs } from '../../api/common/clubService';
import type { FloorballTeam } from '../../types/floorball/floorballTypes';
import { findClubBySlug, getTeamSlug } from '../../utils/slugUtils';
import { useDivisions } from '../../hooks/useDivisions';
import { useFloorballTeamsData } from '../../hooks/useTeamsData';
import './ClubPage.scss';

function ClubPage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const { divisions } = useDivisions();
  const [clubs, setClubs] = useState<Club[]>([]);
  const {
    teams,
    setParams: setTeamParams,
    isLoading: teamsLoading,
  } = useFloorballTeamsData();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const clubsData = await getClubs();
        setClubs(clubsData);

        if (slug) {
          let foundClub = findClubBySlug(clubsData, slug);

          // Backwards compatibility: try finding by ID
          if (!foundClub) {
            foundClub = clubsData.find((club) => club.id === slug);
          }

          if (foundClub) {
            setTeamParams({ clubId: foundClub.id });
          }
        }

        setLoading(false);
      } catch {
        setError('Failed to load club information. Please try again later.');
        setLoading(false);
      }
    };

    fetchData();
  }, [slug, setTeamParams]);

  const club = !loading && slug ? findClubBySlug(clubs, slug) : undefined;

  const handleTeamClick = (team: FloorballTeam) => {
    const teamSlug = getTeamSlug(team, teams);
    navigate(`/team/${teamSlug}`);
  };

  const getDivisionDisplayName = (divisionId?: string | null): string => {
    if (!divisionId) return 'No division';
    const division = divisions.find((d) => d.id === divisionId);
    return division?.name || 'Unknown Division';
  };

  if (loading) {
    return (
      <PageTemplate title="Loading...">
        <div className="club-page">
          <div className="loading-state">
            <h2>Loading club information...</h2>
          </div>
        </div>
      </PageTemplate>
    );
  }

  if (error) {
    return (
      <PageTemplate title="Error">
        <div className="club-page">
          <div className="error-state">
            <h2>Error</h2>
            <p>{error}</p>
          </div>
        </div>
      </PageTemplate>
    );
  }

  if (!club) {
    return (
      <PageTemplate title="Club Not Found">
        <div className="club-page">
          <div className="not-found-state">
            <h2>Club not found</h2>
            <p>The club you are looking for does not exist.</p>
          </div>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={club.name}>
      <div className="club-page">
        {/* Hero Section */}
        <div className="hero-image-container">
          <div className="hero-image" />

          <div className="club-header">
            <div className="header-content">
              <div className="club-branding">
                <div className="club-logo">
                  {club.logoUrl ? (
                    <img src={club.logoUrl} alt={`${club.name} logo`} />
                  ) : (
                    <div className="logo-placeholder">
                      {club.name.charAt(0)}
                    </div>
                  )}
                </div>
              </div>

              <div className="club-info">
                <h1 className="club-title">{club.name}</h1>
                {club.websiteUrl && (
                  <div className="club-details">
                    <div className="detail-item">
                      <span className="detail-icon">🌐</span>
                      <a href={club.websiteUrl} target="_blank" rel="noopener noreferrer">
                        Website
                      </a>
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Teams Section */}
        <div className="club-content">
          <h2 className="section-title">Teams</h2>

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
                  role="button"
                  tabIndex={0}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      handleTeamClick(team);
                    }
                  }}
                >
                  <div className="team-card-header">
                    <h4>{team.name}</h4>
                    <div className="jersey-colors">
                      <span
                        className="color-primary"
                        style={{ backgroundColor: team.primaryJerseyColor.toLowerCase() }}
                        title={`Primary: ${team.primaryJerseyColor}`}
                      />
                      {team.secondaryJerseyColor && (
                        <span
                          className="color-secondary"
                          style={{ backgroundColor: team.secondaryJerseyColor.toLowerCase() }}
                          title={`Secondary: ${team.secondaryJerseyColor}`}
                        />
                      )}
                    </div>
                  </div>

                  <div className="team-card-body">
                    <div className="team-info">
                      <span className="division">
                        {getDivisionDisplayName(team.divisionId)}
                      </span>
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
              <p>This club doesn&apos;t have any floorball teams yet.</p>
            </div>
          )}
        </div>
      </div>
    </PageTemplate>
  );
}

export default ClubPage;
