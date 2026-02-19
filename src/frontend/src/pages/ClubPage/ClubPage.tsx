import { useEffect, useState, useMemo, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import type { Club } from '../../api/common/clubService';
import { getClubs } from '../../api/common/clubService';
import type { FloorballTeam } from '../../types/floorball/floorballTypes';
import { findClubBySlug, getTeamSlug } from '../../utils/slugUtils';
import { useDivisions } from '../../hooks/useDivisions';
import { useFloorballTeamsData } from '../../hooks/useTeamsData';
import { floorballSeasonService, type FloorballSeasonDto } from '../../api/floorball/floorballSeasonService';
import './ClubPage.scss';

function ClubPage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { divisions } = useDivisions();
  const [clubs, setClubs] = useState<Club[]>([]);
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
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
        const [clubsData, seasonsResponse] = await Promise.all([
          getClubs(),
          floorballSeasonService.getAll(),
        ]);
        setClubs(clubsData);
        setSeasons(seasonsResponse.data || []);

        if (slug) {
          let foundClub = findClubBySlug(clubsData, slug);
          if (!foundClub) {
            foundClub = clubsData.find((club) => club.id === slug);
          }
          if (foundClub) {
            setTeamParams({ clubId: foundClub.id });
          }
        }

        setLoading(false);
      } catch {
        setError(t('clubPage.errorMessage'));
        setLoading(false);
      }
    };

    fetchData();
  }, [slug, setTeamParams, t]);

  const club = useMemo(
    () => (!loading && slug ? findClubBySlug(clubs, slug) : undefined),
    [loading, slug, clubs]
  );

  const handleTeamClick = useCallback(
    (team: FloorballTeam) => {
      const teamSlug = getTeamSlug(team, teams);
      navigate(`/team/${teamSlug}`);
    },
    [navigate, teams]
  );

  const getDivisionDisplayName = useCallback(
    (divisionId?: string | null): string => {
      if (!divisionId) return 'N/A';
      const division = divisions.find((d) => d.id === divisionId);
      return division?.name || 'Unknown';
    },
    [divisions]
  );

  const getTeamSeasons = useCallback(
    (teamId: string): FloorballSeasonDto[] => {
      return seasons
        .filter((season) =>
          season.seasonDivisions.some((sd) => sd.teamIds.includes(teamId))
        )
        .sort((a, b) => {
          if (a.isActive && !b.isActive) return -1;
          if (!a.isActive && b.isActive) return 1;
          return new Date(b.startDate).getTime() - new Date(a.startDate).getTime();
        });
    },
    [seasons]
  );

  const formatFoundingDate = (dateString?: string | null): string | null => {
    if (!dateString) return null;
    try {
      const date = new Date(dateString);
      return date.getFullYear().toString();
    } catch {
      return null;
    }
  };

  if (loading) {
    return (
      <PageTemplate title={t('clubPage.loading')}>
        <div className="club-page">
          <div className="club-page__state">
            <LoadingSpinner variant="light" text={t('clubPage.loading')} />
          </div>
        </div>
      </PageTemplate>
    );
  }

  if (error) {
    return (
      <PageTemplate title={t('clubPage.errorTitle')}>
        <div className="club-page">
          <div className="club-page__state">
            <h2>{t('clubPage.errorTitle')}</h2>
            <p>{error}</p>
          </div>
        </div>
      </PageTemplate>
    );
  }

  if (!club) {
    return (
      <PageTemplate title={t('clubPage.notFoundTitle')}>
        <div className="club-page">
          <div className="club-page__state">
            <h2>{t('clubPage.notFoundTitle')}</h2>
            <p>{t('clubPage.notFoundMessage')}</p>
            <Link to="/clubs" className="club-page__back-link">
              {t('clubPage.backToClubs')}
            </Link>
          </div>
        </div>
      </PageTemplate>
    );
  }

  const foundingYear = formatFoundingDate(club.foundingDate);

  return (
    <PageTemplate title={club.name}>
      <div className="club-page">
        {/* Hero Section */}
        <div className="club-page__hero">
          <div className="club-page__hero-overlay" />

          <div className="club-page__hero-inner">
            <Link to="/clubs" className="club-page__breadcrumb">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <polyline points="15 18 9 12 15 6" />
              </svg>
              {t('clubPage.backToClubs')}
            </Link>

            <div className="club-page__header">
              <div className="club-page__logo">
                {club.logoUrl ? (
                  <img src={club.logoUrl} alt={`${club.name} logo`} />
                ) : (
                  <div className="club-page__logo-placeholder">
                    {club.name.charAt(0)}
                  </div>
                )}
              </div>

              <div className="club-page__info">
                <h1 className="club-page__title">{club.name}</h1>

                <div className="club-page__meta">
                  {club.city && (
                    <span className="club-page__meta-item">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
                        <circle cx="12" cy="10" r="3" />
                      </svg>
                      {club.city}{club.country ? `, ${club.country}` : ''}
                    </span>
                  )}
                  {foundingYear && (
                    <span className="club-page__meta-item">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
                        <line x1="16" y1="2" x2="16" y2="6" />
                        <line x1="8" y1="2" x2="8" y2="6" />
                        <line x1="3" y1="10" x2="21" y2="10" />
                      </svg>
                      {t('clubPage.founded')} {foundingYear}
                    </span>
                  )}
                  {club.websiteUrl && (
                    <a
                      href={club.websiteUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="club-page__meta-item club-page__meta-item--link"
                    >
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <circle cx="12" cy="12" r="10" />
                        <line x1="2" y1="12" x2="22" y2="12" />
                        <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z" />
                      </svg>
                      {t('clubPage.website')}
                      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="external-icon">
                        <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6" />
                        <polyline points="15 3 21 3 21 9" />
                        <line x1="10" y1="14" x2="21" y2="3" />
                      </svg>
                    </a>
                  )}
                  {club.contactEmail && (
                    <a
                      href={`mailto:${club.contactEmail}`}
                      className="club-page__meta-item club-page__meta-item--link"
                    >
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" />
                        <polyline points="22,6 12,13 2,6" />
                      </svg>
                      {t('clubPage.contact')}
                    </a>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Teams Section */}
        <div className="club-page__content">
          <h2 className="club-page__section-title">{t('clubPage.teams')}</h2>

          {teamsLoading ? (
            <div className="club-page__teams-loading">
              <LoadingSpinner size="sm" text={t('clubPage.teamsLoading')} />
            </div>
          ) : teams.length > 0 ? (
            <div className="club-page__teams-grid">
              {teams.map((team) => {
                const teamSeasons = getTeamSeasons(team.id);
                return (
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
                    <div className="team-card__header">
                      <h4 className="team-card__name">{team.name}</h4>
                      <div className="team-card__colors">
                        <span
                          className="team-card__color"
                          style={{ backgroundColor: team.primaryJerseyColor.toLowerCase() }}
                          title={`Primary: ${team.primaryJerseyColor}`}
                        />
                        {team.secondaryJerseyColor && (
                          <span
                            className="team-card__color"
                            style={{ backgroundColor: team.secondaryJerseyColor.toLowerCase() }}
                            title={`Secondary: ${team.secondaryJerseyColor}`}
                          />
                        )}
                      </div>
                    </div>

                    <div className="team-card__body">
                      <div className="team-card__tags">
                        <span className="team-card__sport">
                          {t('sports.floorball')}
                        </span>
                        <span className="team-card__division">
                          {getDivisionDisplayName(team.divisionId)}
                        </span>
                      </div>

                      {teamSeasons.length > 0 && (
                        <div className="team-card__seasons">
                          {teamSeasons.map((season) => (
                            <span
                              key={season.id}
                              className={`team-card__season ${season.isActive ? 'team-card__season--active' : ''}`}
                            >
                              {season.name}
                              {season.isActive && (
                                <span className="team-card__season-badge">
                                  {t('floorballPage.active')}
                                </span>
                              )}
                            </span>
                          ))}
                        </div>
                      )}

                   
                    </div>

                    <div className="team-card__footer">
                      <span className="team-card__view-link">
                        {t('clubPage.viewTeam')}
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                          <polyline points="9 18 15 12 9 6" />
                        </svg>
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <div className="club-page__no-teams">
              <p>{t('clubPage.noTeams')}</p>
            </div>
          )}
        </div>
      </div>
    </PageTemplate>
  );
}

export default ClubPage;
