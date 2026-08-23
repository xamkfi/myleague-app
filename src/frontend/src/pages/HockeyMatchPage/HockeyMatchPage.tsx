import { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import HockeyMatchHeader from './HockeyMatchHeader';
import HockeyMatchEvents from './HockeyMatchEvents';
import HockeyMatchStats from './HockeyMatchStats';
import HockeyMatchLineups from './HockeyMatchLineups';
import { hockeyMatchService } from '../../api/hockey/hockeyMatchService';
import { hockeyStatisticsService } from '../../api/hockey/hockeyStatisticsService';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import { hockeySeasonService } from '../../api/hockey/hockeySeasonService';
import { hockeyTournamentService } from '../../api/hockey/hockeyTournamentService';
import type { HockeyMatchDto, HockeyMatchStatisticsDto, HockeyTeamDto } from '../../types/hockey/hockeyTypes';
import { isHockeyMatchFinished } from '../../types/hockey/hockeyTypes';
import { useIntervalWhen } from '../../hooks/useIntervalWhen';
import {
  hockeyStatusTranslationKey,
  loadHockeyRosterNameMaps,
  loadTeamNameMap,
} from '../../utils/hockeyLookups';
import '../MatchPage/MatchPage.scss';
import '../../components/LeagueStanding/LeagueStanding.scss';

type HockeyMatchTab = 'summary' | 'stats' | 'lineups';

function HockeyMatchPage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const [match, setMatch] = useState<HockeyMatchDto | null>(null);
  const [stats, setStats] = useState<HockeyMatchStatisticsDto | null>(null);
  const [teams, setTeams] = useState<HockeyTeamDto[]>([]);
  const [teamNames, setTeamNames] = useState<Map<string, string>>(new Map());
  const [playerNames, setPlayerNames] = useState<Map<string, string>>(new Map());
  const [careerPlayerNames, setCareerPlayerNames] = useState<Map<string, string>>(new Map());
  const [competitionName, setCompetitionName] = useState('');
  const [competitionPath, setCompetitionPath] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<HockeyMatchTab>('summary');

  const refreshLiveData = useCallback(async (): Promise<void> => {
    if (!id) {
      return;
    }
    try {
      const [loaded, box] = await Promise.all([
        hockeyMatchService.getById(id),
        hockeyStatisticsService.getMatchStats(id).catch(() => null),
      ]);
      setMatch(loaded);
      setStats(box);
    } catch {
      /* keep last known live state */
    }
  }, [id]);

  useEffect(() => {
    if (!id) {
      return;
    }
    const load = async (): Promise<void> => {
      const [loaded, teamList] = await Promise.all([
        hockeyMatchService.getById(id),
        hockeyTeamService.getAll(),
      ]);
      setMatch(loaded);
      setTeams(teamList);
      setTeamNames(await loadTeamNameMap(teamList));
      const names = await loadHockeyRosterNameMaps(teamList);
      setPlayerNames(names.byTeamPlayerId);
      setCareerPlayerNames(names.byPlayerId);
      const box = await hockeyStatisticsService.getMatchStats(id).catch(() => null);
      setStats(box);
      if (loaded.competitionId) {
        const season = await hockeySeasonService.getById(loaded.competitionId).catch(() => null);
        if (season) {
          setCompetitionName(season.name);
          setCompetitionPath(`/hockey/league/${season.id}`);
        } else {
          const tournament = await hockeyTournamentService.getById(loaded.competitionId).catch(() => null);
          setCompetitionName(tournament?.name ?? '');
          setCompetitionPath(tournament ? `/hockey/tournaments/${tournament.id}` : '');
        }
      }
    };
    void load().catch((err) => setError(err instanceof Error ? err.message : 'Failed to load match'));
  }, [id]);

  const liveOrUpcoming = Boolean(
    match && !isHockeyMatchFinished(match.status) && match.status !== 'Cancelled',
  );
  useIntervalWhen(liveOrUpcoming, () => {
    void refreshLiveData();
  }, 3000);

  const homeName = match?.homeTeamId ? teamNames.get(match.homeTeamId) ?? t('hockeyPage.home', 'Home') : 'TBD';
  const awayName = match?.awayTeamId ? teamNames.get(match.awayTeamId) ?? t('hockeyPage.away', 'Away') : 'TBD';

  const tabs: Array<{ key: HockeyMatchTab; label: string }> = [
    { key: 'summary', label: t('matchPage.navigation.summary') },
    { key: 'stats', label: t('matchPage.navigation.stats') },
    { key: 'lineups', label: t('matchPage.navigation.lineups') },
  ];

  return (
    <div className="match-page-wrapper">
      <PageTemplate title={`${homeName} vs ${awayName}`}>
        <div className="match-page">
          {error && <div className="error">{error}</div>}
          {!match ? (
            <div className="loading">{t('common.loading', 'Loading...')}</div>
          ) : (
            <>
              {competitionPath && (
                <p>
                  <Link to={competitionPath}>
                    {competitionName || t('hockeyPage.title', 'Ice hockey')}
                  </Link>
                </p>
              )}
              <HockeyMatchHeader match={match} homeName={homeName} awayName={awayName} />
              <div className="navigation-tabs" role="tablist">
                {tabs.map((item) => (
                  <button
                    key={item.key}
                    type="button"
                    className={`nav-tab ${activeTab === item.key ? 'active' : ''}`}
                    onClick={() => setActiveTab(item.key)}
                    role="tab"
                    aria-selected={activeTab === item.key}
                  >
                    {item.label}
                  </button>
                ))}
              </div>
              {activeTab === 'summary' && (
                <div className="tab-content">
                  <div className="summary-content">
                    <div className="match-info">
                      {match.venue && <p>{t('matchPage.matchInfo.venue')}: {match.venue}</p>}
                      <p>
                        {t('matchPage.matchInfo.status')}:{' '}
                        {t(hockeyStatusTranslationKey(match.status), match.status)}
                      </p>
                      {match.wentToOvertime && <p>{t('matchPage.matchInfo.overtime')}</p>}
                      {match.wentToShootout && <p>{t('matchPage.matchInfo.shootout')}</p>}
                    </div>
                    <div className="period-scores">
                      <div className="period-scores-grid">
                        {match.periodScores.map((period) => (
                          <div key={period.id} className="period-score">
                            <strong>
                              {period.periodType === 'Overtime'
                                ? t('hockey.matches.overtime', 'OT')
                                : period.periodType === 'Shootout'
                                  ? t('hockey.matches.shootout', 'SO')
                                  : t('hockey.matches.periodN', 'P{{number}}', { number: period.periodNumber })}
                            </strong>
                            <span>{period.homeGoals}–{period.awayGoals}</span>
                          </div>
                        ))}
                      </div>
                    </div>
                    <HockeyMatchEvents
                      match={match}
                      homeName={homeName}
                      awayName={awayName}
                      playerNames={playerNames}
                    />
                  </div>
                </div>
              )}
              {activeTab === 'stats' && (
                <div className="tab-content">
                  {!stats ? (
                    <p>{t('hockeyPage.noStats', 'No statistics yet')}</p>
                  ) : (
                    <HockeyMatchStats
                      stats={stats}
                      homeName={homeName}
                      awayName={awayName}
                      homeTeamId={match.homeTeamId}
                      awayTeamId={match.awayTeamId}
                      playerNames={careerPlayerNames}
                    />
                  )}
                </div>
              )}
              {activeTab === 'lineups' && (
                <div className="tab-content">
                  <HockeyMatchLineups
                    match={match}
                    homeName={homeName}
                    awayName={awayName}
                    teams={teams}
                    playerNames={playerNames}
                  />
                </div>
              )}
            </>
          )}
        </div>
      </PageTemplate>
    </div>
  );
}

export default HockeyMatchPage;
