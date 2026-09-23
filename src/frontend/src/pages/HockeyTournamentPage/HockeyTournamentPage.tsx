import { useCallback, useEffect, useMemo, useState } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import CompetitionHero from '../../components/CompetitionHero/CompetitionHero';
import UnderlineTabs from '../../components/UnderlineTabs/UnderlineTabs';
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
import { isHockeyMatchFinished, shouldRefreshHockeyMatches } from '../../types/hockey/hockeyTypes';
import { formatHockeyDate, loadTeamNameMap, uniqueHockeyStandingsByTeamId } from '../../utils/hockeyLookups';
import { useAudience } from '../../context/AudienceContext';
import { useIntervalWhen } from '../../hooks/useIntervalWhen';
import '../FloorballTournamentPage/FloorballTournamentPage.scss';
import '../../components/MatchesList/MatchesList.scss';
import '../../components/LeagueStanding/LeagueStanding.scss';

type HockeyTournamentTab = 'summary' | 'groups' | 'results' | 'fixtures';
type LifecycleStatus = 'upcoming' | 'ongoing' | 'past';

const VALID_TABS: HockeyTournamentTab[] = ['summary', 'groups', 'results', 'fixtures'];

function getLifecycleStatus(tournament: HockeyTournamentDto): LifecycleStatus {
  if (tournament.status === 'Completed' || tournament.isCompleted) {
    return 'past';
  }
  const now = Date.now();
  const start = new Date(tournament.startDate).getTime();
  const end = new Date(tournament.endDate).getTime();
  if (now < start) {
    return 'upcoming';
  }
  if (now > end) {
    return 'past';
  }
  return 'ongoing';
}

function HockeyTournamentPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const { id } = useParams<{ id: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  const tabParam = searchParams.get('tab');
  const tab: HockeyTournamentTab = VALID_TABS.includes(tabParam as HockeyTournamentTab)
    ? (tabParam as HockeyTournamentTab)
    : 'summary';
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

  const sortedGroups = useMemo(
    () => (tournament ? [...tournament.groups].sort((left, right) => left.sortOrder - right.sortOrder) : []),
    [tournament],
  );
  const resultMatches = useMemo(
    () =>
      matches
        .filter((match) => isHockeyMatchFinished(match.status))
        .slice()
        .sort((left, right) => new Date(right.scheduledStartTime).getTime() - new Date(left.scheduledStartTime).getTime()),
    [matches],
  );
  const fixtureMatches = useMemo(
    () =>
      matches
        .filter((match) => !isHockeyMatchFinished(match.status) && match.status !== 'Cancelled')
        .slice()
        .sort((left, right) => new Date(left.scheduledStartTime).getTime() - new Date(right.scheduledStartTime).getTime()),
    [matches],
  );

  const setTab = (next: string): void => {
    if (VALID_TABS.includes(next as HockeyTournamentTab)) {
      setSearchParams({ tab: next });
    }
  };

  const renderMatchList = (items: HockeyMatchDto[]) => {
    if (items.length === 0) {
      return (
        <div className="tournament-page__card tournament-page__empty">
          <h3>{t('hockeyPage.noMatches')}</h3>
        </div>
      );
    }
    return (
      <div className="results-section">
        <div className="matches-grid">
          {items.map((match) => (
            <HockeyMatchRow key={match.id} match={match} teamNames={teamNames} />
          ))}
        </div>
      </div>
    );
  };

  if (!tournament && !error) {
    return (
      <PageTemplate title={t('common.loading')}>
        <div className="tournament-page">
          <div className="tournament-page__notice">{t('common.loading')}</div>
        </div>
      </PageTemplate>
    );
  }

  if (!tournament) {
    return (
      <PageTemplate title={t('hockeyPage.tournaments')}>
        <div className="tournament-page">
          <div className="tournament-page__card tournament-page__empty">
            <p>{error}</p>
          </div>
        </div>
      </PageTemplate>
    );
  }

  const lifecycle = getLifecycleStatus(tournament);
  const lifecycleLabels: Record<LifecycleStatus, string> = {
    upcoming: t('tournaments.statusUpcoming'),
    ongoing: t('tournaments.statusOngoing'),
    past: t('tournaments.statusPast'),
  };
  const description = t('tournaments.cardDefaultDescription');

  const renderSummary = () => (
    <div className="tournament-page__content">
      <div className="tournament-page__card">
        <h2>{t('tournaments.tabSummary')}</h2>
        <div className="tournament-page__overview">
          <div>
            <span className="label">{t('tournaments.teams')}</span>
            <span className="value">{tournament.teams.length}</span>
          </div>
          <div>
            <span className="label">{t('leaguePage.summary.matchesPlayed')}</span>
            <span className="value">{matches.length}</span>
          </div>
          <div>
            <span className="label">{t('tournaments.groups')}</span>
            <span className="value">{tournament.groups.length}</span>
          </div>
          {tournament.venue && (
            <div>
              <span className="label">{t('tournaments.venue')}</span>
              <span className="value">{tournament.venue}</span>
            </div>
          )}
        </div>
      </div>
    </div>
  );

  const renderGroups = () => {
    if (sortedGroups.length === 0) {
      return (
        <div className="tournament-page__card tournament-page__empty">
          <h3>{t('tournaments.tabGroups')}</h3>
          <p>{t('hockey.tournaments.noGroups')}</p>
        </div>
      );
    }
    return (
      <div className="tournament-page__content">
        {sortedGroups.map((group) => {
          const standings = groupStandings.get(group.id) ?? [];
          return (
            <section key={group.id} className="tournament-page__card">
              <h2>{group.name}</h2>
              {standings.length > 0 ? (
                <HockeyStandingsTable standings={standings} teamNames={teamNames} competitionId={tournament.id} />
              ) : (
                <ul>
                  {group.teams.map((member) => {
                    const competitionTeam = tournament.teams.find((item) => item.id === member.competitionTeamId);
                    const label = competitionTeam
                      ? teamNames.get(competitionTeam.teamId) ?? member.competitionTeamId.slice(0, 8)
                      : member.competitionTeamId.slice(0, 8);
                    return <li key={member.id}>{label}</li>;
                  })}
                </ul>
              )}
            </section>
          );
        })}
      </div>
    );
  };

  const renderTab = () => {
    switch (tab) {
      case 'groups':
        return renderGroups();
      case 'results':
        return renderMatchList(resultMatches);
      case 'fixtures':
        return renderMatchList(fixtureMatches);
      default:
        return renderSummary();
    }
  };

  return (
    <PageTemplate title={tournament.name}>
      <div className="tournament-page">
        <CompetitionHero
          title={tournament.name}
          logoUrl={tournament.logoUrl}
          meta={
            <>
              <div className="tournament-page__meta">
                <span>
                  <i className="fas fa-calendar-alt" aria-hidden="true"></i>
                  {formatHockeyDate(tournament.startDate)} – {formatHockeyDate(tournament.endDate)}
                </span>
                {tournament.venue && (
                  <span>
                    <i className="fas fa-map-marker-alt" aria-hidden="true"></i>
                    {tournament.venue}
                  </span>
                )}
                <span className={`tournament-page__status-pill tournament-page__status-pill--${lifecycle}`}>
                  {lifecycleLabels[lifecycle]}
                </span>
              </div>
              {tournament.contentHtml ? (
                <div
                  className="tournament-page__description"
                  dangerouslySetInnerHTML={{ __html: tournament.contentHtml }}
                />
              ) : (
                <p className="tournament-page__description">{description}</p>
              )}
            </>
          }
        />
        <UnderlineTabs
          tabs={[
            { id: 'summary', label: t('leaguePage.tabs.summary') },
            { id: 'groups', label: t('tournaments.tabGroups') },
            { id: 'results', label: t('leaguePage.tabs.results') },
            { id: 'fixtures', label: t('leaguePage.tabs.fixtures') },
          ]}
          activeId={tab}
          onChange={setTab}
          ariaLabel={t('tournaments.tabSummary')}
        />
        <div role="tabpanel" id={`tabpanel-${tab}`} aria-labelledby={`tab-${tab}`}>
          {renderTab()}
        </div>
      </div>
    </PageTemplate>
  );
}

export default HockeyTournamentPage;
