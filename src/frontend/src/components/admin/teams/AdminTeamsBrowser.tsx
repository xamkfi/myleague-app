import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../ErrorPopup/ErrorPopup';
import AdminTeamsToolbar from './AdminTeamsToolbar';
import { useAdminTeamListFilters } from './useAdminTeamListFilters';
import {
  ALL_TEAMS_SEASON,
  type AdminSeasonChoice,
  type AdminTeamListPage,
  type AdminTeamListQuery,
  type AdminTeamsTableSlot,
  type NamedOption,
} from './adminTeamListTypes';

interface AdminTeamsBrowserProps<T> {
  title: string;
  sportPath: string;
  loadSeasons: () => Promise<AdminSeasonChoice[]>;
  loadClubs: () => Promise<NamedOption[]>;
  loadHomeDivisions: () => Promise<NamedOption[]>;
  loadTeams: (query: AdminTeamListQuery) => Promise<AdminTeamListPage<T>>;
  onRemoveFromSeason: (seasonId: string, teamId: string) => Promise<void>;
  onDeleteTeam: (teamId: string) => Promise<void>;
  confirmDelete: (name: string) => string;
  confirmBulkDelete: (count: number) => string;
  deleteFailed: string;
  bulkDeleteFailed: string;
  renderTable: (slot: AdminTeamsTableSlot<T>) => ReactNode;
}

function pickDefaultSeason(seasons: AdminSeasonChoice[]): string {
  const active = seasons.find((season) => season.isActive);
  if (active) {
    return active.id;
  }
  const newest = [...seasons].sort((left, right) => right.startDate.localeCompare(left.startDate))[0];
  return newest?.id ?? ALL_TEAMS_SEASON;
}

function AdminTeamsBrowser<T extends { id: string }>({
  title,
  sportPath,
  loadSeasons,
  loadClubs,
  loadHomeDivisions,
  loadTeams,
  onRemoveFromSeason,
  onDeleteTeam,
  confirmDelete,
  confirmBulkDelete,
  deleteFailed,
  bulkDeleteFailed,
  renderTable,
}: AdminTeamsBrowserProps<T>) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const {
    seasonId,
    divisionId,
    clubId,
    categories,
    searchTerm,
    debouncedSearch,
    page,
    pageSize,
    setSeasonId,
    setDivisionId,
    setClubId,
    setCategories,
    setSearchTerm,
    setPage,
    setPageSize,
  } = useAdminTeamListFilters();
  const [seasons, setSeasons] = useState<AdminSeasonChoice[]>([]);
  const [clubs, setClubs] = useState<NamedOption[]>([]);
  const [homeDivisions, setHomeDivisions] = useState<NamedOption[]>([]);
  const [optionsReady, setOptionsReady] = useState(false);
  const [teams, setTeams] = useState<T[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadToken, setReloadToken] = useState(0);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    let cancelled = false;
    const loadOptions = async (): Promise<void> => {
      try {
        const [seasonList, clubList, divisionList] = await Promise.all([
          loadSeasons(),
          loadClubs(),
          loadHomeDivisions(),
        ]);
        if (cancelled) {
          return;
        }
        setSeasons([...seasonList].sort((left, right) => right.startDate.localeCompare(left.startDate)));
        setClubs([...clubList].sort((left, right) => left.name.localeCompare(right.name)));
        setHomeDivisions(divisionList);
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : t('admin.teams.loadFailed'));
        }
      } finally {
        if (!cancelled) {
          setOptionsReady(true);
        }
      }
    };
    void loadOptions();
    return () => {
      cancelled = true;
    };
  }, [loadSeasons, loadClubs, loadHomeDivisions, t]);

  useEffect(() => {
    if (!optionsReady || seasonId !== null) {
      return;
    }
    setSeasonId(pickDefaultSeason(seasons));
  }, [optionsReady, seasonId, setSeasonId, seasons]);

  const selectedSeason = useMemo(
    () => seasons.find((season) => season.id === seasonId) ?? null,
    [seasons, seasonId],
  );
  const seasonMode = seasonId !== null && seasonId !== ALL_TEAMS_SEASON;
  const divisionOptions = useMemo(
    () => (seasonMode ? (selectedSeason?.divisions ?? []) : homeDivisions),
    [seasonMode, selectedSeason, homeDivisions],
  );

  useEffect(() => {
    if (!optionsReady || seasonId === null) {
      return;
    }
    if (divisionId && !divisionOptions.some((division) => division.id === divisionId)) {
      setDivisionId('');
    }
  }, [optionsReady, seasonId, divisionId, setDivisionId, divisionOptions]);

  useEffect(() => {
    if (!optionsReady || seasonId === null) {
      return;
    }
    let cancelled = false;
    const load = async (): Promise<void> => {
      try {
        setLoading(true);
        setError(null);
        const query: AdminTeamListQuery = {
          page: page,
          pageSize: pageSize,
          searchTerm: debouncedSearch || undefined,
          clubId: clubId || undefined,
          categories: categories,
        };
        if (seasonMode && seasonId) {
          query.competitionId = seasonId;
          query.competitionDivisionId = divisionId || undefined;
        } else {
          query.homeDivisionId = divisionId || undefined;
        }
        const teamPage = await loadTeams(query);
        if (cancelled) {
          return;
        }
        setTeams(teamPage.items);
        setTotalCount(teamPage.totalCount);
        setTotalPages(Math.max(1, teamPage.totalPages));
      } catch (err) {
        if (!cancelled) {
          setTeams([]);
          setTotalCount(0);
          setTotalPages(1);
          setError(err instanceof Error ? err.message : t('admin.teams.loadFailed'));
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [
    optionsReady,
    seasonId,
    divisionId,
    clubId,
    categories,
    debouncedSearch,
    page,
    pageSize,
    seasonMode,
    reloadToken,
    loadTeams,
    t,
  ]);

  useEffect(() => {
    setSelectedIds(new Set());
  }, [seasonId, divisionId, clubId, debouncedSearch, categories]);

  const handleDanger = async (teamId: string, teamName: string): Promise<void> => {
    const removing = seasonMode && seasonId;
    const confirmed = window.confirm(
      removing
        ? t('admin.teams.confirmRemoveFromSeason', { name: teamName })
        : confirmDelete(teamName),
    );
    if (!confirmed) {
      return;
    }
    try {
      if (removing && seasonId) {
        await onRemoveFromSeason(seasonId, teamId);
      } else {
        await onDeleteTeam(teamId);
      }
      setSelectedIds((prev) => {
        const updated = new Set(prev);
        updated.delete(teamId);
        return updated;
      });
      setReloadToken((token) => token + 1);
    } catch (err) {
      setError(err instanceof Error ? err.message : (removing ? t('admin.teams.removeFailed') : deleteFailed));
    }
  };

  const handleBulk = async (): Promise<void> => {
    if (selectedIds.size === 0) {
      return;
    }
    const removing = seasonMode && seasonId;
    const confirmed = window.confirm(
      removing
        ? t('admin.teams.confirmBulkRemove', { count: selectedIds.size })
        : confirmBulkDelete(selectedIds.size),
    );
    if (!confirmed) {
      return;
    }
    try {
      for (const id of selectedIds) {
        if (removing && seasonId) {
          await onRemoveFromSeason(seasonId, id);
        } else {
          await onDeleteTeam(id);
        }
      }
      setSelectedIds(new Set());
      setReloadToken((token) => token + 1);
    } catch (err) {
      setError(err instanceof Error ? err.message : (removing ? t('admin.teams.bulkRemoveFailed') : bulkDeleteFailed));
    }
  };

  const bulkActionLabel = seasonMode
    ? t('admin.teams.bulkRemove', { count: selectedIds.size })
    : t('common.bulk.delete', { count: selectedIds.size, defaultValue: 'Delete ({{count}})' });

  return (
    <PageTemplate title={title}>
      <div className="admin-teams-page">
        <AdminTeamsToolbar
          totalCount={totalCount}
          seasonId={seasonId}
          seasons={seasons}
          divisionId={divisionId}
          divisions={divisionOptions}
          clubId={clubId}
          clubs={clubs}
          searchTerm={searchTerm}
          categories={categories}
          onSeasonChange={setSeasonId}
          onDivisionChange={setDivisionId}
          onClubChange={setClubId}
          onSearchChange={setSearchTerm}
          onCategoriesChange={setCategories}
          onCreateTeam={() => navigate(`${sportPath}/teams/new`)}
          onOpenSeason={
            seasonMode && seasonId
              ? () => navigate(`${sportPath}/seasons/${seasonId}/edit`)
              : undefined
          }
        />
        <ErrorPopup message={error} />
        {renderTable({
          teams,
          loading: loading || !optionsReady || seasonId === null,
          viewMode: seasonMode ? 'season' : 'catalog',
          seasonId: seasonMode ? seasonId : null,
          divisionNameByTeamId: selectedSeason?.divisionNameByTeamId ?? new Map(),
          clubNameById: new Map(clubs.map((club) => [club.id, club.name])),
          selectedIds,
          onToggleSelect: (id) => {
            setSelectedIds((prev) => {
              const updated = new Set(prev);
              if (updated.has(id)) {
                updated.delete(id);
              } else {
                updated.add(id);
              }
              return updated;
            });
          },
          onSelectAll: () => setSelectedIds(new Set(teams.map((team) => team.id))),
          onClearSelection: () => setSelectedIds(new Set()),
          onBulkDelete: () => void handleBulk(),
          onEdit: (teamId) => navigate(`${sportPath}/teams/${teamId}/edit`),
          onEditRoster: (teamId) => {
            const query = seasonMode && seasonId
              ? `?competitionId=${encodeURIComponent(seasonId)}`
              : '';
            navigate(`${sportPath}/teams/${teamId}/roster${query}`);
          },
          onDelete: (teamId, teamName) => void handleDanger(teamId, teamName),
          bulkActionLabel,
          pagination: {
            currentPage: page,
            totalPages,
            totalCount,
            pageSize: pageSize,
          },
          onPageChange: setPage,
          onPageSizeChange: setPageSize,
        })}
      </div>
    </PageTemplate>
  );
}

export default AdminTeamsBrowser;
