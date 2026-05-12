import { useState, useEffect, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useParams, useSearchParams, Link } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LeagueStanding from '../../components/LeagueStanding/LeagueStanding';
import MatchesList from '../../components/MatchesList/MatchesList';
import TournamentGroupStandingsTable from '../../components/TournamentGroupStandingsTable/TournamentGroupStandingsTable';
import { floorballTournamentService } from '../../api/floorball/floorballTournamentService';
import {
  floorballStatisticsService,
  type FloorballSeasonStatisticsSummaryDto
} from '../../api/floorball/floorballStatistics';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import {
  type FloorballMatchDto,
  FloorballMatchStatus
} from '../../types/floorball/floorballTypes';
import type {
  FloorballTournamentDto,
  FloorballTournamentGroupDto
} from '../../types/floorball/tournamentTypes';
import '../LeaguePage/LeaguePage.scss';
import './TournamentPage.scss';

type TabType = 'summary' | 'groups' | 'statistics' | 'results' | 'fixtures';

const VALID_TABS: TabType[] = ['summary', 'groups', 'statistics', 'results', 'fixtures'];

const STATUS_COLORS: Record<string, string> = {
  Draft: '#6b7280',
  Registration: '#3b82f6',
  GroupStage: '#f59e0b',
  PlayoffStage: '#8b5cf6',
  Completed: '#10b981'
};

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('fi-FI', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
}

function getStatusForTab(tab: TabType): FloorballMatchStatus | undefined {
  if (tab === 'results') return FloorballMatchStatus.Completed;
  return undefined;
}

function getSortOrderForTab(tab: TabType): string {
  return tab === 'results' ? 'desc' : 'asc';
}

function TournamentPage() {
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

  useEffect(() => {
    const tabParam = searchParams.get('tab');
    if (tabParam && VALID_TABS.includes(tabParam as TabType)) {
      setActiveTab(tabParam as TabType);
    }
  }, [searchParams]);

  // Tournament data
  const [tournament, setTournament] = useState<FloorballTournamentDto | null>(null);
  const [tournamentLoading, setTournamentLoading] = useState(true);
  const [tournamentError, setTournamentError] = useState<string | null>(null);

  // Statistics summary (used by Summary and Statistics tabs, and as a teaser on others)
  const [statsSummary, setStatsSummary] = useState<FloorballSeasonStatisticsSummaryDto | null>(null);
  const [statsLoading, setStatsLoading] = useState(false);
  const [statsError, setStatsError] = useState<string | null>(null);

  // Matches list state (Results / Fixtures tabs)
  const [matches, setMatches] = useState<FloorballMatchDto[] | null>(null);
  const [matchesLoading, setMatchesLoading] = useState(false);
  const [matchesError, setMatchesError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const handleTabChange = (tab: TabType) => {
    setActiveTab(tab);
    setSearchParams({ tab });
    setCurrentPage(1);
  };

  // Fetch tournament details
  useEffect(() => {
    if (!id) return;
    let cancelled = false;
    const load = async () => {
      try {
        setTournamentLoading(true);
        setTournamentError(null);
        const response = await floorballTournamentService.getById(id);
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

  // Fetch statistics summary
  useEffect(() => {
    if (!id) return;
    let cancelled = false;
    const load = async () => {
      try {
        setStatsLoading(true);
        setStatsError(null);
        const data = await floorballStatisticsService.getSeasonStatistics(id);
        if (!cancelled) {
          setStatsSummary(data);
        }
      } catch (err) {
        if (!cancelled) {
          // Tournament may have no completed matches yet — surface the error in dependent tabs only
          setStatsError(err instanceof Error ? err.message : 'Failed to load statistics');
          setStatsSummary(null);
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
        const response = await floorballMatchService.getAll({
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

  const sortedGroups = useMemo<FloorballTournamentGroupDto[]>(() => {
    if (!tournament) return [];
    return tournament.groups.slice().sort((a, b) => a.order - b.order);
  }, [tournament]);

  const tabs: { key: TabType; label: string }[] = [
    { key: 'summary', label: t('leaguePage.tabs.summary', 'Summary') },
    { key: 'groups', label: t('tournaments.tabs.groups', 'Groups') },
    { key: 'statistics', label: t('leaguePage.tabs.statistics', 'Statistics') },
    { key: 'results', label: t('leaguePage.tabs.results', 'Results') },
    { key: 'fixtures', label: t('leaguePage.tabs.fixtures', 'Fixtures') }
  ];

  if (tournamentLoading) {
    return (
      <PageTemplate title={t('tournaments.loading', 'Loading...')}>
        <div style={{ textAlign: 'center', padding: '3rem', color: '#6b7280' }}>
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  if (tournamentError || !tournament) {
    return (
      <PageTemplate title={t('tournaments.error', 'Error')}>
        <div style={{ textAlign: 'center', padding: '3rem' }}>
          <p style={{ color: '#ef4444', marginBottom: '1rem' }}>
            {tournamentError ?? t('tournaments.notFound', 'Tournament not found')}
          </p>
          <Link to="/tournaments" style={{ color: '#3b82f6' }}>
            {t('tournaments.backToList', 'Back to tournaments')}
          </Link>
        </div>
      </PageTemplate>
    );
  }

  const renderSummaryTab = () => (
    <div className="tournament-page__content">
      {tournament.contentHtml && (
        <div className="tournament-page__about">
          <h2>{t('tournaments.about', 'About')}</h2>
          <div dangerouslySetInnerHTML={{ __html: tournament.contentHtml }} />
        </div>
      )}

      <div className="tournament-page__about">
        <h2>{t('tournaments.overview', 'Overview')}</h2>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '1rem', fontSize: '0.95rem' }}>
          <span><strong>{tournament.teamCount}</strong> {t('tournaments.teams', 'teams')}</span>
          <span><strong>{tournament.matchCount}</strong> {t('tournaments.matches', 'matches')}</span>
          <span><strong>{tournament.groups.length}</strong> {t('tournaments.groups', 'groups')}</span>
          {tournament.venue && (
            <span>
              <i className="fas fa-map-marker-alt" style={{ marginRight: '0.4rem' }}></i>
              {tournament.venue}
            </span>
          )}
        </div>
      </div>

      {/* Top scorers / goalies preview from full statistics summary */}
      {(statsSummary || statsLoading) && (
        <LeagueStanding seasonSummary={statsSummary} loading={statsLoading} error={null} />
      )}
    </div>
  );

  const renderGroupsTab = () => {
    if (!id) return null;
    if (sortedGroups.length === 0) {
      return (
        <div style={{ padding: '2rem', textAlign: 'center', color: '#6b7280' }}>
          {t('tournaments.noGroups', 'No groups have been created yet.')}
        </div>
      );
    }
    return (
      <div className="tournament-page__content">
        {sortedGroups.map((group) => (
          <GroupSection key={group.id} competitionId={id} group={group} />
        ))}
      </div>
    );
  };

  const renderStatisticsTab = () => (
    <LeagueStanding
      seasonSummary={statsSummary}
      loading={statsLoading}
      error={statsError}
    />
  );

  const renderTabContent = () => {
    switch (activeTab) {
      case 'summary':
        return renderSummaryTab();
      case 'groups':
        return renderGroupsTab();
      case 'statistics':
        return renderStatisticsTab();
      case 'results':
      case 'fixtures':
        return (
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
        );
      default:
        return null;
    }
  };

  return (
    <PageTemplate title={tournament.name}>
      <div className="league-page tournament-page">
        <div className="hero-image-container">
          <div className="hero-image"></div>

          <div className="league-header">
            <div className="header-content">
              <div className="league-branding">
                <div className="league-icon">
                  <div className="trophy-icon">🏆</div>
                </div>
              </div>

              <div className="league-info">
                <h1 className="league-title">{tournament.name}</h1>
                <div className="tournament-page__meta">
                  <span>
                    <i className="fas fa-calendar-alt"></i>
                    {formatDate(tournament.startDate)} – {formatDate(tournament.endDate)}
                  </span>
                  {tournament.venue && (
                    <span>
                      <i className="fas fa-map-marker-alt"></i>
                      {tournament.venue}
                    </span>
                  )}
                  <span
                    className="tournament-page__status-pill"
                    style={{ backgroundColor: STATUS_COLORS[tournament.tournamentStatus] ?? '#6b7280' }}
                  >
                    {tournament.tournamentStatus}
                  </span>
                </div>

                <div className="league-tabs">
                  {tabs.map((tab) => (
                    <button
                      key={tab.key}
                      className={`tab-button ${activeTab === tab.key ? 'active' : ''}`}
                      onClick={() => handleTabChange(tab.key)}
                    >
                      {tab.label}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="league-content">{renderTabContent()}</div>
      </div>
    </PageTemplate>
  );
}

interface GroupSectionProps {
  competitionId: string;
  group: FloorballTournamentGroupDto;
}

function GroupSection({ competitionId, group }: GroupSectionProps) {
  const { t } = useTranslation();

  const [matches, setMatches] = useState<FloorballMatchDto[] | null>(null);
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
        const response = await floorballMatchService.getAll({
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
      <h2 className="tournament-page__group-block-title">
        {t('tournaments.group', 'Group')} {group.name}
      </h2>

      <TournamentGroupStandingsTable groupId={group.id} groupName={group.name} />

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

export default TournamentPage;
