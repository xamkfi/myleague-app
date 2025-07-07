import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/PageTemplate';
import { floorballTeamService } from '../../../../api/floorball/floorballTeamService';
import { getClubs, type Club } from '../../../../api/common/clubService';
import type { FloorballTeam, PaginatedApiResponse, FloorballTeamRequest } from '../../../../types/floorball/floorballTypes';
import TeamsTable from './components/TeamsTable';
import PaginationControls from './components/PaginationControls';
import CreateTeamModal from './components/CreateTeamModal';
import './FloorballTeamsPage.scss';
import './components/CreateTeamModal.scss';
import './components/EditTeamModal.scss';

const FloorballTeamsPage = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();
  
  const [teams, setTeams] = useState<FloorballTeam[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [clubs, setClubs] = useState<Club[]>([]);

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

  // Fetch clubs for the modal
  const fetchClubs = async () => {
    try {
      const clubsData = await getClubs();
      setClubs(clubsData);
    } catch (err) {
      console.error('Error fetching clubs:', err);
    }
  };

  // Load teams and clubs on component mount  
  useEffect(() => {
    fetchTeams();
    fetchClubs();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Handle edit team
  const handleEdit = async (teamData: FloorballTeamRequest, teamId: string) => {
    try {
      console.log('Handling edit for team ID:', teamId);
      console.log('Team data to update:', teamData);
      
      await floorballTeamService.update(teamId, teamData);
      
      // Clear any previous errors
      setError(null);
      
      // Refresh the teams list
      await fetchTeams(currentPage, pageSize);
      
      console.log('Team updated successfully');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to update team';
      setError(errorMessage);
      console.error('Error updating team:', err);
      throw err; // Re-throw to let modal handle the error
    }
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
  const handleCreateTeam = async (teamData: FloorballTeamRequest) => {
    try {
      await floorballTeamService.create(teamData);
      // Refresh the teams list
      await fetchTeams(currentPage, pageSize);
      setIsCreateModalOpen(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create team');
      console.error('Error creating team:', err);
      throw err; // Re-throw to let modal handle the error
    }
  };

  if (loading) {
    return (
      <PageTemplate title={t('floorball.teams.title', 'Manage Teams')}>
        <div className="floorball-teams-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.teams.title', 'Manage Teams')}>
      <div className="floorball-teams-container">
        {/* Header with actions */}
        <div className="floorball-teams-header">
          <div className="teams-count">
            <span>{t('floorball.teams.totalCount', { count: totalCount })}</span>
          </div>
          <div className="teams-actions">
            <button
              className="create-team-button"
              onClick={() => setIsCreateModalOpen(true)}
            >
              {t('floorball.teams.createNew', 'Create New Team')}
            </button>
          </div>
        </div>

        {/* Error message */}
        {error && (
          <div className="error-message">
            <p>{error}</p>
          </div>
        )}

        {/* Pagination */}
        <PaginationControls
          currentPage={currentPage}
          totalPages={totalPages}
          totalCount={totalCount}
          pageSize={pageSize}
          onPageChange={handlePageChange}
          onPageSizeChange={handlePageSizeChange}
        />


        {/* Teams table */}
        <TeamsTable
          teams={teams}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />

        {/* Pagination */}
        <PaginationControls
          currentPage={currentPage}
          totalPages={totalPages}
          totalCount={totalCount}
          pageSize={pageSize}
          onPageChange={handlePageChange}
          onPageSizeChange={handlePageSizeChange}
        />

        
        {/* Back button */}
        <div className="back-button-container">
          <button
            className="back-button"
            onClick={() => navigate('/admin/floorball')}
          >
            {t('common.back', 'Back to Floorball Management')}
          </button>
        </div>

        {/* Create Team Modal */}
        <CreateTeamModal
          isOpen={isCreateModalOpen}
          onClose={() => setIsCreateModalOpen(false)}
          onSubmit={handleCreateTeam}
          clubs={clubs}
        />
      </div>
    </PageTemplate>
  );
};

export default FloorballTeamsPage; 