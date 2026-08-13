import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { footballTeamService } from '../../../../api/football/footballTeamService';
import type { FootballTeam, PaginatedApiResponse, TeamCategory } from '../../../../types/football/footballTypes';
import TeamsTable from './components/TeamsTable';
import TeamCategoryFilter from '../../../../components/TeamCategoryFilter/TeamCategoryFilter';
import './FootballTeamsPage.scss';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';

const FootballTeamsPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  
  const [teams, setTeams] = useState<FootballTeam[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize, setPageSize] = useState(50);
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState<TeamCategory[]>([]);

  // Selection state
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Fetch teams data
  const fetchTeams = async (
    page: number = 1,
    size: number = pageSize,
    categories: TeamCategory[] = categoryFilter
  ) => {
    try {
      setLoading(true);
      setError(null);
      
      const response: PaginatedApiResponse<FootballTeam> = await footballTeamService.getAll({
        page,
        pageSize: size,
        teamCategories: categories.length > 0 ? categories : undefined
      });
      
      // Ensure we have valid response data
      if (response?.data && Array.isArray(response.data)) {
        setTeams(response.data);
        setCurrentPage(response.pagination?.currentPage || 1);
        setTotalPages(response.pagination?.totalPages || 1);
        setTotalCount(response.pagination?.totalCount || 0);
      } else {
        // Handle case where response structure is unexpected
        setTeams([]);
        setCurrentPage(1);
        setTotalPages(1);
        setTotalCount(0);
        setError('Invalid response format from server');
      }
    } catch (err) {
      // Ensure teams is always an array even on error
      setTeams([]);
      setCurrentPage(1);
      setTotalPages(1);
      setTotalCount(0);
      setError(err instanceof Error ? err.message : 'Failed to fetch teams');
      console.error('Error fetching teams:', err);
    } finally {
      setLoading(false);
    }
  };

  // Fetch all teams (no pagination) - used for search
  const fetchAllTeams = async (categories: TeamCategory[] = categoryFilter) => {
    try {
      setLoading(true);
      setError(null);

      const pageSizeBatch = 100; // Use reasonable page size within backend limit
      let page = 1;
      let allTeams: FootballTeam[] = [];
      let hasNextPage = true;

      while (hasNextPage) {
        const response: PaginatedApiResponse<FootballTeam> = await footballTeamService.getAll({
          page,
          pageSize: pageSizeBatch,
          teamCategories: categories.length > 0 ? categories : undefined,
        });

        if (response?.data && Array.isArray(response.data)) {
          allTeams = allTeams.concat(response.data);
          hasNextPage = response.pagination?.hasNextPage ?? false;
          page += 1;
        } else {
          throw new Error('Invalid response format from server');
        }
      }

      setTeams(allTeams);
      setCurrentPage(1);
      setTotalPages(1);
      setTotalCount(allTeams.length);
    } catch (err) {
      setTeams([]);
      setCurrentPage(1);
      setTotalPages(1);
      setTotalCount(0);
      setError(err instanceof Error ? err.message : 'Failed to fetch all teams');
      console.error('Error fetching all teams:', err);
    } finally {
      setLoading(false);
    }
  };

  // Load teams on component mount  
  useEffect(() => {
    fetchTeams();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Filter teams by search term (client-side)
  const filteredTeams = teams.filter(team =>
    team.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // Handle category filter change (multi-select); refetch with the new filter
  const handleCategoryFilterChange = (categories: string[]) => {
    const typedCategories = categories as TeamCategory[];
    setCategoryFilter(typedCategories);

    if (searchTerm.trim()) {
      fetchAllTeams(typedCategories);
    } else {
      fetchTeams(1, pageSize, typedCategories);
    }
  };

  // Handle search input change
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const term = e.target.value;
    setSearchTerm(term);

    // If searching, load all teams to search across the full dataset
    if (term.trim()) {
      // Debounce not implemented for simplicity; could be added
      fetchAllTeams();
    } else {
      // Reset to paginated data when search is cleared
      fetchTeams(1, pageSize);
    }
  };

  // Handle edit team
  const handleEdit = (teamId: string) => {
    navigate(`/admin/football/teams/${teamId}/edit`);
  };

  // Handle edit roster
  const handleEditRoster = (teamId: string) => {
    navigate(`/admin/football/teams/${teamId}/roster`);
  };

  // Handle delete team
  const handleDelete = async (teamId: string, teamName: string) => {
    if (!window.confirm(t('football.teams.confirmDelete', { name: teamName }))) {
      return;
    }

    try {
      await footballTeamService.delete(teamId);
      // Clear from selection if it was selected
      setSelectedIds(prev => {
        const updated = new Set(prev);
        updated.delete(teamId);
        return updated;
      });
      // Refresh the teams list
      await fetchTeams(currentPage, pageSize);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete team');
      console.error('Error deleting team:', err);
    }
  };

  // Handle page change
  const handlePageChange = (page: number) => {
    fetchTeams(page, pageSize);
  };

  // Handle page size change
  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize);
    setCurrentPage(1); // Reset to first page when changing page size
    fetchTeams(1, newPageSize);
  };

  // Handle create team
  const handleCreateTeam = () => {
    navigate('/admin/football/teams/new');
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
    setSelectedIds(new Set(filteredTeams.map(t => t.id)));
  };

  const clearSelection = () => {
    setSelectedIds(new Set());
  };

  const handleBulkDelete = async () => {
    if (selectedIds.size === 0) return;

    const confirmMessage = t('football.teams.confirmBulkDelete', {
      count: selectedIds.size,
      defaultValue: 'Are you sure you want to delete {{count}} teams?',
    });

    if (!window.confirm(confirmMessage)) return;

    try {
      setError(null);
      for (const id of selectedIds) {
        await footballTeamService.delete(id);
      }
      setSelectedIds(new Set());
      await fetchTeams(currentPage, pageSize);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete teams');
      console.error('Error bulk deleting teams:', err);
    }
  };

  // Do not early-return on loading; instead show loading inside TeamsTable

  return (
    <PageTemplate title={t('football.teams.title', 'Manage Teams')}>
      <div className="football-teams-container">
        <h2 className="football-teams-title">{t('football.teams.title', 'MANAGE TEAMS')}</h2>
        
        {/* Header with actions */}
        <div className="football-teams-header">
          <div className="teams-count">
            <span>{t('football.teams.totalCount', { count: totalCount })}</span>
          </div>
          <div className="teams-actions">
            <button
              className="create-team-button"
              onClick={handleCreateTeam}
            >
              {t('football.teams.createNew', 'Create New Team')}
            </button>
          </div>
        </div>

        {/* Search Bar */}
        <div className="teams-search-bar">
          <input
            type="text"
            value={searchTerm}
            onChange={handleSearchChange}
            placeholder={t('football.teams.searchPlaceholder', 'Search teams by name...') as string}
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
          teams={filteredTeams}
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

export default FootballTeamsPage;
