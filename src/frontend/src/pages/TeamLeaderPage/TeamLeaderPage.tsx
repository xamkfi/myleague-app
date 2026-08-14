import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import TeamLeaderPageTemplate from './components/TeamLeaderPageTemplate';
import { teamLeaderService } from '../../api/teamLeader/teamLeaderService';
import type { TeamLeaderTeam } from '../../types/teamLeader/teamLeaderTypes';
import type { FloorballMatchDto } from '../../types/floorball/floorballTypes';
import type { FootballMatchDto } from '../../types/football/footballTypes';
import './TeamLeaderPage.scss';

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

function TeamLeaderPage() {
  const { t, i18n } = useTranslation();
  const [teams, setTeams] = useState<TeamLeaderTeam[]>([]);
  const [upcomingByTeam, setUpcomingByTeam] = useState<UpcomingByTeam>({});
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      try {
        const myTeams = await teamLeaderService.getMyTeams();
        if (cancelled) return;
        setTeams(myTeams);

        const matchEntries = await Promise.all(
          myTeams.map(async (team): Promise<[string, UpcomingMatchSummary[]]> => {
            try {
              if (team.sport === 'floorball') {
                const matches = await teamLeaderService.getFloorballUpcomingMatches(team.teamId);
                return [team.teamId, matches.map((m) => toFloorballSummary(m, team.teamId))];
              }
              const matches = await teamLeaderService.getFootballUpcomingMatches(team.teamId);
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
          setError(err instanceof Error ? err.message : t('teamLeader.loadError', 'Failed to load your teams'));
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
    <TeamLeaderPageTemplate title={t('teamLeader.myTeams', 'My teams')}>
      {isLoading && <div className="team-leader-loading">{t('common.loading', 'Loading...')}</div>}

      {error && <div className="team-leader-error">{error}</div>}

      {!isLoading && !error && teams.length === 0 && (
        <div className="team-leader-empty">
          {t('teamLeader.noTeams', 'You are not currently assigned as a team leader for any team. Please contact an administrator.')}
        </div>
      )}

      <div className="team-leader-team-grid">
        {teams.map((team) => {
          const upcoming = upcomingByTeam[team.teamId] ?? [];
          return (
            <div key={`${team.sport}-${team.teamId}`} className="team-leader-team-card">
              <div className="team-leader-team-card-header">
                {team.logoUrl && <img src={team.logoUrl} alt="" className="team-leader-team-logo" />}
                <div>
                  <h2 className="team-leader-team-name">{team.name}</h2>
                  <span className={`team-leader-sport-badge team-leader-sport-badge--${team.sport}`}>
                    {team.sport === 'floorball'
                      ? t('teamLeader.sportFloorball', 'Floorball')
                      : t('teamLeader.sportFootball', 'Football')}
                  </span>
                </div>
              </div>

              <Link
                to={`/team-leader/teams/${team.sport}/${team.teamId}/roster`}
                className="team-leader-button team-leader-button--primary"
              >
                {t('teamLeader.editRoster', 'Roster & jersey numbers')}
              </Link>

              <h3 className="team-leader-upcoming-title">{t('teamLeader.upcomingMatches', 'Upcoming matches')}</h3>
              {upcoming.length === 0 ? (
                <p className="team-leader-upcoming-empty">{t('teamLeader.noUpcomingMatches', 'No upcoming matches.')}</p>
              ) : (
                <ul className="team-leader-upcoming-list">
                  {upcoming.map((match) => (
                    <li key={match.matchId} className="team-leader-upcoming-item">
                      <div className="team-leader-upcoming-info">
                        <span className="team-leader-upcoming-teams">
                          {match.homeTeamName ?? 'TBD'} – {match.awayTeamName ?? 'TBD'}
                        </span>
                        <span className="team-leader-upcoming-meta">
                          {formatDate(match.scheduledDateTime)}
                          {match.venue ? ` · ${match.venue}` : ''}
                        </span>
                      </div>
                      <div className="team-leader-upcoming-actions">
                        {match.hasAnnouncedRoster && (
                          <span className="team-leader-announced-badge">
                            {t('teamLeader.rosterAnnounced', 'Roster announced')}
                          </span>
                        )}
                        <Link
                          to={`/team-leader/teams/${team.sport}/${team.teamId}/matches/${match.matchId}/roster`}
                          className="team-leader-button team-leader-button--secondary"
                        >
                          {match.hasAnnouncedRoster
                            ? t('teamLeader.editAnnouncedRoster', 'Edit roster')
                            : t('teamLeader.announceRoster', 'Announce roster')}
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
    </TeamLeaderPageTemplate>
  );
}

export default TeamLeaderPage;
