import { useEffect, useState, useMemo } from "react";
import { useParams, Link } from "react-router-dom";
import { FloorballPosition } from "../../types/floorball/floorballTypes";
import {
  floorballPlayerService,
  type FloorballPlayerWithMatchesDto,
  type FloorballPlayerMatchDto,
} from "../../api/floorball/floorballPlayerService";
import {
  floorballStatisticsService,
  type FloorballPlayerProfileDto,
  type FloorballPlayerSeasonStatisticsDto,
  type FloorballGoalieSeasonStatisticsDto,
} from "../../api/floorball/floorballStatistics";
import PageTemplate from "../../components/PageTemplate/PageTemplate";
import { TeamLink, MatchLink } from "../../components/SportLinks";
import { getLeaguePath } from "../../utils/sportRoutes";
import { useTranslation } from "react-i18next";
import './FloorballTeamPlayerUserPage.scss';

const getPositionText = (position: FloorballPosition | string, t: (key: string) => string): string => {
  switch (position) {
    case FloorballPosition.Goalkeeper:
    case 'Goalkeeper':
      return t('playerPage.positions.goalkeeper');
    case FloorballPosition.Defender:
    case 'Defender':
      return t('playerPage.positions.defender');
    case FloorballPosition.Forward:
    case 'Forward':
      return t('playerPage.positions.forward');
    default:
      return t('playerPage.positions.player');
  }
};

const formatDate = (dateStr: string, locale: string): string => {
  const date = new Date(dateStr);
  return date.toLocaleDateString(locale, {
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

const formatBirthDate = (birthDate: string | null, locale: string): string => {
  if (!birthDate) return '';
  const date = new Date(birthDate);
  return date.toLocaleDateString(locale);
};

interface SeasonTotals {
  gamesPlayed: number;
  goals: number;
  assists: number;
  points: number;
  penaltyMinutes: number;
  plusMinusRating: number;
  powerPlayGoals: number;
  powerPlayAssists: number;
  shortHandedGoals: number;
  shortHandedAssists: number;
}

const calculateSeasonTotals = (stats: FloorballPlayerSeasonStatisticsDto[]): SeasonTotals => {
  return stats.reduce(
    (total, s) => ({
      gamesPlayed: total.gamesPlayed + s.gamesPlayed,
      goals: total.goals + s.goals,
      assists: total.assists + s.assists,
      points: total.points + s.points,
      penaltyMinutes: total.penaltyMinutes + s.penaltyMinutes,
      plusMinusRating: total.plusMinusRating + s.plusMinusRating,
      powerPlayGoals: total.powerPlayGoals + s.powerPlayGoals,
      powerPlayAssists: total.powerPlayAssists + s.powerPlayAssists,
      shortHandedGoals: total.shortHandedGoals + s.shortHandedGoals,
      shortHandedAssists: total.shortHandedAssists + s.shortHandedAssists,
    }),
    {
      gamesPlayed: 0, goals: 0, assists: 0, points: 0, penaltyMinutes: 0,
      plusMinusRating: 0, powerPlayGoals: 0, powerPlayAssists: 0,
      shortHandedGoals: 0, shortHandedAssists: 0,
    }
  );
};

interface MatchTotals {
  goals: number;
  assists: number;
  penaltyMinutes: number;
}

const calculateMatchTotals = (matches: FloorballPlayerMatchDto[]): MatchTotals => {
  return matches.reduce(
    (total, m) => ({
      goals: total.goals + (m.playerStats?.goals ?? 0),
      assists: total.assists + (m.playerStats?.assists ?? 0),
      penaltyMinutes: total.penaltyMinutes + (m.playerStats?.penaltyMinutes ?? 0),
    }),
    { goals: 0, assists: 0, penaltyMinutes: 0 }
  );
};

interface GoalieTotals {
  gamesPlayed: number;
  wins: number;
  losses: number;
  ties: number;
  saves: number;
  shotsAgainst: number;
  goalsAgainst: number;
  shutouts: number;
  minutesPlayed: number;
}

const calculateGoalieTotals = (stats: FloorballGoalieSeasonStatisticsDto[]): GoalieTotals => {
  return stats.reduce(
    (total, s) => ({
      gamesPlayed: total.gamesPlayed + s.gamesPlayed,
      wins: total.wins + s.wins,
      losses: total.losses + s.losses,
      ties: total.ties + s.ties,
      saves: total.saves + s.saves,
      shotsAgainst: total.shotsAgainst + s.shotsAgainst,
      goalsAgainst: total.goalsAgainst + s.goalsAgainst,
      shutouts: total.shutouts + s.shutouts,
      minutesPlayed: total.minutesPlayed + s.minutesPlayed,
    }),
    {
      gamesPlayed: 0, wins: 0, losses: 0, ties: 0, saves: 0,
      shotsAgainst: 0, goalsAgainst: 0, shutouts: 0, minutesPlayed: 0,
    }
  );
};

const calculateOverallSavePercentage = (totals: GoalieTotals): number => {
  return totals.shotsAgainst > 0 ? (totals.saves / totals.shotsAgainst) * 100 : 0;
};

const MATCHES_PER_PAGE = 20;

const FloorballTeamPlayerUserPage = () => {
  const { t, i18n } = useTranslation();
  const locale = i18n.language?.startsWith('en') ? 'en-GB' : 'fi-FI';
  const { id } = useParams<{ id: string }>();
  const [profile, setProfile] = useState<FloorballPlayerProfileDto | null>(null);
  const [matchData, setMatchData] = useState<FloorballPlayerWithMatchesDto | null>(null);
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
          floorballStatisticsService.getPlayerProfile(id),
          floorballPlayerService.getPlayerMatches(id, 50),
        ]);

        setProfile(profileResult);
        setMatchData(matchesResult);
      } catch (err) {
        console.error('Error loading player data:', err);
        setError(err instanceof Error ? err.message : t('playerPage.loadError'));
      } finally {
        setLoading(false);
      }
    };

    loadPlayerData();
  }, [id, t]);

  const seasonStats = useMemo(() => profile?.seasonStatistics ?? [], [profile]);
  const goalieStats = useMemo(() => profile?.seasonStatisticsForGoalie ?? [], [profile]);
  const matches = useMemo(() => matchData?.recentMatches ?? [], [matchData]);
  const totals = useMemo(() => calculateSeasonTotals(seasonStats), [seasonStats]);
  const goalieTotals = useMemo(() => calculateGoalieTotals(goalieStats), [goalieStats]);
  const matchTotals = useMemo(() => calculateMatchTotals(matches), [matches]);
  const totalMatchPages = Math.max(1, Math.ceil(matches.length / MATCHES_PER_PAGE));
  const paginatedMatches = useMemo(
    () => matches.slice((matchPage - 1) * MATCHES_PER_PAGE, matchPage * MATCHES_PER_PAGE),
    [matches, matchPage]
  );

  if (loading) return <PageTemplate title={t('playerPage.title')}><div className="player-loading">{t('common.loading')}</div></PageTemplate>;
  if (error) return <PageTemplate title={t('playerPage.title')}><div className="player-error">{error}</div></PageTemplate>;
  if (!profile) return <PageTemplate title={t('playerPage.title')}><div className="player-error">{t('playerPage.notFound')}</div></PageTemplate>;

  const { player } = profile;
  const playerName = player.person.fullName;
  const isCreator = playerName === 'Tuomas Reijonen';
  const age = calculateAge(player.person.birthDate);
  const teamName = matchData?.teamName ?? player.team?.name ?? '';
  const position = matchData?.position ?? player.position;
  const jerseyNumber = matchData?.jerseyNumber;

  return (
    <PageTemplate title={playerName}>
      <div className="player-page">
        {/* Player Header */}
        <div className="player-container">
          <div className="player-info-layout">
            <div className="player-info-box">
              <div className={`player-avatar-large${isCreator ? ' player-avatar--creator' : ''}`}>
                {isCreator ? (
                  <img
                    className="creator-avatar-img"
                    src="https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExanpjZzBqNm1xYnp1d3Y5c2V5OWxoeTg2ZjV5dHpldHQ4anI0dDd5MSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/sPE5g5cHJ3dNm/giphy.gif"
                    alt="Creator avatar"
                  />
                ) : seasonStats[0]?.teamLogo ? (
                  <img
                    className="team-logo-img"
                    src={seasonStats[0].teamLogo}
                    alt={teamName}
                  />
                ) : null}
              </div>
              <div className="player-details">
                <div className={`player-name${isCreator ? ' player-name--creator' : ''}`}>
                  {playerName}
                  {isCreator && (
                    <span className="creator-title" title="System Creator">Macho King</span>
                  )}
                </div>
                <div className="player-details-row">
                  {teamName && (
                    <TeamLink
                      sport="floorball"
                      teamId={matchData?.teamId ?? player.team?.id}
                      teamName={teamName}
                      className="player-team"
                    />
                  )}
                  <span className="player-position">{getPositionText(position, t)}</span>
                  {jerseyNumber != null && <span className="player-jersey">#{jerseyNumber}</span>}
                </div>
              </div>
            </div>

            <div className="player-stats-box">
              {age !== null && (
                <div className="stat-item">
                  <span className="stat-label">{t('playerPage.age')}:</span>
                  <span className="stat-value">{age} ({formatBirthDate(player.person.birthDate, locale)})</span>
                </div>
              )}
              <div className="stat-item">
                <span className="stat-label">{t('playerPage.status')}:</span>
                <span className={`stat-value ${player.isActive ? 'active' : 'inactive'}`}>
                  {player.isActive ? t('playerPage.active') : t('playerPage.inactive')}
                </span>
              </div>
              <div className="stat-item">
                <span className="stat-label">{t('playerPage.position')}:</span>
                <span className="stat-value">{getPositionText(position, t)}</span>
              </div>
            </div>
          </div>
        </div>

        {/* Career Summary Boxes */}
        <div className="player-container">
          <div className="career-stats-section">
            <h3>{t('playerPage.careerStats')}</h3>
            <div className="stats-grid">
              <div className="stats-box">
                <div className="stats-value">{totals.gamesPlayed}</div>
                <div className="stats-label">{t('playerPage.games')}</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.goals}</div>
                <div className="stats-label">{t('playerPage.goals')}</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.assists}</div>
                <div className="stats-label">{t('playerPage.assists')}</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.points}</div>
                <div className="stats-label">{t('playerPage.points')}</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.penaltyMinutes}</div>
                <div className="stats-label">{t('playerPage.penaltyMinutes')}</div>
              </div>
            </div>
          </div>
        </div>

        {/* Goalie Career Summary - shown only if player has goalie stats */}
        {goalieStats.length > 0 && (
          <div className="player-container">
            <div className="career-stats-section">
              <h3>{t('playerPage.goalieCareer')}</h3>
              <div className="stats-grid">
                <div className="stats-box">
                  <div className="stats-value">{goalieTotals.gamesPlayed}</div>
                  <div className="stats-label">{t('playerPage.games')}</div>
                </div>
                <div className="stats-box">
                  <div className="stats-value">{goalieTotals.wins}</div>
                  <div className="stats-label">{t('playerPage.wins')}</div>
                </div>
                <div className="stats-box">
                  <div className="stats-value">{goalieTotals.losses}</div>
                  <div className="stats-label">{t('playerPage.losses')}</div>
                </div>
                <div className="stats-box">
                  <div className="stats-value">{calculateOverallSavePercentage(goalieTotals).toFixed(1)}%</div>
                  <div className="stats-label">{t('playerPage.savePercentage')}</div>
                </div>
                <div className="stats-box">
                  <div className="stats-value">{goalieTotals.shutouts}</div>
                  <div className="stats-label">{t('playerPage.shutouts')}</div>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Otteluhistoria (Match History) */}
        <div className="player-container">
          <div className="section-block">
            <h3>{t('playerPage.matchHistory')}</h3>
            {matches.length > 0 ? (
              <>
                <div className="stats-table-scroll">
                  <table className="stats-table">
                    <thead>
                      <tr>
                        <th className="col-date">{t('playerPage.date')}</th>
                        <th className="col-league">{t('playerPage.competition')}</th>
                        <th className="col-team">{t('playerPage.home')}</th>
                        <th className="col-score">{t('playerPage.score')}</th>
                        <th className="col-team">{t('playerPage.away')}</th>
                        <th className="col-num" title={t('playerPage.goals')}>M</th>
                        <th className="col-num" title={t('playerPage.assists')}>S</th>
                        <th className="col-num" title={t('playerPage.points')}>P</th>
                        <th className="col-num" title={t('playerPage.penaltyMinutes')}>JM</th>
                      </tr>
                    </thead>
                    <tbody>
                      {paginatedMatches.map((match) => (
                        <tr key={match.id}>
                          <td className="col-date">{formatDate(match.scheduledDateTime, locale)}</td>
                          <td className="col-league">
                            <Link to={getLeaguePath('floorball', match.competitionId)} className="team-link">{match.competitionName}</Link>
                          </td>
                          <td className="col-team">
                            <TeamLink sport="floorball" teamName={match.homeTeamName} className="team-link" />
                          </td>
                          <td className="col-score">
                            <MatchLink sport="floorball" matchId={match.id} className="team-link">
                              {match.homeScore} - {match.awayScore}
                            </MatchLink>
                          </td>
                          <td className="col-team">
                            <TeamLink sport="floorball" teamName={match.awayTeamName} className="team-link" />
                          </td>
                          <td className="col-num">{match.playerStats?.goals ?? 0}</td>
                          <td className="col-num">{match.playerStats?.assists ?? 0}</td>
                          <td className="col-num">{(match.playerStats?.goals ?? 0) + (match.playerStats?.assists ?? 0)}</td>
                          <td className="col-num">{match.playerStats?.penaltyMinutes ?? 0}</td>
                        </tr>
                      ))}
                    </tbody>
                    <tfoot>
                      <tr className="totals-row">
                        <td colSpan={5}>{t('playerPage.matchesTotal')}: {matches.length}</td>
                        <td className="col-num">{matchTotals.goals}</td>
                        <td className="col-num">{matchTotals.assists}</td>
                        <td className="col-num">{matchTotals.goals + matchTotals.assists}</td>
                        <td className="col-num">{matchTotals.penaltyMinutes}</td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
                {totalMatchPages > 1 && (
                  <div className="pagination">
                    <button
                      className="pagination-btn"
                      disabled={matchPage === 1}
                      onClick={() => setMatchPage(1)}
                      title={t('playerPage.pagination.first')}
                    >
                      &laquo;
                    </button>
                    <button
                      className="pagination-btn"
                      disabled={matchPage === 1}
                      onClick={() => setMatchPage((p) => p - 1)}
                      title={t('playerPage.pagination.previous')}
                    >
                      &lsaquo;
                    </button>
                    <span className="pagination-info">
                      {t('playerPage.pagination.pageOf', { current: matchPage, total: totalMatchPages })}
                    </span>
                    <button
                      className="pagination-btn"
                      disabled={matchPage === totalMatchPages}
                      onClick={() => setMatchPage((p) => p + 1)}
                      title={t('playerPage.pagination.next')}
                    >
                      &rsaquo;
                    </button>
                    <button
                      className="pagination-btn"
                      disabled={matchPage === totalMatchPages}
                      onClick={() => setMatchPage(totalMatchPages)}
                      title={t('playerPage.pagination.last')}
                    >
                      &raquo;
                    </button>
                  </div>
                )}
              </>
            ) : (
              <p className="no-data-message">{t('playerPage.noMatches')}</p>
            )}
          </div>
        </div>

        {/* Henkilökohtaiset tilastot (Personal Season Statistics) */}
        <div className="player-container">
          <div className="section-block">
            <h3>{t('playerPage.seasonStats')}</h3>
            {seasonStats.length > 0 ? (
              <div className="stats-table-scroll">
                <table className="stats-table">
                  <thead>
                    <tr>
                      <th className="col-season">{t('playerPage.season')}</th>
                      <th className="col-team">{t('playerPage.team')}</th>
                      <th className="col-num" title={t('playerPage.games')}>O</th>
                      <th className="col-num" title={t('playerPage.goals')}>M</th>
                      <th className="col-num" title={t('playerPage.assists')}>S</th>
                      <th className="col-num" title={t('playerPage.points')}>P</th>
                      <th className="col-num" title={t('playerPage.penaltyMinutes')}>JM</th>
                      <th className="col-num" title={t('playerPage.plusMinus')}>+/-</th>
                      <th className="col-num" title={t('playerPage.powerPlayGoals')}>YVM</th>
                      <th className="col-num" title={t('playerPage.powerPlayAssists')}>YVS</th>
                      <th className="col-num" title={t('playerPage.shortHandedGoals')}>AVM</th>
                      <th className="col-num" title={t('playerPage.shortHandedAssists')}>AVS</th>
                    </tr>
                  </thead>
                  <tbody>
                    {seasonStats.map((stat) => (
                      <tr key={stat.id}>
                        <td className="col-season">{stat.seasonName}</td>
                        <td className="col-team">
                          <div className="team-cell">
                            {stat.teamLogo && (
                              <img src={stat.teamLogo} alt={stat.teamName} className="team-logo-small" />
                            )}
                            <TeamLink sport="floorball" teamId={stat.teamId} teamName={stat.teamName} className="team-link" />
                          </div>
                        </td>
                        <td className="col-num">{stat.gamesPlayed}</td>
                        <td className="col-num">{stat.goals}</td>
                        <td className="col-num">{stat.assists}</td>
                        <td className="col-num">{stat.points}</td>
                        <td className="col-num">{stat.penaltyMinutes}</td>
                        <td className="col-num">{stat.plusMinusRating}</td>
                        <td className="col-num">{stat.powerPlayGoals}</td>
                        <td className="col-num">{stat.powerPlayAssists}</td>
                        <td className="col-num">{stat.shortHandedGoals}</td>
                        <td className="col-num">{stat.shortHandedAssists}</td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr className="totals-row">
                      <td>{t('playerPage.careerTotal')}</td>
                      <td></td>
                      <td className="col-num">{totals.gamesPlayed}</td>
                      <td className="col-num">{totals.goals}</td>
                      <td className="col-num">{totals.assists}</td>
                      <td className="col-num">{totals.points}</td>
                      <td className="col-num">{totals.penaltyMinutes}</td>
                      <td className="col-num">{totals.plusMinusRating}</td>
                      <td className="col-num">{totals.powerPlayGoals}</td>
                      <td className="col-num">{totals.powerPlayAssists}</td>
                      <td className="col-num">{totals.shortHandedGoals}</td>
                      <td className="col-num">{totals.shortHandedAssists}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            ) : (
              <p className="no-data-message">{t('playerPage.noSeasonStats')}</p>
            )}
          </div>
        </div>

        {/* Maalivahtitilastot kausittain (Goalie Season Statistics) */}
        {goalieStats.length > 0 && (
          <div className="player-container">
            <div className="section-block">
              <h3>{t('playerPage.goalieSeasonStats')}</h3>
              <div className="stats-table-scroll">
                <table className="stats-table">
                  <thead>
                    <tr>
                      <th className="col-season">{t('playerPage.season')}</th>
                      <th className="col-team">{t('playerPage.team')}</th>
                      <th className="col-num" title={t('playerPage.games')}>O</th>
                      <th className="col-num" title={t('playerPage.wins')}>V</th>
                      <th className="col-num" title={t('playerPage.losses')}>H</th>
                      <th className="col-num" title={t('playerPage.ties')}>T</th>
                      <th className="col-num" title={t('playerPage.saves')}>TO</th>
                      <th className="col-num" title={t('playerPage.shotsAgainst')}>LA</th>
                      <th className="col-num" title={t('playerPage.savePercentage')}>TO%</th>
                      <th className="col-num" title={t('playerPage.goalsAgainst')}>PM</th>
                      <th className="col-num" title={t('playerPage.shutouts')}>NP</th>
                      <th className="col-num" title={t('playerPage.minutes')}>MIN</th>
                    </tr>
                  </thead>
                  <tbody>
                    {goalieStats.map((stat) => (
                      <tr key={stat.id}>
                        <td className="col-season">{stat.seasonName}</td>
                        <td className="col-team">
                          <div className="team-cell">
                            <TeamLink sport="floorball" teamId={stat.teamId} teamName={stat.teamName} className="team-link" />
                          </div>
                        </td>
                        <td className="col-num">{stat.gamesPlayed}</td>
                        <td className="col-num">{stat.wins}</td>
                        <td className="col-num">{stat.losses}</td>
                        <td className="col-num">{stat.ties}</td>
                        <td className="col-num">{stat.saves}</td>
                        <td className="col-num">{stat.shotsAgainst}</td>
                        <td className="col-num">{stat.savePercentage.toFixed(1)}%</td>
                        <td className="col-num">{stat.goalsAgainst}</td>
                        <td className="col-num">{stat.shutouts}</td>
                        <td className="col-num">{stat.minutesPlayed}</td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr className="totals-row">
                      <td>{t('playerPage.careerTotal')}</td>
                      <td></td>
                      <td className="col-num">{goalieTotals.gamesPlayed}</td>
                      <td className="col-num">{goalieTotals.wins}</td>
                      <td className="col-num">{goalieTotals.losses}</td>
                      <td className="col-num">{goalieTotals.ties}</td>
                      <td className="col-num">{goalieTotals.saves}</td>
                      <td className="col-num">{goalieTotals.shotsAgainst}</td>
                      <td className="col-num">{calculateOverallSavePercentage(goalieTotals).toFixed(1)}%</td>
                      <td className="col-num">{goalieTotals.goalsAgainst}</td>
                      <td className="col-num">{goalieTotals.shutouts}</td>
                      <td className="col-num">{goalieTotals.minutesPlayed}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>
          </div>
        )}

        {/* Pelaajaura (Career Timeline) */}
        <div className="player-container">
          <div className="section-block">
            <h3>{t('playerPage.career')}</h3>
            {seasonStats.length > 0 ? (
              <div className="stats-table-scroll">
                <table className="stats-table career-timeline-table">
                  <thead>
                    <tr>
                      <th className="col-season">{t('playerPage.season')}</th>
                      <th className="col-team">{t('playerPage.team')}</th>
                      <th className="col-position">{t('playerPage.position')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {seasonStats.map((stat) => (
                      <tr key={stat.id}>
                        <td className="col-season">
                          <Link to={getLeaguePath('floorball', stat.competitionId)} className="team-link">{stat.seasonName}</Link>
                        </td>
                        <td className="col-team">
                          <div className="team-cell">
                            {stat.teamLogo && (
                              <img src={stat.teamLogo} alt={stat.teamName} className="team-logo-small" />
                            )}
                            <TeamLink sport="floorball" teamId={stat.teamId} teamName={stat.teamName} className="team-link" />
                          </div>
                        </td>
                        <td className="col-position">{getPositionText(position, t)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="no-data-message">{t('playerPage.noCareer')}</p>
            )}
          </div>
        </div>
      </div>
    </PageTemplate>
  );
};

export default FloorballTeamPlayerUserPage;
