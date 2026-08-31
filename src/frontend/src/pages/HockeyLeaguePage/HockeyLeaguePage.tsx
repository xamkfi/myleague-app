import { useCallback, useEffect, useMemo, useState } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import HockeyMatchRow from '../../components/HockeyMatchRow/HockeyMatchRow';
import HockeyStandingsTable from './HockeyStandingsTable';
import HockeyPlayerStatsTables from './HockeyPlayerStatsTables';
import SharedFixturesSection from '../../components/FixturesSection/FixturesSection';
import type { FixtureMatch } from '../../components/FixturesSection/FixturesSection';
import SeasonInfoCards from '../../components/SeasonInfoCards/SeasonInfoCards';
import { hockeySeasonService } from '../../api/hockey/hockeySeasonService';
import { hockeyMatchService } from '../../api/hockey/hockeyMatchService';
import { hockeyStatisticsService } from '../../api/hockey/hockeyStatisticsService';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import type {
  HockeyGoalieCompetitionStatisticsDto,
  HockeyMatchDto,
  HockeyPlayerCompetitionStatisticsDto,
  HockeySeasonDto,
  HockeyTeamCompetitionStatisticsDto,
  HockeyTeamDto,
} from '../../types/hockey/hockeyTypes';
import {
  isHockeyMatchFinished,
  isHockeyMatchLive,
  shouldRefreshHockeyMatches,
} from '../../types/hockey/hockeyTypes';
import {
  loadHockeyRosterNameMaps,
  mergeHockeyPlayerFaceoffWins,
  uniqueHockeyStandingsByTeamId,
} from '../../utils/hockeyLookups';
import { useAudience } from '../../context/AudienceContext';
import type { SeasonContentBlockDto } from '../../types/common/seasonContent';
import { useIntervalWhen } from '../../hooks/useIntervalWhen';
import '../LeaguePage/LeaguePage.scss';
import '../LeaguePage/components/SummarySection.scss';
import '../LeaguePage/components/FixturesSection.scss';
import '../../components/LeagueStanding/LeagueStanding.scss';
import '../../components/MatchesList/MatchesList.scss';

type HockeyLeagueTab = 'summary' | 'results' | 'fixtures' | 'statistics' | 'players';

const VALID_TABS: HockeyLeagueTab[] = ['summary', 'results', 'fixtures', 'statistics', 'players'];
const STANDINGS_PREVIEW = 8;

function toFixtureStatus(status: string): string {
  if (isHockeyMatchLive(status)) {
    return 'InProgress';
  }
  if (isHockeyMatchFinished(status)) {
    return 'Completed';
  }
  return 'Scheduled';
}

function periodScoreMap(match: HockeyMatchDto): Record<number, { homeScore: number; awayScore: number }> {
  const scores: Record<number, { homeScore: number; awayScore: number }> = {};
  for (const period of match.periodScores) {
    scores[period.periodNumber] = { homeScore: period.homeGoals, awayScore: period.awayGoals };
  }
  return scores;
}

function toFixtureMatch(
  match: HockeyMatchDto,
  teamNames: Map<string, string>,
  teamLogos: Map<string, string | null>,
): FixtureMatch {
  return {
    id: match.id,
    scheduledDateTime: match.scheduledStartTime,
    status: toFixtureStatus(match.status),
    homeTeamName: match.homeTeamId ? teamNames.get(match.homeTeamId) ?? null : null,
    awayTeamName: match.awayTeamId ? teamNames.get(match.awayTeamId) ?? null : null,
    homeTeamLogo: match.homeTeamId ? teamLogos.get(match.homeTeamId) ?? null : null,
    awayTeamLogo: match.awayTeamId ? teamLogos.get(match.awayTeamId) ?? null : null,
    homeScore: match.homeScore,
    awayScore: match.awayScore,
    periodScores: periodScoreMap(match),
  };
}

function HockeyLeaguePage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const { id } = useParams<{ id: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  const tabParam = searchParams.get('tab');
  const tab: HockeyLeagueTab = VALID_TABS.includes(tabParam as HockeyLeagueTab)
    ? (tabParam as HockeyLeagueTab)
    : tabParam === 'standings'
      ? 'statistics'
      : 'summary';

  const [season, setSeason] = useState<HockeySeasonDto | null>(null);
  const [teams, setTeams] = useState<HockeyTeamDto[]>([]);
  const [allMatches, setAllMatches] = useState<HockeyMatchDto[]>([]);
  const [standings, setStandings] = useState<HockeyTeamCompetitionStatisticsDto[]>([]);
  const [players, setPlayers] = useState<HockeyPlayerCompetitionStatisticsDto[]>([]);
  const [goalies, setGoalies] = useState<HockeyGoalieCompetitionStatisticsDto[]>([]);
  const [playerNames, setPlayerNames] = useState<Map<string, string>>(new Map());
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [matchesLoading, setMatchesLoading] = useState(false);
  const matchesError: string | null = null;
  const [contentBlocks, setContentBlocks] = useState<SeasonContentBlockDto[]>([]);
  const [currentPage, setCurrentPage] = useState(1);

  const teamNames = useMemo(
    () => new Map(teams.map((team) => [team.id, team.name])),
    [teams],
  );
  const teamLogos = useMemo(
    () => new Map(teams.map((team) => [team.id, team.logoUrl])),
    [teams],
  );

  useEffect(() => {
    if (!id) {
      return;
    }
    const load = async (): Promise<void> => {
      setLoading(true);
      setMatchesLoading(true);
      setError(null);
      try {
        const [loaded, standingList, playerList, goalieList, teamList, content] = await Promise.all([
          hockeySeasonService.getById(id),
          hockeyStatisticsService.getStandings(id).catch(() => []),
          hockeyStatisticsService.getPlayers(id).catch(() => []),
          hockeyStatisticsService.getGoalies(id).catch(() => []),
          hockeyTeamService.getAll(audience.teamCategory),
          hockeySeasonService.getContentBlocks(id).catch(() => ({ seasonId: id, blocks: [] })),
        ]);
        setSeason(loaded);
        setContentBlocks(content.blocks);
        setStandings(uniqueHockeyStandingsByTeamId(standingList));
        setGoalies(goalieList);
        setTeams(teamList);
        const names = await loadHockeyRosterNameMaps(teamList);
        setPlayerNames(names.byPlayerId);
        const matchList = await hockeyMatchService.getByCompetition(id).catch(() => []);
        setAllMatches(matchList);
        setPlayers(mergeHockeyPlayerFaceoffWins(playerList, matchList));
      } catch (err) {
        setError(err instanceof Error ? err.message : t('leaguePage.errors.loadLeagueData'));
      } finally {
        setLoading(false);
        setMatchesLoading(false);
      }
    };
    void load();
  }, [id, audience.teamCategory, t]);

  const refreshLiveMatches = useCallback(async (): Promise<void> => {
    if (!id) {
      return;
    }
    try {
      const matchList = await hockeyMatchService.getByCompetition(id);
      setAllMatches(matchList);
    } catch {
      /* keep last known scores */
    }
  }, [id]);

  const hasLiveMatches = shouldRefreshHockeyMatches(allMatches);
  useIntervalWhen(hasLiveMatches, () => {
    void refreshLiveMatches();
  }, 4000);

  const totalGoals = useMemo(
    () => standings.reduce((sum, row) => sum + row.goalsFor, 0),
    [standings],
  );
  const matchesPlayed = useMemo(
    () => standings.reduce((sum, row) => sum + row.gamesPlayed, 0) / 2,
    [standings],
  );

  const setTab = (next: HockeyLeagueTab): void => {
    setSearchParams({ tab: next });
    setCurrentPage(1);
  };

  const handlePageChange = useCallback((page: number) => {
    setCurrentPage(page);
  }, []);

  const tabs: Array<{ key: HockeyLeagueTab; label: string }> = [
    { key: 'summary', label: t('leaguePage.tabs.summary') },
    { key: 'statistics', label: t('leaguePage.tabs.statistics') },
    { key: 'players', label: t('hockeyPage.playerStats') },
    { key: 'results', label: t('leaguePage.tabs.results') },
    { key: 'fixtures', label: t('leaguePage.tabs.fixtures') },
  ];

  const resultsMatches = useMemo(() => {
    const finished = allMatches
      .filter((match) => isHockeyMatchFinished(match.status))
      .sort(
        (left, right) =>
          new Date(right.scheduledStartTime).getTime() - new Date(left.scheduledStartTime).getTime(),
      );
    const pageSize = 10;
    const pages = Math.max(1, Math.ceil(finished.length / pageSize));
    const start = (currentPage - 1) * pageSize;
    return {
      items: finished.slice(start, start + pageSize),
      totalPages: finished.length === 0 ? 1 : pages,
    };
  }, [allMatches, currentPage]);

  const fixtureMatches = useMemo(() => {
    const list = [...allMatches].sort(
      (left, right) =>
        new Date(left.scheduledStartTime).getTime() - new Date(right.scheduledStartTime).getTime(),
    );
    const pageSize = 20;
    const pages = Math.max(1, Math.ceil(list.length / pageSize));
    const start = (currentPage - 1) * pageSize;
    return {
      items: list.slice(start, start + pageSize).map((match) => toFixtureMatch(match, teamNames, teamLogos)),
      totalPages: list.length === 0 ? 1 : pages,
    };
  }, [allMatches, currentPage, teamNames, teamLogos]);

  const renderTabContent = () => {
    switch (tab) {
      case 'summary':
        return (
          <div className="summary-section">
            <SeasonInfoCards blocks={contentBlocks} className="season-info-cards" />
            {loading ? (
              <div className="summary-section__loading">{t('leaguePage.summary.loading')}</div>
            ) : error ? (
              <div className="summary-section__error">{t('leaguePage.summary.error', { error })}</div>
            ) : (
              <>
                <div className="summary-section__stats">
                  <div className="summary-section__stat-card">
                    <span className="summary-section__stat-value">{season?.teams.length ?? 0}</span>
                    <span className="summary-section__stat-label">{t('leaguePage.summary.teams')}</span>
                  </div>
                  <div className="summary-section__stat-card">
                    <span className="summary-section__stat-value">{Math.round(matchesPlayed)}</span>
                    <span className="summary-section__stat-label">{t('leaguePage.summary.matchesPlayed')}</span>
                  </div>
                  <div className="summary-section__stat-card">
                    <span className="summary-section__stat-value">{totalGoals}</span>
                    <span className="summary-section__stat-label">{t('leaguePage.summary.goalsScored')}</span>
                  </div>
                </div>
                {standings.length > 0 && (
                  <div className="summary-section__standings">
                    <div className="summary-section__standings-header">
                      <h3 className="summary-section__standings-title">
                        {t('leaguePage.summary.standingsPreview')}
                      </h3>
                    </div>
                    <HockeyStandingsTable
                      standings={standings}
                      teamNames={teamNames}
                      previewLimit={STANDINGS_PREVIEW}
                    />
                    <button
                      type="button"
                      className="summary-section__view-full"
                      onClick={() => setTab('statistics')}
                    >
                      {t('leaguePage.summary.viewFullStandings')} &rarr;
                    </button>
                  </div>
                )}
                {players.length > 0 && (
                  <div className="standing-container">
                    <HockeyPlayerStatsTables
                      players={[...players].sort((left, right) => right.points - left.points).slice(0, 10)}
                      goalies={[...goalies].sort((left, right) => right.savePercentage - left.savePercentage).slice(0, 5)}
                      playerNames={playerNames}
                      teamNames={teamNames}
                    />
                    <button
                      type="button"
                      className="summary-section__view-full"
                      onClick={() => setTab('players')}
                    >
                      {t('hockeyPage.viewFullStats')} &rarr;
                    </button>
                  </div>
                )}
              </>
            )}
          </div>
        );
      case 'statistics':
        return (
          <div className="standing-container">
            <div className="standing-header">
              <div className="header-top-row">
                <span className="league-title">
                  {t('hockeyPage.standingsTitle')} {season?.name}
                </span>
              </div>
            </div>
            <HockeyStandingsTable standings={standings} teamNames={teamNames} />
          </div>
        );
      case 'players':
        return (
          <HockeyPlayerStatsTables
            players={players}
            goalies={goalies}
            playerNames={playerNames}
            teamNames={teamNames}
          />
        );
      case 'results':
        return (
          <div className="results-section">
            {matchesLoading ? (
              <div className="no-matches">{t('leaguePage.summary.loading')}</div>
            ) : matchesError ? (
              <div className="no-matches">{matchesError}</div>
            ) : resultsMatches.items.length === 0 ? (
              <div className="no-matches">{t('hockeyPage.noMatches')}</div>
            ) : (
              <>
                <div className="matches-grid">
                  {resultsMatches.items.map((match) => (
                    <HockeyMatchRow key={match.id} match={match} teamNames={teamNames} />
                  ))}
                </div>
                {resultsMatches.totalPages > 1 && (
                  <div className="schedule-section__pagination">
                    <button
                      type="button"
                      onClick={() => handlePageChange(currentPage - 1)}
                      disabled={currentPage === 1}
                      className="schedule-section__page-btn"
                    >
                      {t('common.pagination.previous')}
                    </button>
                    <span className="schedule-section__page-info">
                      {t('common.pagination.pageOf', { current: currentPage, total: resultsMatches.totalPages })}
                    </span>
                    <button
                      type="button"
                      onClick={() => handlePageChange(currentPage + 1)}
                      disabled={currentPage === resultsMatches.totalPages}
                      className="schedule-section__page-btn"
                    >
                      {t('common.pagination.next')}
                    </button>
                  </div>
                )}
              </>
            )}
          </div>
        );
      case 'fixtures':
        return (
          <SharedFixturesSection
            sport="hockey"
            matches={fixtureMatches.items}
            matchesLoading={matchesLoading}
            matchesError={matchesError}
            currentPage={currentPage}
            totalPages={fixtureMatches.totalPages}
            handlePageChange={handlePageChange}
          />
        );
      default:
        return null;
    }
  };

  return (
    <PageTemplate title={season?.name ?? t('leaguePage.defaultTitle')}>
      <div className="league-page">
        <div className="hero-image-container">
          <div className="hero-image" />
          <div className="league-header">
            <div className="header-content">
              <div className="league-branding">
                <div className="league-icon">
                  <div className="trophy-icon">🏆</div>
                </div>
              </div>
              <div className="league-info">
                <h1 className="league-title">{season?.name ?? t('leaguePage.defaultTitle')}</h1>
                <div className="league-tabs">
                  {tabs.map((item) => (
                    <button
                      key={item.key}
                      type="button"
                      className={`tab-button ${tab === item.key ? 'active' : ''}`}
                      onClick={() => setTab(item.key)}
                    >
                      {item.label}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
        <div className="league-content">
          {renderTabContent()}
        </div>
      </div>
    </PageTemplate>
  );
}

export default HockeyLeaguePage;
