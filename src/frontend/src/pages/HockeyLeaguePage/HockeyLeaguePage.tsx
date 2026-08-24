import { useCallback, useEffect, useMemo, useState } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import HockeyMatchRow from '../../components/HockeyMatchRow/HockeyMatchRow';
import HockeyStandingsTable from './HockeyStandingsTable';
import HockeyPlayerStatsTables from './HockeyPlayerStatsTables';
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
} from '../../types/hockey/hockeyTypes';
import { isHockeyMatchFinished, shouldRefreshHockeyMatches } from '../../types/hockey/hockeyTypes';
import { loadHockeyRosterNameMaps, loadTeamNameMap, mergeHockeyPlayerFaceoffWins, uniqueHockeyStandingsByTeamId } from '../../utils/hockeyLookups';
import { useAudience } from '../../context/AudienceContext';
import { useIntervalWhen } from '../../hooks/useIntervalWhen';
import '../LeaguePage/LeaguePage.scss';
import '../LeaguePage/components/SummarySection.scss';
import '../../components/LeagueStanding/LeagueStanding.scss';
import '../../components/MatchesList/MatchesList.scss';

type HockeyLeagueTab = 'summary' | 'results' | 'fixtures' | 'statistics' | 'players';

const VALID_TABS: HockeyLeagueTab[] = ['summary', 'results', 'fixtures', 'statistics', 'players'];
const STANDINGS_PREVIEW = 8;

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
  const [matches, setMatches] = useState<HockeyMatchDto[]>([]);
  const [standings, setStandings] = useState<HockeyTeamCompetitionStatisticsDto[]>([]);
  const [players, setPlayers] = useState<HockeyPlayerCompetitionStatisticsDto[]>([]);
  const [goalies, setGoalies] = useState<HockeyGoalieCompetitionStatisticsDto[]>([]);
  const [teamNames, setTeamNames] = useState<Map<string, string>>(new Map());
  const [playerNames, setPlayerNames] = useState<Map<string, string>>(new Map());
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      return;
    }
    const load = async (): Promise<void> => {
      const [loaded, matchList, standingList, playerList, goalieList, teams] = await Promise.all([
        hockeySeasonService.getById(id),
        hockeyMatchService.getByCompetition(id),
        hockeyStatisticsService.getStandings(id).catch(() => []),
        hockeyStatisticsService.getPlayers(id).catch(() => []),
        hockeyStatisticsService.getGoalies(id).catch(() => []),
        hockeyTeamService.getAll(audience.teamCategory),
      ]);
      setSeason(loaded);
      setMatches(matchList);
      setStandings(uniqueHockeyStandingsByTeamId(standingList));
      setPlayers(mergeHockeyPlayerFaceoffWins(playerList, matchList));
      setGoalies(goalieList);
      setTeamNames(await loadTeamNameMap(teams));
      const names = await loadHockeyRosterNameMaps(teams);
      setPlayerNames(names.byPlayerId);
    };
    void load().catch((err) => setError(err instanceof Error ? err.message : 'Failed to load league'));
  }, [id, audience.teamCategory]);

  const refreshLiveMatches = useCallback(async (): Promise<void> => {
    if (!id) {
      return;
    }
    try {
      setMatches(await hockeyMatchService.getByCompetition(id));
    } catch {
      /* keep last known scores */
    }
  }, [id]);

  const hasLiveMatches = shouldRefreshHockeyMatches(matches);
  useIntervalWhen(hasLiveMatches, () => {
    void refreshLiveMatches();
  }, 4000);

  const results = useMemo(
    () => matches.filter((match) => isHockeyMatchFinished(match.status)),
    [matches],
  );
  const fixtures = useMemo(
    () => matches.filter((match) => !isHockeyMatchFinished(match.status) && match.status !== 'Cancelled'),
    [matches],
  );
  const totalGoals = useMemo(
    () => standings.reduce((sum, row) => sum + row.goalsFor, 0),
    [standings],
  );

  const setTab = (next: HockeyLeagueTab): void => {
    setSearchParams({ tab: next });
  };

  const tabs: Array<{ key: HockeyLeagueTab; label: string }> = [
    { key: 'summary', label: t('leaguePage.tabs.summary') },
    { key: 'statistics', label: t('leaguePage.tabs.statistics') },
    { key: 'players', label: t('hockeyPage.playerStats', 'Player Statistics') },
    { key: 'results', label: t('leaguePage.tabs.results') },
    { key: 'fixtures', label: t('leaguePage.tabs.fixtures') },
  ];

  const renderMatches = (list: HockeyMatchDto[], variant: 'results' | 'fixtures') => (
    <div className={variant === 'results' ? 'results-section' : 'fixtures-section'}>
      {list.length === 0 ? (
        <div className="no-matches">{t('hockeyPage.noMatches', 'No matches')}</div>
      ) : (
        <div className="matches-grid">
          {list.map((match) => (
            <HockeyMatchRow key={match.id} match={match} teamNames={teamNames} />
          ))}
        </div>
      )}
    </div>
  );

  return (
    <PageTemplate title={season?.name ?? t('hockeyPage.title', 'Hockey')}>
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
                <h1 className="league-title">{season?.name ?? t('common.loading', 'Loading...')}</h1>
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
        {error && <p>{error}</p>}
        <div className="league-content">
          {tab === 'summary' && (
            <div className="summary-section">
              <div className="summary-section__stats">
                <div className="summary-section__stat-card">
                  <span className="summary-section__stat-value">{season?.teams.length ?? 0}</span>
                  <span className="summary-section__stat-label">{t('leaguePage.summary.teams')}</span>
                </div>
                <div className="summary-section__stat-card">
                  <span className="summary-section__stat-value">{results.length}</span>
                  <span className="summary-section__stat-label">{t('leaguePage.summary.matchesPlayed')}</span>
                </div>
                <div className="summary-section__stat-card">
                  <span className="summary-section__stat-value">{totalGoals}</span>
                  <span className="summary-section__stat-label">{t('leaguePage.summary.goalsScored')}</span>
                </div>
              </div>
              {standings.length > 0 && (
                <div className="standing-container">
                  <div className="standing-header">
                    <div className="header-top-row">
                      <span className="league-title">{t('leaguePage.summary.standingsPreview')}</span>
                    </div>
                  </div>
                  <HockeyStandingsTable
                    standings={standings}
                    teamNames={teamNames}
                    previewLimit={STANDINGS_PREVIEW}
                  />
                  <button type="button" className="summary-section__view-full" onClick={() => setTab('statistics')}>
                    {t('leaguePage.summary.viewFullStandings')} &rarr;
                  </button>
                </div>
              )}
              {players.length > 0 && (
                <div className="standing-container">
                  <HockeyPlayerStatsTables
                    players={[...players].sort((a, b) => b.points - a.points).slice(0, 10)}
                    goalies={[...goalies].sort((a, b) => b.savePercentage - a.savePercentage).slice(0, 5)}
                    playerNames={playerNames}
                    teamNames={teamNames}
                  />
                  <button type="button" className="summary-section__view-full" onClick={() => setTab('players')}>
                    {t('hockeyPage.viewFullStats', 'View full player statistics')} &rarr;
                  </button>
                </div>
              )}
            </div>
          )}
          {tab === 'statistics' && (
            <div className="standing-container">
              <div className="standing-header">
                <div className="header-top-row">
                  <span className="league-title">{t('hockeyPage.standingsTitle', 'STANDINGS')} {season?.name}</span>
                </div>
              </div>
              <HockeyStandingsTable standings={standings} teamNames={teamNames} />
            </div>
          )}
          {tab === 'players' && (
            <HockeyPlayerStatsTables
              players={players}
              goalies={goalies}
              playerNames={playerNames}
              teamNames={teamNames}
            />
          )}
          {tab === 'results' && renderMatches(results, 'results')}
          {tab === 'fixtures' && renderMatches(fixtures, 'fixtures')}
        </div>
      </div>
    </PageTemplate>
  );
}

export default HockeyLeaguePage;
