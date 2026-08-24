import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { hockeyPlayerService } from '../../api/hockey/hockeyPlayerService';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import { hockeyMatchService } from '../../api/hockey/hockeyMatchService';
import { hockeyStatisticsService } from '../../api/hockey/hockeyStatisticsService';
import { hockeySeasonService } from '../../api/hockey/hockeySeasonService';
import { hockeyTournamentService } from '../../api/hockey/hockeyTournamentService';
import { personApi } from '../../api/admin/personApi';
import type {
  HockeyGoalieCompetitionStatisticsDto,
  HockeyMatchDto,
  HockeyPlayerCompetitionStatisticsDto,
  HockeyPlayerDto,
  HockeyTeamDto,
} from '../../types/hockey/hockeyTypes';
import { isHockeyMatchFinished } from '../../types/hockey/hockeyTypes';
import { getTeamSlug } from '../../utils/slugUtils';
import { useAudience } from '../../context/AudienceContext';
import {
  countHockeyFaceoffsForActivePlayers,
  formatHockeyDate,
  formatHockeyFaceoffPercentage,
  mergeHockeyFaceoffTally,
  type HockeyFaceoffTally,
} from '../../utils/hockeyLookups';
import StatAbbr from '../../components/StatAbbr/StatAbbr';
import '../FloorballTeamPlayerUserPage/FloorballTeamPlayerUserPage.scss';

const MATCHES_PER_PAGE = 20;

interface SeasonRow {
  competitionId: string;
  competitionName: string;
  teamId: string;
  teamName: string;
  stats: HockeyPlayerCompetitionStatisticsDto;
}

interface GoalieSeasonRow {
  competitionId: string;
  competitionName: string;
  teamName: string;
  stats: HockeyGoalieCompetitionStatisticsDto;
}

interface PlayerMatchRow {
  match: HockeyMatchDto;
  homeName: string;
  awayName: string;
  competitionName: string;
  teamId: string;
  goals: number;
  assists: number;
  points: number;
  penaltyMinutes: number;
  faceoffWins: number;
  faceoffAttempts: number;
}

function calculateAge(birthDate: string | null): number | null {
  if (!birthDate) {
    return null;
  }
  const birth = new Date(birthDate);
  const today = new Date();
  let age = today.getFullYear() - birth.getFullYear();
  const monthDiff = today.getMonth() - birth.getMonth();
  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
    age -= 1;
  }
  return age;
}

async function loadCompetitionNameMap(): Promise<Map<string, string>> {
  const [seasons, tournaments] = await Promise.all([
    hockeySeasonService.getAll().catch(() => []),
    hockeyTournamentService.getAll().catch(() => []),
  ]);
  const names = new Map<string, string>();
  for (const season of seasons) {
    names.set(season.id, season.name);
  }
  for (const tournament of tournaments) {
    names.set(tournament.id, tournament.name);
  }
  return names;
}

async function resolveCompetitionName(
  competitionId: string,
  names: Map<string, string>,
): Promise<string> {
  const cached = names.get(competitionId);
  if (cached) {
    return cached;
  }
  const season = await hockeySeasonService.getById(competitionId).catch(() => null);
  if (season) {
    names.set(competitionId, season.name);
    return season.name;
  }
  const tournament = await hockeyTournamentService.getById(competitionId).catch(() => null);
  const name = tournament?.name ?? competitionId;
  names.set(competitionId, name);
  return name;
}

function pickCareerValue(...values: number[]): number {
  return values.reduce((highest, value) => Math.max(highest, value), 0);
}

function countFaceoffs(
  match: HockeyMatchDto,
  teamPlayerIds: Set<string>,
  recordedWins: number | undefined,
  recordedAttempts: number | undefined,
): HockeyFaceoffTally {
  const activeIds = new Set(
    match.matchTeams.flatMap((side) =>
      side.activePlayers
        .filter((entry) => teamPlayerIds.has(entry.teamPlayerId))
        .map((entry) => entry.id),
    ),
  );
  return mergeHockeyFaceoffTally(
    { wins: recordedWins ?? 0, attempts: recordedAttempts ?? 0 },
    countHockeyFaceoffsForActivePlayers(match, activeIds),
  );
}

function competitionPath(competitionId: string): string {
  return `/hockey/league/${competitionId}`;
}

async function mapInBatches<T, R>(
  items: T[],
  size: number,
  mapper: (item: T) => Promise<R>,
): Promise<R[]> {
  const results: R[] = [];
  for (let index = 0; index < items.length; index += size) {
    const chunk = items.slice(index, index + size);
    results.push(...await Promise.all(chunk.map(mapper)));
  }
  return results;
}

function HockeyPlayerPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const { id } = useParams<{ id: string }>();
  const [player, setPlayer] = useState<HockeyPlayerDto | null>(null);
  const [name, setName] = useState('');
  const [birthDate, setBirthDate] = useState<string | null>(null);
  const [teams, setTeams] = useState<HockeyTeamDto[]>([]);
  const [seasonRows, setSeasonRows] = useState<SeasonRow[]>([]);
  const [goalieRows, setGoalieRows] = useState<GoalieSeasonRow[]>([]);
  const [matchRows, setMatchRows] = useState<PlayerMatchRow[]>([]);
  const [matchPage, setMatchPage] = useState(1);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) {
      return;
    }
    const load = async (): Promise<void> => {
      setLoading(true);
      const loaded = await hockeyPlayerService.getById(id);
      setPlayer(loaded);
      try {
        const person = await personApi.getById(loaded.personId);
        setName(person.fullName);
        setBirthDate(person.birthDate);
      } catch {
        setName(t('hockey.players.title', 'Player'));
      }

      const [allTeams, competitionNames] = await Promise.all([
        hockeyTeamService.getAll(audience.teamCategory),
        loadCompetitionNameMap(),
      ]);
      const playerTeams = allTeams.filter((team) => team.roster.some((row) => row.playerId === loaded.id));
      setTeams(playerTeams);
      const teamNames = new Map(allTeams.map((team) => [team.id, team.name]));

      const rosterCompetitionIds = playerTeams.flatMap((team) => team.roster
        .filter((row) => row.playerId === loaded.id && row.competitionId)
        .map((row) => row.competitionId as string));

      const matchesById = new Map<string, HockeyMatchDto>();
      const teamMatchLists = await Promise.all(
        playerTeams.map((team) => hockeyMatchService.getByTeam(team.id).catch(() => [] as HockeyMatchDto[])),
      );
      for (const list of teamMatchLists) {
        for (const match of list) {
          matchesById.set(match.id, match);
        }
      }

      const teamPlayerIds = new Set(
        playerTeams.flatMap((team) => team.roster.filter((row) => row.playerId === loaded.id).map((row) => row.id)),
      );
      const played = [...matchesById.values()]
        .filter((match) => match.matchTeams.some((side) =>
          side.activePlayers.some((entry) => teamPlayerIds.has(entry.teamPlayerId))))
        .sort((a, b) => new Date(b.scheduledStartTime).getTime() - new Date(a.scheduledStartTime).getTime())
        .slice(0, 50);

      const matchCompetitionIds = played
        .map((match) => match.competitionId)
        .filter((competitionId): competitionId is string => Boolean(competitionId));
      const competitionIds = [...new Set([...rosterCompetitionIds, ...matchCompetitionIds])];

      const seasons = await Promise.all(competitionIds.map(async (competitionId) => {
        const [playerStats, goalieStats, competitionName] = await Promise.all([
          hockeyStatisticsService.getPlayers(competitionId, loaded.id).catch(() => []),
          hockeyStatisticsService.getGoalies(competitionId, loaded.id).catch(() => []),
          resolveCompetitionName(competitionId, competitionNames),
        ]);
        return { competitionId, competitionName, playerStats, goalieStats };
      }));

      const nextSeasons: SeasonRow[] = [];
      const nextGoalies: GoalieSeasonRow[] = [];
      for (const season of seasons) {
        for (const stats of season.playerStats) {
          nextSeasons.push({
            competitionId: season.competitionId,
            competitionName: season.competitionName,
            teamId: stats.teamId,
            teamName: teamNames.get(stats.teamId) ?? stats.teamId.slice(0, 8),
            stats,
          });
        }
        for (const stats of season.goalieStats) {
          nextGoalies.push({
            competitionId: season.competitionId,
            competitionName: season.competitionName,
            teamName: teamNames.get(stats.teamId) ?? stats.teamId.slice(0, 8),
            stats,
          });
        }
      }

      const history = await mapInBatches(played, 8, async (match) => {
        const box = isHockeyMatchFinished(match.status) || match.status !== 'Scheduled'
          ? await hockeyStatisticsService.getMatchStats(match.id).catch(() => null)
          : null;
        const row = box?.players.find((item) => item.playerId === loaded.id);
        const playerSide = match.matchTeams.find((side) =>
          side.activePlayers.some((entry) => teamPlayerIds.has(entry.teamPlayerId)));
        const faceoffs = countFaceoffs(
          match,
          teamPlayerIds,
          row?.faceoffWins,
          row?.faceoffAttempts,
        );
        return {
          match,
          homeName: match.homeTeamId ? teamNames.get(match.homeTeamId) ?? 'TBD' : 'TBD',
          awayName: match.awayTeamId ? teamNames.get(match.awayTeamId) ?? 'TBD' : 'TBD',
          competitionName: match.competitionId
            ? await resolveCompetitionName(match.competitionId, competitionNames)
            : '',
          teamId: playerSide?.teamId ?? '',
          goals: row?.goals ?? 0,
          assists: row?.assists ?? 0,
          points: row?.points ?? 0,
          penaltyMinutes: row?.penaltyMinutes ?? 0,
          faceoffWins: faceoffs.wins,
          faceoffAttempts: faceoffs.attempts,
        };
      });

      if (nextSeasons.length > 0) {
        const faceoffsByKey = new Map<string, { wins: number; attempts: number }>();
        for (const row of history) {
          const competitionId = row.match.competitionId;
          if (!competitionId) {
            continue;
          }
          const key = `${competitionId}:${row.teamId}`;
          const current = faceoffsByKey.get(key) ?? { wins: 0, attempts: 0 };
          faceoffsByKey.set(key, {
            wins: current.wins + row.faceoffWins,
            attempts: current.attempts + row.faceoffAttempts,
          });
        }
        for (const season of nextSeasons) {
          if ((season.stats.faceoffAttempts ?? 0) === 0) {
            const fromMatches = faceoffsByKey.get(`${season.competitionId}:${season.teamId}`);
            season.stats = {
              ...season.stats,
              faceoffWins: fromMatches?.wins ?? 0,
              faceoffAttempts: fromMatches?.attempts ?? 0,
            };
          }
        }
      }

      if (nextSeasons.length === 0) {
        const derived = new Map<string, SeasonRow>();
        for (const row of history) {
          const competitionId = row.match.competitionId;
          if (!competitionId) {
            continue;
          }
          const key = `${competitionId}:${row.teamId}`;
          const existing = derived.get(key);
          if (existing) {
            existing.stats.gamesPlayed += 1;
            existing.stats.goals += row.goals;
            existing.stats.assists += row.assists;
            existing.stats.points += row.points;
            existing.stats.penaltyMinutes += row.penaltyMinutes;
            existing.stats.faceoffWins += row.faceoffWins;
            existing.stats.faceoffAttempts += row.faceoffAttempts;
          } else {
            derived.set(key, {
              competitionId,
              competitionName: row.competitionName,
              teamId: row.teamId,
              teamName: teamNames.get(row.teamId) ?? row.teamId.slice(0, 8),
              stats: {
                id: `derived-${key}`,
                playerId: loaded.id,
                teamId: row.teamId,
                teamPlayerId: '',
                competitionId,
                gamesPlayed: 1,
                goals: row.goals,
                assists: row.assists,
                points: row.points,
                penaltyMinutes: row.penaltyMinutes,
                plusMinusRating: 0,
                faceoffWins: row.faceoffWins,
                faceoffAttempts: row.faceoffAttempts,
              },
            });
          }
        }
        nextSeasons.push(...derived.values());
      }

      setSeasonRows(nextSeasons);
      setGoalieRows(nextGoalies);
      setMatchRows(history);
    };
    void load()
      .catch((err) => setError(err instanceof Error ? err.message : 'Failed to load player'))
      .finally(() => setLoading(false));
  }, [id, t, audience.teamCategory]);

  const namedTeams = teams.map((team) => ({ id: team.id, name: team.name }));
  const age = calculateAge(birthDate);
  const totals = useMemo(() => seasonRows.reduce(
    (sum, row) => ({
      gamesPlayed: sum.gamesPlayed + row.stats.gamesPlayed,
      goals: sum.goals + row.stats.goals,
      assists: sum.assists + row.stats.assists,
      points: sum.points + row.stats.points,
      penaltyMinutes: sum.penaltyMinutes + row.stats.penaltyMinutes,
      faceoffWins: sum.faceoffWins + (row.stats.faceoffWins ?? 0),
      faceoffAttempts: sum.faceoffAttempts + (row.stats.faceoffAttempts ?? 0),
    }),
    { gamesPlayed: 0, goals: 0, assists: 0, points: 0, penaltyMinutes: 0, faceoffWins: 0, faceoffAttempts: 0 },
  ), [seasonRows]);
  const matchTotals = useMemo(() => matchRows.reduce(
    (sum, row) => ({
      goals: sum.goals + row.goals,
      assists: sum.assists + row.assists,
      points: sum.points + row.points,
      penaltyMinutes: sum.penaltyMinutes + row.penaltyMinutes,
      faceoffWins: sum.faceoffWins + row.faceoffWins,
      faceoffAttempts: sum.faceoffAttempts + row.faceoffAttempts,
    }),
    { goals: 0, assists: 0, points: 0, penaltyMinutes: 0, faceoffWins: 0, faceoffAttempts: 0 },
  ), [matchRows]);
  const totalMatchPages = Math.max(1, Math.ceil(matchRows.length / MATCHES_PER_PAGE));
  const paginatedMatches = matchRows.slice((matchPage - 1) * MATCHES_PER_PAGE, matchPage * MATCHES_PER_PAGE);
  const careerGames = pickCareerValue(player?.careerGamesPlayed ?? 0, totals.gamesPlayed, matchRows.length);
  const careerGoals = pickCareerValue(player?.careerGoals ?? 0, totals.goals, matchTotals.goals);
  const careerAssists = pickCareerValue(player?.careerAssists ?? 0, totals.assists, matchTotals.assists);
  const careerPim = pickCareerValue(player?.careerPenaltyMinutes ?? 0, totals.penaltyMinutes, matchTotals.penaltyMinutes);
  const careerFaceoffs = totals.faceoffAttempts > 0
    ? { wins: totals.faceoffWins, attempts: totals.faceoffAttempts }
    : { wins: matchTotals.faceoffWins, attempts: matchTotals.faceoffAttempts };

  if (loading) {
    return (
      <PageTemplate title={t('hockey.players.title', 'Player')}>
        <div className="player-loading">{t('common.loading', 'Loading...')}</div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={name || t('hockey.players.title', 'Player')}>
      <div className="player-page">
        {error && <div className="player-error">{error}</div>}
        {player && (
          <>
            <div className="player-container">
              <div className="player-info-layout">
                <div className="player-info-box">
                  <div className="player-avatar-large" />
                  <div className="player-details">
                    <div className="player-name">{name}</div>
                    <div className="player-details-row">
                      {teams[0] && <span className="player-team">{teams[0].name}</span>}
                      <span className="player-position">
                        {t(`hockey.positions.${player.primaryPosition}`, player.primaryPosition)}
                      </span>
                    </div>
                  </div>
                </div>
                <div className="player-stats-box">
                  {age !== null && (
                    <div className="stat-item">
                      <span className="stat-label">{t('hockey.players.age', 'Age')}</span>
                      <span className="stat-value">{age}</span>
                    </div>
                  )}
                  <div className="stat-item">
                    <span className="stat-label">{t('hockey.players.shoots', 'Shoots')}</span>
                    <span className="stat-value">{t(`hockey.shoots.${player.shoots}`, player.shoots)}</span>
                  </div>
                  <div className="stat-item">
                    <span className="stat-label">{t('hockey.players.status', 'Status')}</span>
                    <span className={`stat-value ${player.isActive ? 'active' : 'inactive'}`}>
                      {player.isActive
                        ? t('hockey.players.active', 'Active')
                        : t('hockey.players.inactive', 'Inactive')}
                    </span>
                  </div>
                </div>
              </div>
            </div>
            <div className="player-container">
              <div className="career-stats-section">
                <h3>{t('hockey.players.careerStats', 'Career statistics')}</h3>
                <div className="stats-grid">
                  <div className="stats-box">
                    <div className="stats-value">{careerGames}</div>
                    <div className="stats-label">
                      <StatAbbr abbr={t('hockeyPage.colGp', 'GP')} title={t('hockeyPage.colGpTitle', 'Games played')} />
                    </div>
                  </div>
                  <div className="stats-box">
                    <div className="stats-value">{careerGoals}</div>
                    <div className="stats-label">
                      <StatAbbr abbr={t('hockeyPage.colG', 'G')} title={t('hockeyPage.colGTitle', 'Goals')} />
                    </div>
                  </div>
                  <div className="stats-box">
                    <div className="stats-value">{careerAssists}</div>
                    <div className="stats-label">
                      <StatAbbr abbr={t('hockeyPage.colA', 'A')} title={t('hockeyPage.colATitle', 'Assists')} />
                    </div>
                  </div>
                  <div className="stats-box">
                    <div className="stats-value">{careerGoals + careerAssists}</div>
                    <div className="stats-label">
                      <StatAbbr abbr={t('hockeyPage.colP', 'P')} title={t('hockeyPage.colPTitle', 'Points')} />
                    </div>
                  </div>
                  <div className="stats-box">
                    <div className="stats-value">{careerPim}</div>
                    <div className="stats-label">
                      <StatAbbr abbr={t('hockeyPage.colPim', 'PIM')} title={t('hockeyPage.colPimTitle', 'Penalty minutes')} />
                    </div>
                  </div>
                  <div className="stats-box">
                    <div className="stats-value">{formatHockeyFaceoffPercentage(careerFaceoffs.wins, careerFaceoffs.attempts)}</div>
                    <div className="stats-label">
                      <StatAbbr abbr={t('hockeyPage.colFo', 'FO%')} title={t('hockeyPage.colFoTitle', 'Faceoff win percentage')} />
                    </div>
                  </div>
                </div>
              </div>
            </div>
            {goalieRows.length > 0 && (
              <div className="player-container">
                <div className="career-stats-section">
                  <h3>{t('hockey.players.goalieCareer', 'Goalie statistics')}</h3>
                  <div className="stats-table-scroll">
                    <table className="stats-table">
                      <thead>
                        <tr>
                          <th className="col-season">{t('hockey.players.season', 'Season')}</th>
                          <th className="col-team">{t('hockey.players.team', 'Team')}</th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colGp', 'GP')} title={t('hockeyPage.colGpTitle', 'Games played')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colW', 'W')} title={t('hockeyPage.colWTitle', 'Wins')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colL', 'L')} title={t('hockeyPage.colLTitle', 'Losses')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colSvPct', 'SV%')} title={t('hockeyPage.colSvPctTitle', 'Save percentage')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colGaa', 'GAA')} title={t('hockeyPage.colGaaTitle', 'Goals against average')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colSo', 'SO')} title={t('hockeyPage.colSoTitle', 'Shutouts')} /></th>
                        </tr>
                      </thead>
                      <tbody>
                        {goalieRows.map((row) => (
                          <tr key={row.stats.id}>
                            <td className="col-season">{row.competitionName}</td>
                            <td className="col-team">{row.teamName}</td>
                            <td className="col-num">{row.stats.gamesPlayed}</td>
                            <td className="col-num">{row.stats.wins}</td>
                            <td className="col-num">{row.stats.losses}</td>
                            <td className="col-num">{row.stats.savePercentage.toFixed(1)}</td>
                            <td className="col-num">{row.stats.goalsAgainstAverage.toFixed(2)}</td>
                            <td className="col-num">{row.stats.shutouts}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            )}
            <div className="player-container">
              <div className="section-block">
                <h3>{t('hockey.players.matchHistory', 'Match history')}</h3>
                {paginatedMatches.length === 0 ? (
                  <p className="no-data-message">{t('hockey.players.noMatchHistory', 'No match history yet')}</p>
                ) : (
                  <div className="stats-table-scroll">
                    <table className="stats-table">
                      <thead>
                        <tr>
                          <th className="col-date">{t('hockey.players.date', 'Date')}</th>
                          <th className="col-league">{t('hockey.players.season', 'League')}</th>
                          <th className="col-team">{t('hockeyPage.home', 'Home')}</th>
                          <th className="col-score">{t('hockeyPage.score', 'Score')}</th>
                          <th className="col-team">{t('hockeyPage.away', 'Away')}</th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colG', 'G')} title={t('hockeyPage.colGTitle', 'Goals')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colA', 'A')} title={t('hockeyPage.colATitle', 'Assists')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colP', 'P')} title={t('hockeyPage.colPTitle', 'Points')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colPim', 'PIM')} title={t('hockeyPage.colPimTitle', 'Penalty minutes')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colFo', 'FO%')} title={t('hockeyPage.colFoTitle', 'Faceoff win percentage')} /></th>
                        </tr>
                      </thead>
                      <tbody>
                        {paginatedMatches.map((row) => (
                          <tr key={row.match.id}>
                            <td className="col-date">{formatHockeyDate(row.match.scheduledStartTime)}</td>
                            <td className="col-league">
                              {row.match.competitionId ? (
                                <Link to={competitionPath(row.match.competitionId)} className="team-link">
                                  {row.competitionName}
                                </Link>
                              ) : '—'}
                            </td>
                            <td className="col-team">{row.homeName}</td>
                            <td className="col-score">
                              <Link to={`/hockey/match/${row.match.id}`}>
                                {row.match.homeScore} - {row.match.awayScore}
                              </Link>
                            </td>
                            <td className="col-team">{row.awayName}</td>
                            <td className="col-num">{row.goals}</td>
                            <td className="col-num">{row.assists}</td>
                            <td className="col-num">{row.points}</td>
                            <td className="col-num">{row.penaltyMinutes}</td>
                            <td className="col-num">{formatHockeyFaceoffPercentage(row.faceoffWins, row.faceoffAttempts)}</td>
                          </tr>
                        ))}
                      </tbody>
                      <tfoot>
                        <tr className="totals-row">
                          <td colSpan={5}>{t('hockey.players.matchesTotal', 'Matches')}: {matchRows.length}</td>
                          <td className="col-num">{matchTotals.goals}</td>
                          <td className="col-num">{matchTotals.assists}</td>
                          <td className="col-num">{matchTotals.points}</td>
                          <td className="col-num">{matchTotals.penaltyMinutes}</td>
                          <td className="col-num">{formatHockeyFaceoffPercentage(matchTotals.faceoffWins, matchTotals.faceoffAttempts)}</td>
                        </tr>
                      </tfoot>
                    </table>
                  </div>
                )}
                {totalMatchPages > 1 && (
                  <div className="pagination">
                    <button type="button" className="pagination-btn" disabled={matchPage === 1} onClick={() => setMatchPage(1)}>&laquo;</button>
                    <button type="button" className="pagination-btn" disabled={matchPage === 1} onClick={() => setMatchPage((page) => page - 1)}>&lsaquo;</button>
                    <span>{matchPage} / {totalMatchPages}</span>
                    <button type="button" className="pagination-btn" disabled={matchPage === totalMatchPages} onClick={() => setMatchPage((page) => page + 1)}>&rsaquo;</button>
                    <button type="button" className="pagination-btn" disabled={matchPage === totalMatchPages} onClick={() => setMatchPage(totalMatchPages)}>&raquo;</button>
                  </div>
                )}
              </div>
            </div>
            <div className="player-container">
              <div className="section-block">
                <h3>{t('hockey.players.seasonStats', 'Season statistics')}</h3>
                {seasonRows.length === 0 ? (
                  <p className="no-data-message">{t('hockeyPage.noStats', 'No statistics yet')}</p>
                ) : (
                  <div className="stats-table-scroll">
                    <table className="stats-table">
                      <thead>
                        <tr>
                          <th className="col-season">{t('hockey.players.season', 'Season')}</th>
                          <th className="col-team">{t('hockey.players.team', 'Team')}</th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colGp', 'GP')} title={t('hockeyPage.colGpTitle', 'Games played')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colG', 'G')} title={t('hockeyPage.colGTitle', 'Goals')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colA', 'A')} title={t('hockeyPage.colATitle', 'Assists')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colP', 'P')} title={t('hockeyPage.colPTitle', 'Points')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colPim', 'PIM')} title={t('hockeyPage.colPimTitle', 'Penalty minutes')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colFo', 'FO%')} title={t('hockeyPage.colFoTitle', 'Faceoff win percentage')} /></th>
                          <th className="col-num"><StatAbbr abbr={t('hockeyPage.colPlusMinus', '+/-')} title={t('hockeyPage.colPlusMinusTitle', 'Plus-minus')} /></th>
                        </tr>
                      </thead>
                      <tbody>
                        {seasonRows.map((row) => (
                          <tr key={row.stats.id}>
                            <td className="col-season">
                              <Link to={competitionPath(row.competitionId)} className="team-link">
                                {row.competitionName}
                              </Link>
                            </td>
                            <td className="col-team">
                              <Link
                                to={`/hockey/team/${getTeamSlug({ id: row.teamId, name: row.teamName }, namedTeams.length > 0 ? namedTeams : [{ id: row.teamId, name: row.teamName }])}`}
                                className="team-link"
                              >
                                {row.teamName}
                              </Link>
                            </td>
                            <td className="col-num">{row.stats.gamesPlayed}</td>
                            <td className="col-num">{row.stats.goals}</td>
                            <td className="col-num">{row.stats.assists}</td>
                            <td className="col-num">{row.stats.points}</td>
                            <td className="col-num">{row.stats.penaltyMinutes}</td>
                            <td className="col-num">{formatHockeyFaceoffPercentage(row.stats.faceoffWins ?? 0, row.stats.faceoffAttempts ?? 0)}</td>
                            <td className="col-num">{row.stats.plusMinusRating}</td>
                          </tr>
                        ))}
                      </tbody>
                      <tfoot>
                        <tr className="totals-row">
                          <td>{t('hockey.players.career', 'Career')}</td>
                          <td />
                          <td className="col-num">{totals.gamesPlayed}</td>
                          <td className="col-num">{totals.goals}</td>
                          <td className="col-num">{totals.assists}</td>
                          <td className="col-num">{totals.points}</td>
                          <td className="col-num">{totals.penaltyMinutes}</td>
                          <td className="col-num">{formatHockeyFaceoffPercentage(totals.faceoffWins, totals.faceoffAttempts)}</td>
                          <td />
                        </tr>
                      </tfoot>
                    </table>
                  </div>
                )}
              </div>
            </div>
            <div className="player-container">
              <h2>{t('hockey.players.team', 'Teams')}</h2>
              <ul>
                {teams.map((team) => (
                  <li key={team.id}>
                    <Link to={`/hockey/team/${getTeamSlug({ id: team.id, name: team.name }, namedTeams)}`}>
                      {team.name}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          </>
        )}
      </div>
    </PageTemplate>
  );
}

export default HockeyPlayerPage;
