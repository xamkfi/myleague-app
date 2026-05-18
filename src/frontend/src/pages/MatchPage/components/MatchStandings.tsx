import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  floorballStatisticsService,
  type FloorballSeasonStatisticsSummaryDto,
} from '../../../api/floorball/floorballStatistics';
import { floorballTournamentService } from '../../../api/floorball/floorballTournamentService';
import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import type {
  FloorballPlayoffBracketDto,
  FloorballTournamentDto,
} from '../../../types/floorball/tournamentTypes';
import LeagueStanding from '../../../components/LeagueStanding/LeagueStanding';
import TournamentGroupStandingsTable from '../../../components/TournamentGroupStandingsTable/TournamentGroupStandingsTable';
import TournamentBracket from '../../../components/TournamentBracket/TournamentBracket';
import { isTournamentCompetition } from '../../../utils/competitionPath';

interface MatchStandingsProps {
  match: FloorballMatchDto;
}

/**
 * Renders the "Sarjataulukko" tab for a match. Behaviour depends on the competition kind:
 *
 *  - Season match              → traditional league-wide standings table (unchanged).
 *  - Tournament group-stage    → only the relevant group's standings, with qualifying
 *                                teams highlighted (uses `TournamentGroupStandingsTable`).
 *  - Tournament playoff match  → the playoff bracket so the user can see this match's
 *                                position in the bracket (uses `TournamentBracket`).
 *
 * Top scorers / assists / goalies tabs are kept untouched and continue to render via the
 * shared `LeagueStanding` component using the season-statistics endpoint, which works for
 * tournaments too thanks to the TPH `competitionId` schema.
 */
export default function MatchStandings({ match }: MatchStandingsProps) {
  const { t } = useTranslation();
  const [statsLoading, setStatsLoading] = useState<boolean>(true);
  const [statsError, setStatsError] = useState<string | null>(null);
  const [seasonStats, setSeasonStats] = useState<FloorballSeasonStatisticsSummaryDto | null>(null);

  // Tournament-only state — populated lazily when the match is a tournament match.
  const [tournament, setTournament] = useState<FloorballTournamentDto | null>(null);
  const [tournamentError, setTournamentError] = useState<string | null>(null);
  const [tournamentLoading, setTournamentLoading] = useState<boolean>(false);
  const [bracket, setBracket] = useState<FloorballPlayoffBracketDto | null>(null);
  const [bracketError, setBracketError] = useState<string | null>(null);
  const [bracketLoading, setBracketLoading] = useState<boolean>(false);

  const isTournament: boolean = useMemo(
    () =>
      isTournamentCompetition({
        tournamentGroupId: match.tournamentGroupId,
        tournamentStage: match.tournamentStage,
      }),
    [match.tournamentGroupId, match.tournamentStage],
  );

  const isGroupStageMatch: boolean = isTournament && Boolean(match.tournamentGroupId);
  const isPlayoffMatch: boolean = isTournament && !match.tournamentGroupId;

  // Load season-statistics summary (used by scorers/assists/goalies in all cases, and as the
  // standings source for season matches).
  useEffect(() => {
    let cancelled = false;
    const fetchSeasonStats = async () => {
      try {
        setStatsLoading(true);
        setStatsError(null);
        const data = await floorballStatisticsService.getSeasonStatistics(match.competitionId);
        if (!cancelled) {
          setSeasonStats(data);
        }
      } catch (err) {
        if (!cancelled) {
          setStatsError(err instanceof Error ? err.message : 'Failed to load season statistics');
        }
      } finally {
        if (!cancelled) {
          setStatsLoading(false);
        }
      }
    };
    fetchSeasonStats();
    return () => {
      cancelled = true;
    };
  }, [match.competitionId]);

  // For tournament group-stage matches we need the group's display name + the
  // `teamsAdvancingPerGroup` rule so the table can highlight qualifying teams.
  useEffect(() => {
    if (!isGroupStageMatch) {
      return;
    }
    let cancelled = false;
    const fetchTournament = async () => {
      try {
        setTournamentLoading(true);
        setTournamentError(null);
        const response = await floorballTournamentService.getById(match.competitionId);
        if (!cancelled && response.success && response.data) {
          setTournament(response.data);
        } else if (!cancelled) {
          setTournamentError(response.message ?? 'Failed to load tournament details');
        }
      } catch (err) {
        if (!cancelled) {
          setTournamentError(err instanceof Error ? err.message : 'Failed to load tournament details');
        }
      } finally {
        if (!cancelled) {
          setTournamentLoading(false);
        }
      }
    };
    fetchTournament();
    return () => {
      cancelled = true;
    };
  }, [isGroupStageMatch, match.competitionId]);

  // For tournament playoff matches we render the bracket; fetch it lazily.
  useEffect(() => {
    if (!isPlayoffMatch) {
      return;
    }
    let cancelled = false;
    const fetchBracket = async () => {
      try {
        setBracketLoading(true);
        setBracketError(null);
        const response = await floorballTournamentService.getPlayoffBracket(match.competitionId);
        if (!cancelled && response.success && response.data) {
          setBracket(response.data);
        } else if (!cancelled) {
          setBracketError(response.message ?? 'Failed to load playoff bracket');
        }
      } catch (err) {
        if (!cancelled) {
          setBracketError(err instanceof Error ? err.message : 'Failed to load playoff bracket');
        }
      } finally {
        if (!cancelled) {
          setBracketLoading(false);
        }
      }
    };
    fetchBracket();
    return () => {
      cancelled = true;
    };
  }, [isPlayoffMatch, match.competitionId]);

  // Build the `standingsOverride` content for tournament matches.
  let standingsOverride: React.ReactNode | undefined;
  let titleOverride: string | undefined;

  if (isGroupStageMatch && match.tournamentGroupId) {
    const group = tournament?.groups.find((g) => g.id === match.tournamentGroupId);
    const groupName: string = group?.name ?? t('matchPage.standings.groupFallback', 'Lohko');
    const teamsAdvancingPerGroup: number = tournament?.tournamentRules?.hasPlayoffStage
      ? tournament?.tournamentRules?.teamsAdvancingPerGroup ?? 0
      : 0;

    titleOverride = groupName;
    if (tournamentLoading && !tournament) {
      standingsOverride = (
        <div style={{ padding: '1.5rem', textAlign: 'center', color: '#6b7280' }}>
          {t('leaguePage.summary.loading', 'Loading...')}
        </div>
      );
    } else if (tournamentError) {
      standingsOverride = (
        <div style={{ padding: '1.5rem', textAlign: 'center', color: '#ef4444' }}>{tournamentError}</div>
      );
    } else {
      standingsOverride = (
        <TournamentGroupStandingsTable
          groupId={match.tournamentGroupId}
          groupName={groupName}
          teamsAdvancingPerGroup={teamsAdvancingPerGroup}
          hideHeader
        />
      );
    }
  } else if (isPlayoffMatch) {
    titleOverride = t('matchPage.standings.playoffBracket', 'Pudotuspelikaavio');
    if (bracketLoading && !bracket) {
      standingsOverride = (
        <div style={{ padding: '1.5rem', textAlign: 'center', color: '#6b7280' }}>
          {t('leaguePage.summary.loading', 'Loading...')}
        </div>
      );
    } else if (bracketError) {
      standingsOverride = (
        <div style={{ padding: '1.5rem', textAlign: 'center', color: '#ef4444' }}>{bracketError}</div>
      );
    } else if (bracket && bracket.rounds.length > 0) {
      standingsOverride = <TournamentBracket bracket={bracket} compact linkMode="public" />;
    } else {
      standingsOverride = (
        <div style={{ padding: '1.5rem', textAlign: 'center', color: '#6b7280' }}>
          {t('floorball.tournaments.bracket.empty', 'Pudotuspelikaaviota ei ole vielä luotu.')}
        </div>
      );
    }
  }

  return (
    <div className="match-standings">
      <LeagueStanding
        seasonSummary={seasonStats}
        loading={statsLoading}
        error={statsError}
        standingsOverride={standingsOverride}
        titleOverride={titleOverride}
      />
    </div>
  );
}
