import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ClubAdminPageTemplate from './components/ClubAdminPageTemplate';
import { clubAdminService } from '../../api/clubAdmin/clubAdminService';
import type { ClubAdminClub, ClubAdminTeam } from '../../types/clubAdmin/clubAdminTypes';
import type { FloorballMatchDto } from '../../types/floorball/floorballTypes';
import type { FootballMatchDto } from '../../types/football/footballTypes';
import './ClubAdminPage.scss';

interface UpcomingMatchSummary {
  matchId: string;
  scheduledDateTime: string;
  homeTeamName?: string | null;
  awayTeamName?: string | null;
  venue?: string | null;
  hasAnnouncedRoster: boolean;
}

type UpcomingByTeam = Record<string, UpcomingMatchSummary[]>;

function toFloorballSummary(match: FloorballMatchDto, teamId: string): UpcomingMatchSummary {
  const isHome = match.homeTeamId === teamId;
  const players = isHome ? match.homeActivePlayers : match.awayActivePlayers;
  return {
    matchId: match.id,
    scheduledDateTime: match.scheduledDateTime,
    homeTeamName: match.homeTeamName,
    awayTeamName: match.awayTeamName,
    venue: match.venue,
    hasAnnouncedRoster: (players?.length ?? 0) > 0,
  };
}

function toFootballSummary(match: FootballMatchDto, teamId: string): UpcomingMatchSummary {
  const isHome = match.homeTeamId === teamId;
  const lineup = isHome ? match.homeLineup : match.awayLineup;
  return {
    matchId: match.id,
    scheduledDateTime: match.scheduledDateTime,
    homeTeamName: match.homeTeamName,
    awayTeamName: match.awayTeamName,
    venue: match.venue,
    hasAnnouncedRoster: (lineup?.length ?? 0) > 0,
  };
}

function ClubAdminPage() {
  const { t, i18n } = useTranslation();
  const [clubs, setClubs] = useState<ClubAdminClub[]>([]);
  const [upcomingByTeam, setUpcomingByTeam] = useState<UpcomingByTeam>({});
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      try {
        const myClubs = await clubAdminService.getMyClubs();
        if (cancelled) return;
        setClubs(myClubs);

        const allTeams: ClubAdminTeam[] = myClubs.flatMap((club) => club.teams);
        const matchEntries = await Promise.all(
          allTeams.map(async (team): Promise<[string, UpcomingMatchSummary[]]> => {
            try {
              if (team.sport === 'floorball') {
                const matches = await clubAdminService.getFloorballUpcomingMatches(team.teamId);
                return [team.teamId, matches.map((m) => toFloorballSummary(m, team.teamId))];
              }
              const matches = await clubAdminService.getFootballUpcomingMatches(team.teamId);
              return [team.teamId, matches.map((m) => toFootballSummary(m, team.teamId))];
            } catch {
              return [team.teamId, []];
            }
          }),
        );
        if (cancelled) return;
        setUpcomingByTeam(Object.fromEntries(matchEntries));
      } catch (err: unknown) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : t('clubAdmin.loadError', 'Failed to load your clubs'));
        }
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    };

    void load();
    return () => { cancelled = true; };
  }, [t]);

  const formatDate = (iso: string): string => {
    const date = new Date(iso);
    return date.toLocaleString(i18n.language === 'fi' ? 'fi-FI' : 'en-GB', {
      day: 'numeric', month: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit',
    });
  };

  return (
    <ClubAdminPageTemplate title={t('clubAdmin.myClubs', 'My clubs')}>
      {isLoading && <div className="club-admin-loading">{t('common.loading', 'Loading...')}</div>}

      {error && <div className="club-admin-error">{error}</div>}

      {!isLoading && !error && clubs.length === 0 && (
        <div className="club-admin-empty">
          {t('clubAdmin.noClubs', 'You are not currently assigned as a club admin for any club. Please contact an administrator.')}
        </div>
      )}

      {clubs.map((club) => (
        <section key={club.clubId} className="club-admin-club-section">
          <div className="club-admin-club-header">
            <div className="club-admin-club-identity">
              {club.logoUrl && <img src={club.logoUrl} alt="" className="club-admin-club-logo" />}
              <div>
                <h2 className="club-admin-club-name">{club.name}</h2>
                {club.city && <span className="club-admin-club-city">{club.city}</span>}
              </div>
            </div>
            <Link
              to={`/club-admin/clubs/${club.clubId}/info`}
              className="club-admin-button club-admin-button--secondary"
            >
              {t('clubAdmin.editClubInfo', 'Edit club information')}
            </Link>
          </div>

          {club.teams.length === 0 ? (
            <p className="club-admin-upcoming-empty">{t('clubAdmin.noTeams', 'This club has no teams yet.')}</p>
          ) : (
            <div className="club-admin-team-grid">
              {club.teams.map((team) => {
                const upcoming = upcomingByTeam[team.teamId] ?? [];
                return (
                  <div key={`${team.sport}-${team.teamId}`} className="club-admin-team-card">
                    <div className="club-admin-team-card-header">
                      {team.logoUrl && <img src={team.logoUrl} alt="" className="club-admin-team-logo" />}
                      <div>
                        <h3 className="club-admin-team-name">{team.name}</h3>
                        <span className={`club-admin-sport-badge club-admin-sport-badge--${team.sport}`}>
                          {team.sport === 'floorball'
                            ? t('clubAdmin.sportFloorball', 'Floorball')
                            : t('clubAdmin.sportFootball', 'Football')}
                        </span>
                      </div>
                    </div>

                    <Link
                      to={`/club-admin/teams/${team.sport}/${team.teamId}/roster`}
                      className="club-admin-button club-admin-button--primary"
                    >
                      {t('clubAdmin.editRoster', 'Roster & jersey numbers')}
                    </Link>

                    <h4 className="club-admin-upcoming-title">{t('clubAdmin.upcomingMatches', 'Upcoming matches')}</h4>
                    {upcoming.length === 0 ? (
                      <p className="club-admin-upcoming-empty">{t('clubAdmin.noUpcomingMatches', 'No upcoming matches.')}</p>
                    ) : (
                      <ul className="club-admin-upcoming-list">
                        {upcoming.map((match) => (
                          <li key={match.matchId} className="club-admin-upcoming-item">
                            <div className="club-admin-upcoming-info">
                              <span className="club-admin-upcoming-teams">
                                {match.homeTeamName ?? 'TBD'} – {match.awayTeamName ?? 'TBD'}
                              </span>
                              <span className="club-admin-upcoming-meta">
                                {formatDate(match.scheduledDateTime)}
                                {match.venue ? ` · ${match.venue}` : ''}
                              </span>
                            </div>
                            <div className="club-admin-upcoming-actions">
                              {match.hasAnnouncedRoster && (
                                <span className="club-admin-announced-badge">
                                  {t('clubAdmin.rosterAnnounced', 'Roster announced')}
                                </span>
                              )}
                              <Link
                                to={`/club-admin/teams/${team.sport}/${team.teamId}/matches/${match.matchId}/roster`}
                                className="club-admin-button club-admin-button--secondary"
                              >
                                {match.hasAnnouncedRoster
                                  ? t('clubAdmin.editAnnouncedRoster', 'Edit roster')
                                  : t('clubAdmin.announceRoster', 'Announce roster')}
                              </Link>
                            </div>
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                );
              })}
            </div>
          )}
        </section>
      ))}
    </ClubAdminPageTemplate>
  );
}

export default ClubAdminPage;
