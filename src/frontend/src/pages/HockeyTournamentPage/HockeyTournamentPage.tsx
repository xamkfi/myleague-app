import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import HockeyMatchRow from '../../components/HockeyMatchRow/HockeyMatchRow';
import HockeyStandingsTable from '../HockeyLeaguePage/HockeyStandingsTable';
import { hockeyTournamentService } from '../../api/hockey/hockeyTournamentService';
import { hockeyMatchService } from '../../api/hockey/hockeyMatchService';
import { hockeyStatisticsService } from '../../api/hockey/hockeyStatisticsService';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import type {
  HockeyMatchDto,
  HockeyTeamCompetitionStatisticsDto,
  HockeyTournamentDto,
} from '../../types/hockey/hockeyTypes';
import { shouldRefreshHockeyMatches } from '../../types/hockey/hockeyTypes';
import { loadTeamNameMap, uniqueHockeyStandingsByTeamId } from '../../utils/hockeyLookups';
import { useAudience } from '../../context/AudienceContext';
import { useIntervalWhen } from '../../hooks/useIntervalWhen';
import '../TournamentPage/TournamentPage.scss';
import '../../components/MatchesList/MatchesList.scss';
import '../../components/LeagueStanding/LeagueStanding.scss';

function HockeyTournamentPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const { id } = useParams<{ id: string }>();
  const [tournament, setTournament] = useState<HockeyTournamentDto | null>(null);
  const [matches, setMatches] = useState<HockeyMatchDto[]>([]);
  const [teamNames, setTeamNames] = useState<Map<string, string>>(new Map());
  const [groupStandings, setGroupStandings] = useState<Map<string, HockeyTeamCompetitionStatisticsDto[]>>(new Map());
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      return;
    }
    const load = async (): Promise<void> => {
      const [loaded, matchList, teams] = await Promise.all([
        hockeyTournamentService.getById(id),
        hockeyMatchService.getByCompetition(id),
        hockeyTeamService.getAll(audience.teamCategory),
      ]);
      setTournament(loaded);
      setMatches(matchList);
      setTeamNames(await loadTeamNameMap(teams));
      const standingsEntries = await Promise.all(
        loaded.groups.map(async (group) => {
          const rows = await hockeyStatisticsService.getGroupStandings(id, group.id).catch(() => []);
          return [group.id, uniqueHockeyStandingsByTeamId(rows)] as const;
        }),
      );
      setGroupStandings(new Map(standingsEntries));
    };
    void load().catch((err) => setError(err instanceof Error ? err.message : 'Failed to load tournament'));
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

  return (
    <PageTemplate title={tournament?.name ?? t('hockey.tournaments.title', 'Tournament')}>
      <div className="tournament-page">
        <div className="tournament-page__hero">
          <h1 className="tournament-page__title">{tournament?.name}</h1>
          <p>{tournament?.currentStage} · {tournament?.status}</p>
        </div>
        {error && <p>{error}</p>}
        {tournament?.groups.map((group) => {
          const standings = groupStandings.get(group.id) ?? [];
          return (
            <section key={group.id} className="standing-container">
              <h2>{group.name}</h2>
              {standings.length > 0 ? (
                <HockeyStandingsTable standings={standings} teamNames={teamNames} />
              ) : (
                <ul>
                  {group.teams.map((member) => {
                    const competitionTeam = tournament.teams.find((item) => item.id === member.competitionTeamId);
                    return (
                      <li key={member.id}>
                        {competitionTeam
                          ? teamNames.get(competitionTeam.teamId) ?? member.competitionTeamId.slice(0, 8)
                          : member.competitionTeamId.slice(0, 8)}
                      </li>
                    );
                  })}
                </ul>
              )}
            </section>
          );
        })}
        <h2>{t('hockey.matches.title', 'Matches')}</h2>
        <div className="results-section">
          <div className="matches-grid">
            {matches.map((match) => (
              <HockeyMatchRow key={match.id} match={match} teamNames={teamNames} />
            ))}
          </div>
        </div>
      </div>
    </PageTemplate>
  );
}

export default HockeyTournamentPage;
