import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { loadAllHockeyMatches } from '../../../../api/hockey/loadAllHockeyMatches';
import { hockeySeasonService } from '../../../../api/hockey/hockeySeasonService';
import { hockeyTournamentService } from '../../../../api/hockey/hockeyTournamentService';
import { hockeyMatchService } from '../../../../api/hockey/hockeyMatchService';
import type { HockeyMatchDto, HockeySeasonDto, HockeyTournamentDto } from '../../../../types/hockey/hockeyTypes';
import { isHockeyMatchFinished, isHockeyMatchLive } from '../../../../types/hockey/hockeyTypes';
import { loadTeamNameMap } from '../../../../utils/hockeyLookups';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import LoadingSpinner from '../../../../components/LoadingSpinner/LoadingSpinner';
import ConfirmationDialog from '../../../../components/ConfirmationDialog/ConfirmationDialog';
import SearchField from '../../../../components/SearchField/SearchField';
import Button from '../../../../components/Button/Button';
import AddIcon from '../../../../assets/basicIcons/add.svg';
import Pagination from '../../../../components/Pagination';
import StatusTabs from './components/StatusTabs/StatusTabs';
import type { MatchTab, StatusCounts } from './components/StatusTabs/StatusTabs';
import StatsBar from './components/StatsBar/StatsBar';
import MatchTable from './components/MatchTable/MatchTable';
import './MatchManagementPage.scss';

const VALID_TABS: MatchTab[] = ['all', 'ongoing', 'scheduled', 'completed', 'cancelled'];

const isValidTab = (value: string | null): value is MatchTab =>
  value !== null && VALID_TABS.includes(value as MatchTab);

export type MatchManagementMode = 'all' | 'season' | 'tournament';

interface HockeyMatchManagementPageProps {
  mode?: MatchManagementMode;
}

function HockeyMatchManagementPage({ mode = 'all' }: HockeyMatchManagementPageProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const urlTab = searchParams.get('tab');
  const activeTab: MatchTab = isValidTab(urlTab) ? urlTab : 'all';
  const urlCompetitionId = searchParams.get('competitionId') ?? '';

  const setActiveTab = (tab: MatchTab): void => {
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev);
      if (tab === 'all') {
        next.delete('tab');
      } else {
        next.set('tab', tab);
      }
      return next;
    }, { replace: true });
    setCurrentPage(1);
  };

  const [matches, setMatches] = useState<HockeyMatchDto[]>([]);
  const [seasons, setSeasons] = useState<HockeySeasonDto[]>([]);
  const [tournaments, setTournaments] = useState<HockeyTournamentDto[]>([]);
  const [teamNames, setTeamNames] = useState<Map<string, string>>(new Map());
  const [initialLoading, setInitialLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedCompetitionId, setSelectedCompetitionId] = useState(urlCompetitionId);
  const [searchQuery, setSearchQuery] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [confirmDialog, setConfirmDialog] = useState<{ type: 'cancel' | 'reactivate'; match: HockeyMatchDto } | null>(null);
  const [dialogLoading, setDialogLoading] = useState(false);

  useEffect(() => {
    if (urlCompetitionId !== selectedCompetitionId) {
      setSelectedCompetitionId(urlCompetitionId);
      setCurrentPage(1);
    }
  }, [urlCompetitionId, selectedCompetitionId]);

  useEffect(() => {
    setSelectedCompetitionId(urlCompetitionId);
    setSearchQuery('');
    setCurrentPage(1);
    setInitialLoading(true);
  }, [mode, urlCompetitionId]);

  useEffect(() => {
    const load = async (): Promise<void> => {
      try {
        setInitialLoading(true);
        const [allMatches, seasonList, tournamentList, names] = await Promise.all([
          loadAllHockeyMatches(),
          hockeySeasonService.getAll(),
          hockeyTournamentService.getAll(),
          loadTeamNameMap(),
        ]);
        setMatches(allMatches);
        setSeasons(seasonList);
        setTournaments(tournamentList);
        setTeamNames(names);
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : t('hockey.matches.errors.loadFailed', 'Failed to fetch data'));
      } finally {
        setInitialLoading(false);
      }
    };
    void load();
  }, [mode, t]);

  const handleCompetitionFilterChange = (value: string): void => {
    setSelectedCompetitionId(value);
    setCurrentPage(1);
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev);
      if (value) {
        next.set('competitionId', value);
      } else {
        next.delete('competitionId');
      }
      return next;
    }, { replace: true });
  };

  const seasonIds = useMemo(() => new Set(seasons.map((item) => item.id)), [seasons]);
  const tournamentIds = useMemo(() => new Set(tournaments.map((item) => item.id)), [tournaments]);
  const competitionNames = useMemo(() => {
    const names = new Map<string, string>();
    for (const season of seasons) {
      names.set(season.id, season.name);
    }
    for (const tournament of tournaments) {
      names.set(tournament.id, tournament.name);
    }
    return names;
  }, [seasons, tournaments]);

  const scoped = useMemo(() => {
    return matches.filter((match) => {
      if (selectedCompetitionId && match.competitionId !== selectedCompetitionId) {
        return false;
      }
      if (!match.competitionId) {
        return mode === 'all';
      }
      if (mode === 'season') {
        return seasonIds.has(match.competitionId);
      }
      if (mode === 'tournament') {
        return tournamentIds.has(match.competitionId);
      }
      return true;
    });
  }, [matches, mode, seasonIds, tournamentIds, selectedCompetitionId]);

  const searched = useMemo(() => {
    const needle = searchQuery.trim().toLowerCase();
    if (!needle) {
      return scoped;
    }
    return scoped.filter((match) => {
      const homeName = match.homeTeamId ? teamNames.get(match.homeTeamId) ?? '' : '';
      const awayName = match.awayTeamId ? teamNames.get(match.awayTeamId) ?? '' : '';
      return `${homeName} ${awayName}`.toLowerCase().includes(needle);
    });
  }, [scoped, searchQuery, teamNames]);

  const filtered = useMemo(() => {
    return searched.filter((match) => {
      if (activeTab === 'ongoing') {
        return isHockeyMatchLive(match.status);
      }
      if (activeTab === 'scheduled') {
        return match.status === 'Scheduled' || match.status === 'Postponed';
      }
      if (activeTab === 'completed') {
        return isHockeyMatchFinished(match.status);
      }
      if (activeTab === 'cancelled') {
        return match.status === 'Cancelled';
      }
      return true;
    });
  }, [searched, activeTab]);

  const statusCounts: StatusCounts = useMemo(() => ({
    total: searched.length,
    inProgress: searched.filter((match) => isHockeyMatchLive(match.status)).length,
    scheduled: searched.filter((match) => match.status === 'Scheduled' || match.status === 'Postponed').length,
    completed: searched.filter((match) => isHockeyMatchFinished(match.status)).length,
    cancelled: searched.filter((match) => match.status === 'Cancelled').length,
  }), [searched]);

  const totalPages = Math.ceil(filtered.length / pageSize) || 1;
  const paged = filtered.slice((currentPage - 1) * pageSize, currentPage * pageSize);

  const managePath = (match: HockeyMatchDto): string => `/admin/hockey/matches/manage/${match.id}`;

  const handleConfirmAction = async (): Promise<void> => {
    if (!confirmDialog) {
      return;
    }
    try {
      setDialogLoading(true);
      const updated = confirmDialog.type === 'cancel'
        ? await hockeyMatchService.setStatus(confirmDialog.match.id, 'Cancelled')
        : await hockeyMatchService.setStatus(confirmDialog.match.id, 'Scheduled');
      setMatches((prev) => prev.map((item) => (item.id === updated.id ? updated : item)));
      setConfirmDialog(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : t('hockey.matches.errors.updateFailed', 'An error occurred'));
    } finally {
      setDialogLoading(false);
    }
  };

  const createPath = mode === 'tournament'
    ? '/admin/hockey/tournaments/matches/create'
    : mode === 'season'
      ? '/admin/hockey/seasons/matches/create'
      : '/admin/hockey/matches/create';

  const pageTitle = mode === 'season'
    ? t('hockey.matches.seasonTitle', 'Season Match Management')
    : mode === 'tournament'
      ? t('hockey.matches.tournamentTitle', 'Tournament Match Management')
      : t('hockey.matches.title', 'Match Management');

  const pageSubtitle = mode === 'season'
    ? t('hockey.matches.seasonSubtitle', 'Manage matches scheduled within league seasons')
    : mode === 'tournament'
      ? t('hockey.matches.tournamentSubtitle', 'Manage matches scheduled within tournaments')
      : t('hockey.matches.subtitle', 'Manage hockey matches, track live games, and organize the season');

  if (initialLoading) {
    return (
      <PageTemplate title={pageTitle}>
        <div className="match-mgmt">
          <LoadingSpinner text={t('hockey.matches.loading', 'Loading matches...')} />
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={pageTitle}>
      <div className="match-mgmt">
        <div className="match-mgmt__header">
          <div>
            <h2 className="match-mgmt__title">{pageTitle}</h2>
            <p className="match-mgmt__subtitle">{pageSubtitle}</p>
          </div>
          <Button iconLeft={AddIcon} rounded="pill" onClick={() => navigate(createPath)}>
            {t('hockey.matches.createNewMatch', 'Create New Match')}
          </Button>
        </div>
        <ErrorPopup message={error} />
        <StatsBar stats={statusCounts} isSeasonFiltered={Boolean(selectedCompetitionId)} />
        <div className="match-mgmt__filters">
          <SearchField
            value={searchQuery}
            onChange={(value) => {
              setSearchQuery(value);
              setCurrentPage(1);
            }}
            placeholder={t('hockey.matches.filters.searchPlaceholder', 'Search for team names...')}
            rounded="pill"
            fullWidth
          />
          <div className="match-mgmt__season-filter">
            <label htmlFor="hockey-competition-filter">
              {mode === 'tournament'
                ? t('hockey.matches.filters.filterByTournament', 'Filter by Tournament:')
                : t('hockey.matches.filters.filterBySeason', 'Filter by Season:')}
            </label>
            <select
              id="hockey-competition-filter"
              value={selectedCompetitionId}
              onChange={(event) => handleCompetitionFilterChange(event.target.value)}
              className="match-mgmt__select"
            >
              <option value="">
                {mode === 'tournament'
                  ? t('hockey.matches.filters.allTournaments', 'All Tournaments')
                  : mode === 'season'
                    ? t('hockey.matches.filters.allSeasons', 'All Seasons')
                    : t('hockey.matches.filters.allCompetitions', 'All Competitions')}
              </option>
              {(mode === 'season' || mode === 'all') && seasons.map((season) => (
                <option key={season.id} value={season.id}>{season.name}</option>
              ))}
              {(mode === 'tournament' || mode === 'all') && tournaments.map((tournament) => (
                <option key={tournament.id} value={tournament.id}>{tournament.name}</option>
              ))}
            </select>
          </div>
        </div>
        <StatusTabs activeTab={activeTab} onTabChange={setActiveTab} counts={statusCounts} />
        <div id="match-table-panel" role="tabpanel" aria-labelledby={`tab-${activeTab}`}>
          <MatchTable
            matches={paged}
            teamNames={teamNames}
            competitionNames={competitionNames}
            loading={false}
            onLiveMatch={(match) => navigate(managePath(match))}
            onEditMatch={(match) => navigate(`/admin/hockey/matches/${match.id}/edit`)}
            onOpenMatch={(match) => navigate(managePath(match))}
            onStartMatch={(match) => navigate(managePath(match))}
            onCancelMatch={(match) => setConfirmDialog({ type: 'cancel', match })}
            onReactivateMatch={(match) => setConfirmDialog({ type: 'reactivate', match })}
          />
        </div>
        {filtered.length > 0 && (
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            totalCount={filtered.length}
            pageSize={pageSize}
            onPageChange={setCurrentPage}
            onPageSizeChange={(newSize) => {
              setPageSize(newSize);
              setCurrentPage(1);
            }}
          />
        )}
        <ConfirmationDialog
          isOpen={confirmDialog !== null}
          icon={confirmDialog?.type === 'cancel' ? '⚠️' : '✅'}
          title={
            confirmDialog?.type === 'cancel'
              ? t('hockey.matches.confirmCancelTitle', 'Cancel Match')
              : t('hockey.matches.confirmReactivateTitle', 'Reactivate Match')
          }
          message={
            confirmDialog?.type === 'cancel'
              ? t('hockey.matches.confirmCancel', 'Are you sure you want to cancel this match? This will mark the match as cancelled.')
              : t('hockey.matches.confirmReactivate', 'Are you sure you want to reactivate this match? This will set the match back to Scheduled status.')
          }
          confirmText={
            confirmDialog?.type === 'cancel'
              ? t('hockey.matches.confirmCancelButton', 'Yes, Cancel Match')
              : t('hockey.matches.confirmReactivateButton', 'Yes, Reactivate Match')
          }
          cancelText={t('common.cancel', 'Cancel')}
          isLoading={dialogLoading}
          onConfirm={() => void handleConfirmAction()}
          onCancel={() => setConfirmDialog(null)}
        />
      </div>
    </PageTemplate>
  );
}

export default HockeyMatchManagementPage;
