import { useEffect, useState, useMemo } from 'react';
import { useParams, Link } from 'react-router-dom';
import { FootballPosition } from '../../types/football/footballTypes';
import {
  footballPlayerService,
  type FootballPlayerWithMatchesDto,
  type FootballPlayerMatchDto,
} from '../../api/football/footballPlayerService';
import {
  footballStatisticsService,
  type FootballPlayerProfileDto,
  type FootballPlayerSeasonStatisticsDto,
} from '../../api/football/footballStatistics';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { slugify } from '../../utils/slugUtils';
import { useTranslation } from 'react-i18next';
import './FootballPlayerPage.scss';

const getPositionText = (position: FootballPosition | string, t: (key: string, fallback: string) => string): string => {
  switch (position) {
    case FootballPosition.Goalkeeper:
    case 'Goalkeeper':
      return t('football.positions.Goalkeeper', 'Goalkeeper');
    case FootballPosition.Defender:
    case 'Defender':
      return t('football.positions.Defender', 'Defender');
    case FootballPosition.Midfielder:
    case 'Midfielder':
      return t('football.positions.Midfielder', 'Midfielder');
    case FootballPosition.Forward:
    case 'Forward':
      return t('football.positions.Forward', 'Forward');
    default:
      return t('football.positions.Player', 'Player');
  }
};

const formatDate = (dateStr: string): string => {
  const date = new Date(dateStr);
  return date.toLocaleDateString('fi-FI', {
    weekday: 'short',
    day: 'numeric',
    month: 'numeric',
    year: 'numeric',
  });
};

const calculateAge = (birthDate: string | null): number | null => {
  if (!birthDate) return null;
  const birth = new Date(birthDate);
  const today = new Date();
  let age = today.getFullYear() - birth.getFullYear();
  const monthDiff = today.getMonth() - birth.getMonth();
  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
    age--;
  }
  return age;
};

const formatBirthDate = (birthDate: string | null): string => {
  if (!birthDate) return '';
  const date = new Date(birthDate);
  return date.toLocaleDateString('fi-FI');
};

interface SeasonTotals {
  gamesPlayed: number;
  goals: number;
  assists: number;
  points: number;
  yellowCards: number;
  redCards: number;
}

const calculateSeasonTotals = (stats: FootballPlayerSeasonStatisticsDto[]): SeasonTotals => {
  return stats.reduce(
    (total, s) => ({
      gamesPlayed: total.gamesPlayed + s.gamesPlayed,
      goals: total.goals + s.goals,
      assists: total.assists + s.assists,
      points: total.points + s.points,
      yellowCards: total.yellowCards + s.yellowCards,
      redCards: total.redCards + s.redCards,
    }),
    { gamesPlayed: 0, goals: 0, assists: 0, points: 0, yellowCards: 0, redCards: 0 },
  );
};

interface MatchTotals {
  goals: number;
  assists: number;
  yellowCards: number;
  redCards: number;
}

const calculateMatchTotals = (matches: FootballPlayerMatchDto[]): MatchTotals => {
  return matches.reduce(
    (total, m) => ({
      goals: total.goals + (m.playerStats?.goals ?? 0),
      assists: total.assists + (m.playerStats?.assists ?? 0),
      yellowCards: total.yellowCards + (m.playerStats?.yellowCards ?? 0),
      redCards: total.redCards + (m.playerStats?.redCards ?? 0),
    }),
    { goals: 0, assists: 0, yellowCards: 0, redCards: 0 },
  );
};

const MATCHES_PER_PAGE = 20;

function FootballPlayerPage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const [profile, setProfile] = useState<FootballPlayerProfileDto | null>(null);
  const [matchData, setMatchData] = useState<FootballPlayerWithMatchesDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [matchPage, setMatchPage] = useState(1);

  useEffect(() => {
    const loadPlayerData = async () => {
      if (!id) return;

      try {
        setLoading(true);
        setError(null);

        const [profileResult, matchesResult] = await Promise.all([
          footballStatisticsService.getPlayerProfile(id),
          footballPlayerService.getPlayerMatches(id, 50),
        ]);

        setProfile(profileResult);
        setMatchData(matchesResult);
      } catch (err) {
        console.error('Error loading player data:', err);
        setError(err instanceof Error ? err.message : t('football.player.loadError', 'Failed to load player data'));
      } finally {
        setLoading(false);
      }
    };

    void loadPlayerData();
  }, [id, t]);

  const seasonStats = useMemo(() => profile?.seasonStatistics ?? [], [profile]);
  const matches = useMemo(() => matchData?.recentMatches ?? [], [matchData]);
  const totals = useMemo(() => calculateSeasonTotals(seasonStats), [seasonStats]);
  const matchTotals = useMemo(() => calculateMatchTotals(matches), [matches]);
  const totalMatchPages = Math.max(1, Math.ceil(matches.length / MATCHES_PER_PAGE));
  const paginatedMatches = useMemo(
    () => matches.slice((matchPage - 1) * MATCHES_PER_PAGE, matchPage * MATCHES_PER_PAGE),
    [matches, matchPage],
  );

  if (loading) {
    return <PageTemplate title={t('football.player.title', 'Player')}><div className="player-loading">{t('common.loading', 'Loading...')}</div></PageTemplate>;
  }
  if (error) {
    return <PageTemplate title={t('football.player.title', 'Player')}><div className="player-error">{error}</div></PageTemplate>;
  }
  if (!profile) {
    return <PageTemplate title={t('football.player.title', 'Player')}><div className="player-error">{t('football.player.notFound', 'Player not found')}</div></PageTemplate>;
  }

  const { player } = profile;
  const playerName = player.person.fullName;
  const age = calculateAge(player.person.birthDate);
  const teamName = matchData?.teamName ?? player.team?.name ?? '';
  const position = matchData?.position ?? player.position;
  const jerseyNumber = matchData?.jerseyNumber;

  return (
    <PageTemplate title={playerName}>
      <div className="player-page">
        <div className="player-container">
          <div className="player-info-layout">
            <div className="player-info-box">
              <div className="player-avatar-large">
                {seasonStats[0]?.teamLogo ? (
                  <img className="team-logo-img" src={seasonStats[0].teamLogo} alt={teamName} />
                ) : null}
              </div>
              <div className="player-details">
                <div className="player-name">{playerName}</div>
                <div className="player-details-row">
                  {teamName && <span className="player-team">{teamName}</span>}
                  <span className="player-position">{getPositionText(position, t)}</span>
                  {jerseyNumber != null && <span className="player-jersey">#{jerseyNumber}</span>}
                </div>
              </div>
            </div>

            <div className="player-stats-box">
              {age !== null && (
                <div className="stat-item">
                  <span className="stat-label">{t('football.player.age', 'Age')}:</span>
                  <span className="stat-value">{age} ({formatBirthDate(player.person.birthDate)})</span>
                </div>
              )}
              <div className="stat-item">
                <span className="stat-label">{t('football.player.status', 'Status')}:</span>
                <span className={`stat-value ${player.isActive ? 'active' : 'inactive'}`}>
                  {player.isActive ? t('football.player.active', 'Active') : t('football.player.inactive', 'Inactive')}
                </span>
              </div>
              <div className="stat-item">
                <span className="stat-label">{t('football.player.position', 'Position')}:</span>
                <span className="stat-value">{getPositionText(position, t)}</span>
              </div>
            </div>
          </div>
        </div>

        <div className="player-container">
          <div className="career-stats-section">
            <h3>{t('football.player.careerStats', 'Career statistics')}</h3>
            <div className="stats-grid">
              <div className="stats-box">
                <div className="stats-value">{totals.gamesPlayed}</div>
                <div className="stats-label">{t('football.player.games', 'Games')}</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.goals}</div>
                <div className="stats-label">{t('football.player.goals', 'Goals')}</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.assists}</div>
                <div className="stats-label">{t('football.player.assists', 'Assists')}</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.points}</div>
                <div className="stats-label">{t('football.player.points', 'Points')}</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.yellowCards}</div>
                <div className="stats-label">{t('football.stats.yellowCards', 'Yellow cards')}</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.redCards}</div>
                <div className="stats-label">{t('football.stats.redCards', 'Red cards')}</div>
              </div>
            </div>
          </div>
        </div>

        <div className="player-container">
          <div className="section-block">
            <h3>{t('football.player.matchHistory', 'Match history')}</h3>
            {matches.length > 0 ? (
              <>
                <div className="stats-table-scroll">
                  <table className="stats-table">
                    <thead>
                      <tr>
                        <th className="col-date">{t('football.player.date', 'Date')}</th>
                        <th className="col-league">{t('football.player.competition', 'Competition')}</th>
                        <th className="col-team">{t('football.player.home', 'Home')}</th>
                        <th className="col-score">{t('football.player.score', 'Score')}</th>
                        <th className="col-team">{t('football.player.away', 'Away')}</th>
                        <th className="col-num">G</th>
                        <th className="col-num">A</th>
                        <th className="col-num">YC</th>
                        <th className="col-num">RC</th>
                      </tr>
                    </thead>
                    <tbody>
                      {paginatedMatches.map((match) => (
                        <tr key={match.id}>
                          <td className="col-date">{formatDate(match.scheduledDateTime)}</td>
                          <td className="col-league">
                            <Link to={`/football/league/${match.competitionId}`} className="team-link">{match.competitionName}</Link>
                          </td>
                          <td className="col-team">
                            <Link to={`/football/team/${slugify(match.homeTeamName)}`} className="team-link">{match.homeTeamName}</Link>
                          </td>
                          <td className="col-score">{match.homeScore} - {match.awayScore}</td>
                          <td className="col-team">
                            <Link to={`/football/team/${slugify(match.awayTeamName)}`} className="team-link">{match.awayTeamName}</Link>
                          </td>
                          <td className="col-num">{match.playerStats?.goals ?? 0}</td>
                          <td className="col-num">{match.playerStats?.assists ?? 0}</td>
                          <td className="col-num">{match.playerStats?.yellowCards ?? 0}</td>
                          <td className="col-num">{match.playerStats?.redCards ?? 0}</td>
                        </tr>
                      ))}
                    </tbody>
                    <tfoot>
                      <tr className="totals-row">
                        <td colSpan={5}>{t('football.player.matchesTotal', 'Matches')}: {matches.length}</td>
                        <td className="col-num">{matchTotals.goals}</td>
                        <td className="col-num">{matchTotals.assists}</td>
                        <td className="col-num">{matchTotals.yellowCards}</td>
                        <td className="col-num">{matchTotals.redCards}</td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
                {totalMatchPages > 1 && (
                  <div className="pagination">
                    <button className="pagination-btn" disabled={matchPage === 1} onClick={() => setMatchPage(1)}>&laquo;</button>
                    <button className="pagination-btn" disabled={matchPage === 1} onClick={() => setMatchPage((p) => p - 1)}>&lsaquo;</button>
                    <span className="pagination-info">{matchPage} / {totalMatchPages}</span>
                    <button className="pagination-btn" disabled={matchPage === totalMatchPages} onClick={() => setMatchPage((p) => p + 1)}>&rsaquo;</button>
                    <button className="pagination-btn" disabled={matchPage === totalMatchPages} onClick={() => setMatchPage(totalMatchPages)}>&raquo;</button>
                  </div>
                )}
              </>
            ) : (
              <p className="no-data-message">{t('football.player.noMatches', 'No match history available.')}</p>
            )}
          </div>
        </div>

        <div className="player-container">
          <div className="section-block">
            <h3>{t('football.player.seasonStats', 'Season statistics')}</h3>
            {seasonStats.length > 0 ? (
              <div className="stats-table-scroll">
                <table className="stats-table">
                  <thead>
                    <tr>
                      <th className="col-season">{t('football.player.season', 'Season')}</th>
                      <th className="col-team">{t('football.player.team', 'Team')}</th>
                      <th className="col-num">GP</th>
                      <th className="col-num">G</th>
                      <th className="col-num">A</th>
                      <th className="col-num">PTS</th>
                      <th className="col-num">YC</th>
                      <th className="col-num">RC</th>
                    </tr>
                  </thead>
                  <tbody>
                    {seasonStats.map((stat) => (
                      <tr key={stat.id}>
                        <td className="col-season">
                          <Link to={`/football/league/${stat.competitionId}`} className="team-link">{stat.seasonName}</Link>
                        </td>
                        <td className="col-team">
                          <div className="team-cell">
                            {stat.teamLogo && (
                              <img src={stat.teamLogo} alt={stat.teamName} className="team-logo-small" />
                            )}
                            <Link to={`/football/team/${slugify(stat.teamName)}`} className="team-link">{stat.teamName}</Link>
                          </div>
                        </td>
                        <td className="col-num">{stat.gamesPlayed}</td>
                        <td className="col-num">{stat.goals}</td>
                        <td className="col-num">{stat.assists}</td>
                        <td className="col-num">{stat.points}</td>
                        <td className="col-num">{stat.yellowCards}</td>
                        <td className="col-num">{stat.redCards}</td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr className="totals-row">
                      <td>{t('football.player.careerTotal', 'Career total')}</td>
                      <td></td>
                      <td className="col-num">{totals.gamesPlayed}</td>
                      <td className="col-num">{totals.goals}</td>
                      <td className="col-num">{totals.assists}</td>
                      <td className="col-num">{totals.points}</td>
                      <td className="col-num">{totals.yellowCards}</td>
                      <td className="col-num">{totals.redCards}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            ) : (
              <p className="no-data-message">{t('football.player.noSeasonStats', 'No season statistics available.')}</p>
            )}
          </div>
        </div>
      </div>
    </PageTemplate>
  );
}

export default FootballPlayerPage;
