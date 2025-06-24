import React, { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballSeasonService, type FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';
import { floorballTeamService } from '../../../../api/floorball/floorballTeamService';
import PageTemplate from '../../../../components/PageTemplate/PageTemplate';
import type { 
  FloorballMatchDto, 
  FloorballTeam,
  CreateFloorballMatchRequest,
  FloorballMatchStatus
} from '../../../../types/floorball/floorballTypes';

interface MatchManagementPageProps {}

const MatchManagementPage: React.FC<MatchManagementPageProps> = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  
  // State management
  const [matches, setMatches] = useState<FloorballMatchDto[]>([]);
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
  const [teams, setTeams] = useState<FloorballTeam[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  // Form state
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [selectedSeasonId, setSelectedSeasonId] = useState<string>('');
  const [createForm, setCreateForm] = useState<CreateFloorballMatchRequest>({
    seasonId: '',
    homeTeamId: '',
    awayTeamId: '',
    scheduledDateTime: '',
    venue: ''
  });

  // Fetch all required data
  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      // Fetch seasons, teams, and matches in parallel
      const [seasonsResponse, teamsResponse, matchesResponse] = await Promise.all([
        floorballSeasonService.getAll(),
        floorballTeamService.getAll(),
        floorballMatchService.getAll({ pageSize: 100 })
      ]);

      if (seasonsResponse.success && seasonsResponse.data) {
        setSeasons(seasonsResponse.data);
      }

      if (teamsResponse.success && teamsResponse.data) {
        setTeams(teamsResponse.data);
      }

      if (matchesResponse.success && matchesResponse.data) {
        setMatches(matchesResponse.data);
      }

    } catch (error) {
      console.error('Error fetching data:', error);
      setError(error instanceof Error ? error.message : 'Failed to fetch data');
    } finally {
      setLoading(false);
    }
  }, []);

  // Filter matches by selected season
  const filteredMatches = selectedSeasonId 
    ? matches.filter(match => match.seasonId === selectedSeasonId)
    : matches;

  // Initialize data on component mount
  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // Handle create form submission
  const handleCreateMatch = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!createForm.seasonId || !createForm.homeTeamId || !createForm.awayTeamId || !createForm.scheduledDateTime) {
      setError('Please fill in all required fields');
      return;
    }

    if (createForm.homeTeamId === createForm.awayTeamId) {
      setError('Home team and away team cannot be the same');
      return;
    }

    try {
      setActionLoading('create');
      setError(null);

      const response = await floorballMatchService.create(createForm);
      
      if (response.success && response.data) {
        setMatches(prev => [...prev, response.data!]);
        setShowCreateForm(false);
        setCreateForm({
          seasonId: '',
          homeTeamId: '',
          awayTeamId: '',
          scheduledDateTime: '',
          venue: ''
        });
      }

    } catch (error) {
      console.error('Error creating match:', error);
      setError(error instanceof Error ? error.message : 'Failed to create match');
    } finally {
      setActionLoading(null);
    }
  };

  // Handle match status changes
  const handleStartMatch = async (matchId: string) => {
    try {
      setActionLoading(`start-${matchId}`);
      setError(null);

      const response = await floorballMatchService.start(matchId);
      
      if (response.success && response.data) {
        setMatches(prev => prev.map(match => 
          match.id === matchId ? response.data! : match
        ));
      }

    } catch (error) {
      console.error('Error starting match:', error);
      setError(error instanceof Error ? error.message : 'Failed to start match');
    } finally {
      setActionLoading(null);
    }
  };

  const handleCompleteMatch = async (matchId: string) => {
    try {
      setActionLoading(`complete-${matchId}`);
      setError(null);

      const response = await floorballMatchService.complete(matchId);
      
      if (response.success && response.data) {
        setMatches(prev => prev.map(match => 
          match.id === matchId ? response.data! : match
        ));
      }

    } catch (error) {
      console.error('Error completing match:', error);
      setError(error instanceof Error ? error.message : 'Failed to complete match');
    } finally {
      setActionLoading(null);
    }
  };

  // Format date for display
  const formatDateTime = (dateTime: string) => {
    return new Date(dateTime).toLocaleString();
  };

  // Get status badge styling
  const getStatusBadge = (status: FloorballMatchStatus) => {
    const baseClasses = "px-2 py-1 text-xs font-medium rounded-full";
    
    switch (status) {
      case 'Scheduled':
        return `${baseClasses} bg-blue-100 text-blue-800`;
      case 'InProgress':
        return `${baseClasses} bg-green-100 text-green-800`;
      case 'Completed':
        return `${baseClasses} bg-gray-100 text-gray-800`;
      case 'Cancelled':
        return `${baseClasses} bg-red-100 text-red-800`;
      case 'Postponed':
        return `${baseClasses} bg-yellow-100 text-yellow-800`;
      default:
        return `${baseClasses} bg-gray-100 text-gray-800`;
    }
  };

  // Helper function to format season display name
  const formatSeasonDisplayName = (season: FloorballSeasonDto) => {
    const startYear = new Date(season.startDate).getFullYear();
    const endYear = new Date(season.endDate).getFullYear();
    return `${season.name} (${startYear}-${endYear})`;
  };

  if (loading) {
    return (
      <PageTemplate title={t('floorball.matches.title', 'Manage Matches')}>
        <div className="p-6">
          <div className="animate-pulse">
            <div className="h-8 bg-gray-200 rounded w-1/4 mb-6"></div>
            <div className="space-y-4">
              {[...Array(5)].map((_, i) => (
                <div key={i} className="h-16 bg-gray-200 rounded"></div>
              ))}
            </div>
          </div>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.matches.title', 'Manage Matches')}>
      <div className="match-management-container p-6">
        {/* Header with Back Button and Title */}
        <div className="flex items-center justify-between mb-8">
          {/* Back Button */}
          <button
            onClick={() => navigate('/admin/floorball')}
            className="flex items-center text-gray-600 hover:text-gray-900 transition-colors"
          >
            <svg 
              className="w-5 h-5 mr-2" 
              fill="none" 
              stroke="currentColor" 
              viewBox="0 0 24 24"
            >
              <path 
                strokeLinecap="round" 
                strokeLinejoin="round" 
                strokeWidth={2} 
                d="M15 19l-7-7 7-7" 
              />
            </svg>
            {t('common.back', 'Back to Floorball Management')}
          </button>

          {/* Centered Title and Create Button */}
          <div className="flex-1 flex items-center justify-center">
            <h1 className="text-3xl font-bold text-gray-900 mr-8">
              {t('floorball.matches.title', 'Match Management')}
            </h1>
            <button
              onClick={() => setShowCreateForm(true)}
              className="bg-blue-600 text-white px-6 py-2 rounded-md hover:bg-blue-700 transition-colors font-medium"
            >
              {t('floorball.matches.createNew', 'Create New Match')}
            </button>
          </div>
          
          {/* Placeholder for balance */}
          <div className="w-48"></div>
        </div>

        {/* Error Display */}
        {error && (
          <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md">
            {error}
            <button 
              onClick={() => setError(null)}
              className="float-right text-red-500 hover:text-red-700 ml-4"
            >
              ×
            </button>
          </div>
        )}

        {/* Match Statistics and Season Filter */}
        <div className="flex items-center justify-between mb-6 bg-gray-50 p-4 rounded-lg">
          <div className="flex items-center space-x-6">
            <div className="text-sm text-gray-600">
              <span className="font-medium text-gray-900">{filteredMatches.length}</span> 
              {selectedSeasonId ? ' matches in selected season' : ' total matches'}
            </div>
            <div className="flex space-x-4 text-xs">
              <span className="flex items-center">
                <span className="w-2 h-2 bg-blue-500 rounded-full mr-1"></span>
                {matches.filter(m => m.status === 'Scheduled').length} Scheduled
              </span>
              <span className="flex items-center">
                <span className="w-2 h-2 bg-green-500 rounded-full mr-1"></span>
                {matches.filter(m => m.status === 'InProgress').length} In Progress
              </span>
              <span className="flex items-center">
                <span className="w-2 h-2 bg-gray-500 rounded-full mr-1"></span>
                {matches.filter(m => m.status === 'Completed').length} Completed
              </span>
            </div>
          </div>

          {/* Season Filter */}
          <div className="flex items-center space-x-3">
            <label className="text-sm font-medium text-gray-700">
              Filter by Season:
            </label>
            <select
              value={selectedSeasonId}
              onChange={(e) => setSelectedSeasonId(e.target.value)}
              className="border border-gray-300 rounded-md px-3 py-2 bg-white text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">All Seasons</option>
              {seasons.map(season => (
                <option key={season.id} value={season.id}>
                  {formatSeasonDisplayName(season)}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Create Form Modal */}
        {showCreateForm && (
          <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 w-full max-w-md max-h-[90vh] overflow-y-auto">
              <h2 className="text-lg font-semibold mb-4">Create New Match</h2>
              
              <form onSubmit={handleCreateMatch} className="space-y-4">
                {/* Season Selection */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Season *
                  </label>
                  <select
                    value={createForm.seasonId}
                    onChange={(e) => setCreateForm(prev => ({ ...prev, seasonId: e.target.value }))}
                    className="w-full border border-gray-300 rounded-md px-3 py-2"
                    required
                  >
                    <option value="">Select Season</option>
                    {seasons.map(season => (
                      <option key={season.id} value={season.id}>
                        {formatSeasonDisplayName(season)}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Home Team Selection */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Home Team *
                  </label>
                  <select
                    value={createForm.homeTeamId}
                    onChange={(e) => setCreateForm(prev => ({ ...prev, homeTeamId: e.target.value }))}
                    className="w-full border border-gray-300 rounded-md px-3 py-2"
                    required
                  >
                    <option value="">Select Home Team</option>
                    {teams.map(team => (
                      <option key={team.id} value={team.id}>
                        {team.name}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Away Team Selection */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Away Team *
                  </label>
                  <select
                    value={createForm.awayTeamId}
                    onChange={(e) => setCreateForm(prev => ({ ...prev, awayTeamId: e.target.value }))}
                    className="w-full border border-gray-300 rounded-md px-3 py-2"
                    required
                  >
                    <option value="">Select Away Team</option>
                    {teams.filter(team => team.id !== createForm.homeTeamId).map(team => (
                      <option key={team.id} value={team.id}>
                        {team.name}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Scheduled Date/Time */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Scheduled Date & Time *
                  </label>
                  <input
                    type="datetime-local"
                    value={createForm.scheduledDateTime}
                    onChange={(e) => setCreateForm(prev => ({ ...prev, scheduledDateTime: e.target.value }))}
                    className="w-full border border-gray-300 rounded-md px-3 py-2"
                    required
                  />
                </div>

                {/* Venue */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Venue
                  </label>
                  <input
                    type="text"
                    value={createForm.venue}
                    onChange={(e) => setCreateForm(prev => ({ ...prev, venue: e.target.value }))}
                    className="w-full border border-gray-300 rounded-md px-3 py-2"
                    placeholder="Enter venue name"
                  />
                </div>

                {/* Form Actions */}
                <div className="flex gap-3 pt-4">
                  <button
                    type="submit"
                    disabled={actionLoading === 'create'}
                    className="flex-1 bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 disabled:opacity-50"
                  >
                    {actionLoading === 'create' ? 'Creating...' : 'Create Match'}
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowCreateForm(false)}
                    className="flex-1 bg-gray-300 text-gray-700 py-2 px-4 rounded-md hover:bg-gray-400"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Matches Table */}
        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Match
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Date & Time
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Venue
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Score
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredMatches.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-6 py-8 text-center text-gray-500">
                    <div className="flex flex-col items-center">
                      <svg className="w-12 h-12 text-gray-300 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                      </svg>
                      {selectedSeasonId ? 'No matches found for selected season' : 'No matches found'}
                      <p className="text-sm text-gray-400 mt-1">Create your first match to get started</p>
                    </div>
                  </td>
                </tr>
              ) : (
                filteredMatches.map((match) => (
                  <tr key={match.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">
                        {match.homeTeamName} vs {match.awayTeamName}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {formatDateTime(match.scheduledDateTime)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {match.venue || <span className="text-gray-400 italic">TBD</span>}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {match.status === 'Scheduled' ? (
                        <span className="text-gray-400">-</span>
                      ) : (
                        <span className="font-medium">{match.homeScore} - {match.awayScore}</span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={getStatusBadge(match.status)}>
                        {match.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-3">
                      {match.status === 'Scheduled' && (
                        <button
                          onClick={() => handleStartMatch(match.id)}
                          disabled={actionLoading === `start-${match.id}`}
                          className="text-green-600 hover:text-green-900 disabled:opacity-50 transition-colors"
                        >
                          {actionLoading === `start-${match.id}` ? 'Starting...' : 'Start'}
                        </button>
                      )}
                      {match.status === 'InProgress' && (
                        <button
                          onClick={() => handleCompleteMatch(match.id)}
                          disabled={actionLoading === `complete-${match.id}`}
                          className="text-blue-600 hover:text-blue-900 disabled:opacity-50 transition-colors"
                        >
                          {actionLoading === `complete-${match.id}` ? 'Completing...' : 'Complete'}
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </PageTemplate>
  );
};

export default MatchManagementPage; 