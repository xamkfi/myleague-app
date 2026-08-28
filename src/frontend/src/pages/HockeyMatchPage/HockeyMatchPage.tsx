import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import HockeyMatchEvents from './HockeyMatchEvents';
import HockeyMatchStats from './HockeyMatchStats';
import HockeyMatchLineups from './HockeyMatchLineups';
import { hockeyMatchService } from '../../api/hockey/hockeyMatchService';
import { hockeyStatisticsService } from '../../api/hockey/hockeyStatisticsService';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import { hockeySeasonService } from '../../api/hockey/hockeySeasonService';
import { hockeyTournamentService } from '../../api/hockey/hockeyTournamentService';
import type { HockeyMatchDto, HockeyMatchStatisticsDto, HockeyTeamDto } from '../../types/hockey/hockeyTypes';
import { isHockeyMatchFinished, isHockeyMatchLive } from '../../types/hockey/hockeyTypes';
import { useAudience } from '../../context/AudienceContext';
import { useIntervalWhen } from '../../hooks/useIntervalWhen';
import {
  hockeyStatusTranslationKey,
  loadHockeyRosterNameMaps,
  loadTeamNameMap,
  mergeHockeyMatchFaceoffWins,
} from '../../utils/hockeyLookups';
import { MatchPageShell, type MatchTabType } from '../../components/match';
import { getTeamPath, getLeaguePath, getTournamentPath } from '../../utils/sportRoutes';
import { getTeamSlug } from '../../utils/slugUtils';
import '../MatchPage/MatchPage.scss';
import '../../components/LeagueStanding/LeagueStanding.scss';

function HockeyMatchPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
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
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<MatchTabType>('summary');

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
      setStats(box ? mergeHockeyMatchFaceoffWins(box, loaded) : null);
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
        hockeyTeamService.getAll(audience.teamCategory),
      ]);
      setMatch(loaded);
      setTeams(teamList);
      setTeamNames(await loadTeamNameMap(teamList));
      const names = await loadHockeyRosterNameMaps(teamList);
      setPlayerNames(names.byTeamPlayerId);
      setCareerPlayerNames(names.byPlayerId);
      const box = await hockeyStatisticsService.getMatchStats(id).catch(() => null);
      setStats(box ? mergeHockeyMatchFaceoffWins(box, loaded) : null);
      if (loaded.competitionId) {
        const season = await hockeySeasonService.getById(loaded.competitionId).catch(() => null);
        if (season) {
          setCompetitionName(season.name);
          setCompetitionPath(getLeaguePath('hockey', season.id));
        } else {
          const tournament = await hockeyTournamentService.getById(loaded.competitionId).catch(() => null);
          setCompetitionName(tournament?.name ?? '');
          setCompetitionPath(tournament ? getTournamentPath('hockey', tournament.id) : '');
        }
      }
    };
    void load()
      .catch((err) => setError(err instanceof Error ? err.message : 'Failed to load match'))
      .finally(() => setLoading(false));
  }, [id, audience.teamCategory]);

  const liveOrUpcoming = Boolean(
    match && !isHockeyMatchFinished(match.status) && match.status !== 'Cancelled',
  );
  useIntervalWhen(liveOrUpcoming, () => {
    void refreshLiveData();
  }, 3000);

  const namedTeams = teams.map((team) => ({ id: team.id, name: team.name }));
  const homeName = match?.homeTeamId ? teamNames.get(match.homeTeamId) ?? t('hockeyPage.home', 'Home') : 'TBD';
  const awayName = match?.awayTeamId ? teamNames.get(match.awayTeamId) ?? t('hockeyPage.away', 'Away') : 'TBD';

  return (
    <MatchPageShell
      isLoading={loading}
      error={error}
      competitionName={competitionName}
      competitionPath={competitionPath}
      header={
        match
          ? {
              home: {
                name: homeName,
                logo: null,
                href: match.homeTeamId
                  ? getTeamPath('hockey', getTeamSlug({ id: match.homeTeamId, name: homeName }, namedTeams))
                  : null,
              },
              away: {
                name: awayName,
                logo: null,
                href: match.awayTeamId
                  ? getTeamPath('hockey', getTeamSlug({ id: match.awayTeamId, name: awayName }, namedTeams))
                  : null,
              },
              homeScore: match.homeScore,
              awayScore: match.awayScore,
              scheduledDateTime: match.scheduledStartTime,
              isScheduled: match.status === 'Scheduled',
              isLive: isHockeyMatchLive(match.status),
              isFinal: isHockeyMatchFinished(match.status),
            }
          : undefined
      }
      activeTab={activeTab}
      onTabChange={setActiveTab}
      tableVariant="season"
      showTableTab={false}
      showStatsTab
    >
      {match && activeTab === 'summary' && (
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
            <HockeyMatchEvents
              match={match}
              homeName={homeName}
              awayName={awayName}
              playerNames={playerNames}
            />
          </div>
        </div>
      )}
      {match && activeTab === 'stats' && (
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
      {match && activeTab === 'lineups' && (
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
    </MatchPageShell>
  );
}

export default HockeyMatchPage;
