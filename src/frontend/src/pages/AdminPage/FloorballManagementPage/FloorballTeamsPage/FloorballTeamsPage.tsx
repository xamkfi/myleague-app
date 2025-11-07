import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import BackButton from '../../../../components/BackButton/BackButton';
import { floorballTeamService } from '../../../../api/floorball/floorballTeamService';
import type { FloorballTeam, PaginatedApiResponse } from '../../../../types/floorball/floorballTypes';
import TeamsTable from './components/TeamsTable';
import './FloorballTeamsPage.scss';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';

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

  // Fetch teams data
  const fetchTeams = async (page: number = 1, size: number = pageSize) => {
    try {
      setLoading(true);
      setError(null);
      
      const response: PaginatedApiResponse<FloorballTeam> = await floorballTeamService.getAll({
        page,
        pageSize: size
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
  const fetchAllTeams = async () => {
    try {
      setLoading(true);
      setError(null);

      const pageSizeBatch = 100; // Use reasonable page size within backend limit
      let page = 1;
      let allTeams: FloorballTeam[] = [];
      let hasNextPage = true;

      while (hasNextPage) {
        const response: PaginatedApiResponse<FloorballTeam> = await floorballTeamService.getAll({
          page,
          pageSize: pageSizeBatch,
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
    navigate(`/admin/floorball/teams/${teamId}/edit`);
  };

  // Handle delete team
  const handleDelete = async (teamId: string, teamName: string) => {
    if (!window.confirm(t('floorball.teams.confirmDelete', { name: teamName }))) {
      return;
    }

    try {
      await floorballTeamService.delete(teamId);
      // Refresh the teams list
      await fetchTeams(currentPage, pageSize);
      // Show success message (you could add a toast notification here)
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
    navigate('/admin/floorball/teams/new');
  };

  // Do not early-return on loading; instead show loading inside TeamsTable

  return (
    <PageTemplate title={t('floorball.teams.title', 'Manage Teams')}>
      <div className="floorball-teams-container">

        {/* Back button */}
        <BackButton 
          to="/admin/floorball" 
        />
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

        {/* Error message */}
        <ErrorPopup message={error} />

        {/* Teams table */}
        <TeamsTable
          teams={filteredTeams}
          onEdit={handleEdit}
          onDelete={handleDelete}
          loading={loading}
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