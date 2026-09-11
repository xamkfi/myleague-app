import { useState, useEffect, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useParams, useSearchParams, Link } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import FootballLeagueStanding from '../FootballLeaguePage/components/FootballLeagueStanding';
import MatchesList from '../FootballLeaguePage/components/FootballMatchesList';
import PlannedPlayoffSchedule from './components/FootballPlannedPlayoffSchedule';
import TournamentGroupStandingsTable from './components/FootballTournamentGroupStandingsTable';
import TournamentBracket from './components/FootballTournamentBracket';
import { footballTournamentService } from '../../api/football/footballTournamentService';
import {
  footballStatisticsService,
  type FootballSeasonStatisticsSummaryDto
} from '../../api/football/footballStatistics';
import { footballMatchService } from '../../api/football/footballMatchService';
import {
  type FootballMatchDto,
  FootballMatchStatus
} from '../../types/football/footballTypes';
import type {
  FootballTournamentDto,
  FootballTournamentGroupDto,
  FootballPlayoffBracketDto
} from '../../types/football/tournamentTypes';
import {
  formatTournamentGroupLabel,
  formatTournamentGroupTabLabel
} from '../../utils/tournamentGroupLabel';
import './FootballTournamentPage.scss';

type TabType = 'summary' | 'groups' | 'playoffs' | 'statistics' | 'results' | 'fixtures';

const VALID_TABS: TabType[] = ['summary', 'groups', 'playoffs', 'statistics', 'results', 'fixtures'];

type LifecycleStatus = 'upcoming' | 'ongoing' | 'past';

const NOT_FOUND_PATTERNS = [
  'was not found',
  'season statistics with key',
  'no statistics found'
];

function isNotFoundError(message: string | null | undefined): boolean {
  if (!message) return false;
  const lower = message.toLowerCase();
  return NOT_FOUND_PATTERNS.some((pattern) => lower.includes(pattern));
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('fi-FI', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
}

function getLifecycleStatus(tournament: FootballTournamentDto): LifecycleStatus {
  if (tournament.tournamentStatus === 'Completed' || tournament.isCompleted) {
    return 'past';
  }
  const now = Date.now();
  const start = new Date(tournament.startDate).getTime();
  const end = new Date(tournament.endDate).getTime();
  if (now < start) return 'upcoming';
  if (now > end) return 'past';
  return 'ongoing';
}

function getStatusForTab(tab: TabType): FootballMatchStatus | undefined {
  if (tab === 'results') return FootballMatchStatus.Completed;
  return undefined;
}

function getSortOrderForTab(tab: TabType): string {
  return tab === 'results' ? 'desc' : 'asc';
}

function FootballTournamentPage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const [searchParams, setSearchParams] = useSearchParams();

  const getInitialTab = (): TabType => {
    const tabParam = searchParams.get('tab');
    if (tabParam && VALID_TABS.includes(tabParam as TabType)) {
      return tabParam as TabType;
    }
    return 'summary';
  };

  const [activeTab, setActiveTab] = useState<TabType>(getInitialTab);
  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(
    () => searchParams.get('group')
  );

  useEffect(() => {
    const tabParam = searchParams.get('tab');
    if (tabParam && VALID_TABS.includes(tabParam as TabType)) {
      setActiveTab(tabParam as TabType);
    }
    const groupParam = searchParams.get('group');
    setSelectedGroupId(groupParam);
  }, [searchParams]);

  const [tournament, setTournament] = useState<FootballTournamentDto | null>(null);
  const [tournamentLoading, setTournamentLoading] = useState(true);
  const [tournamentError, setTournamentError] = useState<string | null>(null);

  const [statsSummary, setStatsSummary] = useState<FootballSeasonStatisticsSummaryDto | null>(null);
  const [statsLoading, setStatsLoading] = useState(false);
  const [statsError, setStatsError] = useState<string | null>(null);

  const [matches, setMatches] = useState<FootballMatchDto[] | null>(null);
  const [matchesLoading, setMatchesLoading] = useState(false);
  const [matchesError, setMatchesError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const [bracket, setBracket] = useState<FootballPlayoffBracketDto | null>(null);
  const [bracketLoading, setBracketLoading] = useState(false);
  const [bracketError, setBracketError] = useState<string | null>(null);

  const handleTabChange = (tab: TabType) => {
    setActiveTab(tab);
    const next: Record<string, string> = { tab };
    if (tab === 'groups' && selectedGroupId) {
      next.group = selectedGroupId;
    }
    setSearchParams(next);
    setCurrentPage(1);
  };

  const handleGroupChange = (groupId: string) => {
    setSelectedGroupId(groupId);
    setSearchParams({ tab: 'groups', group: groupId });
  };

  // Fetch tournament details
  useEffect(() => {
    if (!id) return;
    let cancelled = false;
    const load = async () => {
      try {
        setTournamentLoading(true);
        setTournamentError(null);
        const response = await footballTournamentService.getById(id);
        if (!cancelled) {
          setTournament(response.data);
        }
      } catch (err) {
        if (!cancelled) {
          setTournamentError(err instanceof Error ? err.message : 'Failed to load tournament');
        }
      } finally {
        if (!cancelled) {
          setTournamentLoading(false);
        }
      }
    };
    load();
    return () => {
      cancelled = true;
    };
  }, [id]);

  // Fetch statistics summary; treat "not found" as "no stats yet" (empty), not as an error.
  useEffect(() => {
    if (!id) return;
    let cancelled = false;
    const load = async () => {
      try {
        setStatsLoading(true);
        setStatsError(null);
        const data = await footballStatisticsService.getSeasonStatistics(id);
        if (!cancelled) {
          setStatsSummary(data);
        }
      } catch (err) {
        if (!cancelled) {
          const message = err instanceof Error ? err.message : 'Failed to load statistics';
          if (isNotFoundError(message)) {
            // Tournament has no completed matches yet — that's an expected empty state, not an error.
            setStatsSummary(null);
            setStatsError(null);
          } else {
            setStatsError(message);
            setStatsSummary(null);
          }
        }
      } finally {
        if (!cancelled) {
          setStatsLoading(false);
        }
      }
    };
    load();
    return () => {
      cancelled = true;
    };
  }, [id]);

  // Fetch matches for results / fixtures tabs
  useEffect(() => {
    if (!id) return;
    if (activeTab !== 'results' && activeTab !== 'fixtures') return;

    let cancelled = false;
    const load = async () => {
      try {
        setMatchesLoading(true);
        setMatchesError(null);
        const pageSize = activeTab === 'fixtures' ? 20 : 10;
        const response = await footballMatchService.getAll({
          competitionId: id,
          page: currentPage,
          pageSize,
          sortOrder: getSortOrderForTab(activeTab),
          status: getStatusForTab(activeTab)
        });
        if (!cancelled) {
          setMatches(response.data || []);
          setTotalPages(response.pagination.totalPages || 1);
        }
      } catch (err) {
        if (!cancelled) {
          setMatchesError(err instanceof Error ? err.message : 'Failed to load matches');
        }
      } finally {
        if (!cancelled) {
          setMatchesLoading(false);
        }
      }
    };
    load();
    return () => {
      cancelled = true;
    };
  }, [id, activeTab, currentPage]);

  const handlePageChange = useCallback((page: number) => {
    setCurrentPage(page);
  }, []);

  const showPlayoffsTab = useMemo<boolean>(() => {
    if (!tournament) return false;
    if (!tournament.tournamentRules?.hasPlayoffStage) return false;
    return (
      tournament.tournamentStatus === 'PlayoffStage' ||
      tournament.tournamentStatus === 'Completed'
    );
  }, [tournament]);

  // Fetch playoff bracket when the tab is active and the tournament is in PlayoffStage / Completed.
  useEffect(() => {
    if (!id) return;
    if (activeTab !== 'playoffs') return;
    if (!showPlayoffsTab) return;
    let cancelled = false;
    const load = async () => {
      try {
        setBracketLoading(true);
        setBracketError(null);
        const response = await footballTournamentService.getPlayoffBracket(id);
        if (!cancelled) {
          setBracket(response.data);
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
    load();
    return () => {
      cancelled = true;
    };
  }, [id, activeTab, showPlayoffsTab]);

  const sortedGroups = useMemo<FootballTournamentGroupDto[]>(() => {
    if (!tournament) return [];
    return tournament.groups.slice().sort((a, b) => a.order - b.order);
  }, [tournament]);

  const activeGroup = useMemo<FootballTournamentGroupDto | null>(() => {
    if (sortedGroups.length === 0) return null;
    if (selectedGroupId) {
      const match = sortedGroups.find((g) => g.id === selectedGroupId);
      if (match) return match;
    }
    return sortedGroups[0];
  }, [sortedGroups, selectedGroupId]);

  useEffect(() => {
    if (activeTab !== 'groups') return;
    if (sortedGroups.length === 0) return;
    const exists = selectedGroupId && sortedGroups.some((g) => g.id === selectedGroupId);
    if (!exists) {
      const fallbackId = sortedGroups[0].id;
      setSelectedGroupId(fallbackId);
      setSearchParams({ tab: 'groups', group: fallbackId });
    }
  }, [activeTab, sortedGroups, selectedGroupId, setSearchParams]);

  const tabs: { key: TabType; label: string }[] = [
    { key: 'summary', label: t('leaguePage.tabs.summary', 'Yhteenveto') },
    { key: 'groups', label: t('tournaments.tabs.groups', 'Lohkot') },
    ...(showPlayoffsTab
      ? [{ key: 'playoffs' as TabType, label: t('tournaments.tabs.playoffs', 'Pudotuspelit') }]
      : []),
    { key: 'statistics', label: t('leaguePage.tabs.statistics', 'Tilastot') },
    { key: 'results', label: t('leaguePage.tabs.results', 'Tulokset') },
    { key: 'fixtures', label: t('leaguePage.tabs.fixtures', 'Otteluohjelma') }
  ];

  if (tournamentLoading) {
    return (
      <PageTemplate title={t('tournaments.loading', 'Ladataan...')}>
        <div className="tournament-page">
          <div className="tournament-page__notice">{t('common.loading', 'Ladataan...')}</div>
        </div>
      </PageTemplate>
    );
  }

  if (tournamentError || !tournament) {
    return (
      <PageTemplate title={t('tournaments.error', 'Virhe')}>
        <div className="tournament-page">
          <div className="tournament-page__card tournament-page__notice">
            <p style={{ color: '#ef4444', marginBottom: '0.5rem' }}>
              {tournamentError ?? t('tournaments.notFound', 'Turnausta ei löytynyt')}
            </p>
            <Link to="/football/tournaments">
              {t('tournaments.backToList', 'Takaisin turnauslistaan')}
            </Link>
          </div>
        </div>
      </PageTemplate>
    );
  }

  const lifecycle = getLifecycleStatus(tournament);
  const lifecycleLabels: Record<LifecycleStatus, string> = {
    upcoming: t('tournaments.status.upcoming', 'Tulossa'),
    ongoing: t('tournaments.status.ongoing', 'Käynnissä'),
    past: t('tournaments.status.past', 'Päättynyt')
  };
  const description =
    tournament.contentHtml ||
    t(
      'tournaments.defaultDescription',
      'Selaa turnauksen lohkoja, tuloksia, tulevia otteluita ja tilastoja.'
    );

  const renderSummaryTab = () => (
    <div className="tournament-page__content">
      <div className="tournament-page__card">
        <h2>{t('tournaments.overview', 'Yleiskatsaus')}</h2>
        <div className="tournament-page__overview">
          <div>
            <span className="label">{t('tournaments.teams', 'Joukkueet')}</span>
            <span className="value">{tournament.teamCount}</span>
          </div>
          <div>
            <span className="label">{t('tournaments.matches', 'Ottelut')}</span>
            <span className="value">{tournament.matchCount}</span>
          </div>
          <div>
            <span className="label">{t('tournaments.groups', 'Lohkot')}</span>
            <span className="value">{tournament.groups.length}</span>
          </div>
          {tournament.venue && (
            <div>
              <span className="label">{t('tournaments.venue', 'Paikka')}</span>
              <span className="value" style={{ fontSize: '1rem' }}>{tournament.venue}</span>
            </div>
          )}
        </div>
      </div>

      {(statsSummary || statsLoading) && (
        <FootballLeagueStanding seasonSummary={statsSummary} loading={statsLoading} error={null} />
      )}

      {!statsLoading && !statsSummary && (
        <div className="tournament-page__card tournament-page__empty">
          <h3>{t('tournaments.statsEmptyTitle', 'Tilastoja ei vielä ole')}</h3>
          <p>
            {t(
              'tournaments.statsEmptyDescription',
              'Tilastot päivittyvät automaattisesti, kun turnauksen ensimmäiset ottelut on pelattu.'
            )}
          </p>
        </div>
      )}
    </div>
  );

  const renderGroupsTab = () => {
    if (!id) return null;
    if (sortedGroups.length === 0) {
      return (
        <div className="tournament-page__card tournament-page__empty">
          <h3>{t('tournaments.noGroupsTitle', 'Lohkoja ei ole vielä luotu')}</h3>
          <p>{t('tournaments.noGroups', 'Lohkot näkyvät tässä, kun turnauksen järjestäjä on lisännyt ne.')}</p>
        </div>
      );
    }
    const teamsAdvancingPerGroup = tournament.tournamentRules?.hasPlayoffStage
      ? tournament.tournamentRules?.teamsAdvancingPerGroup ?? 0
      : 0;
    const currentGroup = activeGroup ?? sortedGroups[0];
    const groupWord = t('tournaments.group', 'Lohko');
    const hasMultipleGroups = sortedGroups.length > 1;
    return (
      <div className="tournament-page__content">
        {hasMultipleGroups && (
          <div className="tournament-page__group-nav">
            <span className="tournament-page__group-nav-label" id="tournament-group-nav-label">
              {t('tournaments.selectGroup', 'Valitse lohko')}
            </span>
            <nav
              className="tournament-page__subtabs"
              aria-labelledby="tournament-group-nav-label"
            >
              {sortedGroups.map((group) => {
                const isActive = currentGroup.id === group.id;
                const tabLabel = formatTournamentGroupTabLabel(group.name, groupWord);
                const fullLabel = formatTournamentGroupLabel(group.name, groupWord);
                return (
                  <button
                    key={group.id}
                    type="button"
                    className={`tournament-page__subtab ${
                      isActive ? 'tournament-page__subtab--active' : ''
                    }`}
                    onClick={() => handleGroupChange(group.id)}
                    aria-pressed={isActive}
                    aria-label={fullLabel}
                    title={fullLabel}
                  >
                    <span className="tournament-page__subtab-primary">{tabLabel}</span>
                    {tabLabel !== fullLabel && (
                      <span className="tournament-page__subtab-secondary">{fullLabel}</span>
                    )}
                  </button>
                );
              })}
            </nav>
          </div>
        )}
        <GroupSection
          key={currentGroup.id}
          competitionId={id}
          group={currentGroup}
          groupDisplayName={formatTournamentGroupLabel(currentGroup.name, groupWord)}
          teamsAdvancingPerGroup={teamsAdvancingPerGroup}
          showTitle={!hasMultipleGroups}
        />
      </div>
    );
  };

  const renderStatisticsTab = () => {
    if (statsError) {
      return (
        <div className="tournament-page__card tournament-page__empty">
          <h3>{t('tournaments.statsErrorTitle', 'Tilastojen lataus epäonnistui')}</h3>
          <p>{statsError}</p>
        </div>
      );
    }

    if (!statsLoading && !statsSummary) {
      return (
        <div className="tournament-page__card tournament-page__empty">
          <h3>{t('tournaments.statsEmptyTitle', 'Tilastoja ei vielä ole')}</h3>
          <p>
            {t(
              'tournaments.statsEmptyDescription',
              'Tilastot päivittyvät automaattisesti, kun turnauksen ensimmäiset ottelut on pelattu.'
            )}
          </p>
        </div>
      );
    }

    return (
      <FootballLeagueStanding
        seasonSummary={statsSummary}
        loading={statsLoading}
        error={null}
      />
    );
  };

  const renderPlayoffsTab = () => {
    if (bracketLoading) {
      return (
        <div className="tournament-page__card tournament-page__notice">
          {t('common.loading', 'Ladataan...')}
        </div>
      );
    }
    if (bracketError) {
      return (
        <div className="tournament-page__card tournament-page__empty">
          <h3>{t('tournaments.playoffs.errorTitle', 'Pudotuspelikaavion lataus epäonnistui')}</h3>
          <p>{bracketError}</p>
        </div>
      );
    }
    if (!bracket || bracket.rounds.length === 0) {
      return (
        <div className="tournament-page__card tournament-page__empty">
          <h3>{t('tournaments.playoffs.notReadyTitle', 'Pudotuspelit eivät ole vielä alkaneet')}</h3>
          <p>
            {t(
              'tournaments.playoffs.notReadyDescription',
              'Pudotuspelikaavio näkyy täällä, kun turnauksen järjestäjä on käynnistänyt pudotuspelivaiheen.'
            )}
          </p>
        </div>
      );
    }
    return (
      <div className="tournament-page__content">
        <TournamentBracket bracket={bracket} />
      </div>
    );
  };

  const renderTabContent = () => {
    if (activeTab === 'playoffs' && !showPlayoffsTab) {
      return renderSummaryTab();
    }
    switch (activeTab) {
      case 'summary':
        return renderSummaryTab();
      case 'groups':
        return renderGroupsTab();
      case 'playoffs':
        return renderPlayoffsTab();
      case 'statistics':
        return renderStatisticsTab();
      case 'results':
      case 'fixtures':
        return (
          <>
            <MatchesList
              variant={activeTab}
              matchesLoading={matchesLoading}
              matchesError={matchesError}
              matches={matches}
              currentPage={currentPage}
              totalPages={totalPages}
              handlePageChange={handlePageChange}
              groupingMode="none"
            />
            {activeTab === 'fixtures' && (
              <PlannedPlayoffSchedule
                tournament={tournament}
                collapsible
                afterMatchList
              />
            )}
          </>
        );
      default:
        return null;
    }
  };

  return (
    <PageTemplate title={tournament.name}>
      <div className="tournament-page">
        <header className="tournament-page__hero">
          <div className="tournament-page__hero-row">
            <div className="tournament-page__icon" aria-hidden="true">🏆</div>
            <div className="tournament-page__heading">
              <h1 className="tournament-page__title">{tournament.name}</h1>
              <div className="tournament-page__meta">
                <span>
                  <i className="fas fa-calendar-alt" aria-hidden="true"></i>
                  {formatDate(tournament.startDate)} – {formatDate(tournament.endDate)}
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
            </div>
          </div>
        </header>

        <nav className="tournament-page__tabs" aria-label={t('tournaments.tabsAria', 'Turnauksen välilehdet')}>
          {tabs.map((tab) => (
            <button
              key={tab.key}
              type="button"
              className={`tournament-page__tab ${activeTab === tab.key ? 'tournament-page__tab--active' : ''}`}
              onClick={() => handleTabChange(tab.key)}
            >
              {tab.label}
            </button>
          ))}
        </nav>

        {renderTabContent()}
      </div>
    </PageTemplate>
  );
}

interface GroupSectionProps {
  competitionId: string;
  group: FootballTournamentGroupDto;
  groupDisplayName: string;
  teamsAdvancingPerGroup: number;
  showTitle: boolean;
}

function GroupSection({
  competitionId,
  group,
  groupDisplayName,
  teamsAdvancingPerGroup,
  showTitle
}: GroupSectionProps) {
  const [matches, setMatches] = useState<FootballMatchDto[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await footballMatchService.getAll({
          competitionId,
          tournamentGroupId: group.id,
          page,
          pageSize: 20,
          sortOrder: 'asc'
        });
        if (!cancelled) {
          setMatches(response.data || []);
          setTotalPages(response.pagination.totalPages || 1);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Failed to load matches');
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };
    load();
    return () => {
      cancelled = true;
    };
  }, [competitionId, group.id, page]);

  const handlePageChange = useCallback((next: number) => {
    setPage(next);
  }, []);

  return (
    <div className="tournament-page__group-block">
      {showTitle && (
        <h2 className="tournament-page__group-block-title">{groupDisplayName}</h2>
      )}

      <TournamentGroupStandingsTable
        groupId={group.id}
        groupName={groupDisplayName}
        teamsAdvancingPerGroup={teamsAdvancingPerGroup}
      />

      <MatchesList
        variant="fixtures"
        matchesLoading={loading}
        matchesError={error}
        matches={matches}
        currentPage={page}
        totalPages={totalPages}
        handlePageChange={handlePageChange}
        groupingMode="none"
      />
    </div>
  );
}

export default FootballTournamentPage;
