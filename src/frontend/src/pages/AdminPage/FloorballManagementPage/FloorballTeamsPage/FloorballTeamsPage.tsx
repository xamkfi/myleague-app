import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { floorballTeamService } from '../../../../api/floorball/floorballTeamService';
import type { FloorballTeam, PaginatedApiResponse, TeamCategory } from '../../../../types/floorball/floorballTypes';
import TeamsTable from './components/TeamsTable';
import TeamCategoryFilter from '../../../../components/TeamCategoryFilter/TeamCategoryFilter';
import './FloorballTeamsPage.scss';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { mapDeletionError } from '../../../../utils/mapDeletionError';

const FloorballTeamsPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  
  const [teams, setTeams] = useState<FloorballTeam[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize, setPageSize] = useState(50);
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [categoryFilter, setCategoryFilter] = useState<TeamCategory[]>([]);
  const [reloadToken, setReloadToken] = useState(0);

  // Selection state
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      const nextSearch = searchTerm.trim();
      setDebouncedSearch((previous) => {
        if (previous !== nextSearch) {
          setCurrentPage(1);
        }
        return nextSearch;
      });
    }, 300);

    return () => window.clearTimeout(timeoutId);
  }, [searchTerm]);

  useEffect(() => {
    let cancelled = false;

    const loadTeams = async () => {
      try {
        setLoading(true);
        setError(null);

        const response: PaginatedApiResponse<FloorballTeam> = await floorballTeamService.getAll({
          page: currentPage,
          pageSize,
          teamCategories: categoryFilter.length > 0 ? categoryFilter : undefined,
          searchTerm: debouncedSearch || undefined,
        });

        if (cancelled) {
          return;
        }

        if (response?.data && Array.isArray(response.data)) {
          setTeams(response.data);
          setTotalPages(response.pagination?.totalPages || 1);
          setTotalCount(response.pagination?.totalCount || 0);
        } else {
          setTeams([]);
          setTotalPages(1);
          setTotalCount(0);
          setError('Invalid response format from server');
        }
      } catch (err) {
        if (cancelled) {
          return;
        }
        setTeams([]);
        setTotalPages(1);
        setTotalCount(0);
        setError(err instanceof Error ? err.message : 'Failed to fetch teams');
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    void loadTeams();
    return () => {
      cancelled = true;
    };
  }, [currentPage, pageSize, debouncedSearch, categoryFilter, reloadToken]);

  const handleCategoryFilterChange = (categories: string[]) => {
    setCategoryFilter(categories as TeamCategory[]);
    setCurrentPage(1);
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
  };

  // Handle edit team
  const handleEdit = (teamId: string) => {
    navigate(`/admin/floorball/teams/${teamId}/edit`);
  };

  // Handle edit roster
  const handleEditRoster = (teamId: string) => {
    navigate(`/admin/floorball/teams/${teamId}/roster`);
  };

  // Handle delete team
  const handleDelete = async (teamId: string, teamName: string) => {
    if (!window.confirm(t('floorball.teams.confirmDelete', { name: teamName }))) {
      return;
    }

    try {
      await floorballTeamService.delete(teamId);
      // Clear from selection if it was selected
      setSelectedIds(prev => {
        const updated = new Set(prev);
        updated.delete(teamId);
        return updated;
      });
      setReloadToken((token) => token + 1);
    } catch (err) {
      setError(mapDeletionError(err, t) ?? t('floorball.teams.errors.deleteFailed', 'Failed to delete team'));
      console.error('Error deleting team:', err);
    }
  };

  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize);
    setCurrentPage(1);
  };

  // Handle create team
  const handleCreateTeam = () => {
    navigate('/admin/floorball/teams/new');
  };

  // ── Selection handlers ──
  const toggleSelect = (id: string) => {
    setSelectedIds(prev => {
      const updated = new Set(prev);
      if (updated.has(id)) {
        updated.delete(id);
      } else {
        updated.add(id);
      }
      return updated;
    });
  };

  const selectAll = () => {
    setSelectedIds(new Set(teams.map((team) => team.id)));
  };

  const clearSelection = () => {
    setSelectedIds(new Set());
  };

  const handleBulkDelete = async () => {
    if (selectedIds.size === 0) return;

    const confirmMessage = t('floorball.teams.confirmBulkDelete', {
      count: selectedIds.size,
      defaultValue: 'Are you sure you want to delete {{count}} teams?',
    });

    if (!window.confirm(confirmMessage)) return;

    try {
      setError(null);
      for (const id of selectedIds) {
        await floorballTeamService.delete(id);
      }
      setSelectedIds(new Set());
      setReloadToken((token) => token + 1);
    } catch (err) {
      setError(mapDeletionError(err, t) ?? t('floorball.teams.errors.bulkDeleteFailed', 'Failed to delete teams'));
      console.error('Error bulk deleting teams:', err);
    }
  };

  // Do not early-return on loading; instead show loading inside TeamsTable

  return (
    <PageTemplate title={t('floorball.teams.title', 'Manage Teams')}>
      <div className="floorball-teams-container">
        <h2 className="floorball-teams-title">{t('floorball.teams.title', 'MANAGE TEAMS')}</h2>
        
        {/* Header with actions */}
        <div className="floorball-teams-header">
          <div className="teams-count">
            <span>{t('floorball.teams.totalCount', { count: totalCount })}</span>
          </div>
          <div className="teams-actions">
            <button
              className="create-team-button"
              onClick={handleCreateTeam}
            >
              {t('floorball.teams.createNew', 'Create New Team')}
            </button>
          </div>
        </div>

        {/* Search Bar */}
        <div className="teams-search-bar">
          <input
            type="text"
            value={searchTerm}
            onChange={handleSearchChange}
            placeholder={t('floorball.teams.searchPlaceholder', 'Search teams by name...') as string}
            className="teams-search-input"
          />
        </div>

        {/* Category filter */}
        <div className="teams-category-filter">
          <TeamCategoryFilter selected={categoryFilter} onChange={handleCategoryFilterChange} />
        </div>

        {/* Error message */}
        <ErrorPopup message={error} />

        {/* Teams table */}
        <TeamsTable
          teams={teams}
          onEdit={handleEdit}
          onEditRoster={handleEditRoster}
          onDelete={handleDelete}
          loading={loading}
          selectedIds={selectedIds}
          onToggleSelect={toggleSelect}
          onSelectAll={selectAll}
          onClearSelection={clearSelection}
          onBulkDelete={handleBulkDelete}
          pagination={{
            currentPage,
            totalPages,
            totalCount,
            pageSize
          }}
          onPageChange={handlePageChange}
          onPageSizeChange={handlePageSizeChange}
        />


      </div>
    </PageTemplate>
  );
};

export default FloorballTeamsPage;
