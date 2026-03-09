import { useState, useEffect, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams } from 'react-router-dom';
import ReactQuill from 'react-quill';
import 'react-quill/dist/quill.snow.css';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';
import Pagination from '../../../../../components/Pagination';
import { floorballTournamentService } from '../../../../../api/floorball/floorballTournamentService';
import { floorballTeamService } from '../../../../../api/floorball/floorballTeamService';
import type {
  FloorballTournamentDto,
  UpdateFloorballTournamentRequest,
  FloorballTournamentGroupDto,
  FloorballTeam,
} from '../../../../../types/floorball/floorballTypes';
import '../../FloorballSeasonsPage/EditSeasonPage/EditSeasonPage.scss';

const QUILL_MODULES = {
  toolbar: [
    [{ header: [1, 2, 3, false] }],
    ['bold', 'italic', 'underline', 'strike'],
    [{ list: 'ordered' }, { list: 'bullet' }],
    ['link', 'image'],
    ['clean'],
  ],
};

const EditTournamentPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { tournamentId } = useParams<{ tournamentId: string }>();

  const [tournament, setTournament] = useState<FloorballTournamentDto | null>(null);
  const [loadingTournament, setLoadingTournament] = useState(true);
  const [activeTab, setActiveTab] = useState<'details' | 'groups'>('details');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const [formData, setFormData] = useState<UpdateFloorballTournamentRequest>({
    name: '',
    startDate: '',
    endDate: '',
    location: '',
    descriptionHtml: '',
    numberOfPeriods: 2,
    periodDurationMinutes: 15,
    allowOvertime: true,
    overtimeDurationMinutes: 5,
    allowShootout: true,
    playoffFormat: 'None',
    groupStageAdvancingCount: 1,
  });

  // Group management
  const [newGroupName, setNewGroupName] = useState('');
  const [newGroupPhase, setNewGroupPhase] = useState('GroupStage');
  const [addingGroup, setAddingGroup] = useState(false);
  const [showAddGroupModal, setShowAddGroupModal] = useState(false);

  // Team management
  const [availableTeams, setAvailableTeams] = useState<FloorballTeam[]>([]);
  const [selectedGroupForTeam, setSelectedGroupForTeam] = useState<string | null>(null);
  const [teamOperationLoading, setTeamOperationLoading] = useState(false);

  // Search + multi-select
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  const [selectedTeamIds, setSelectedTeamIds] = useState<Set<string>>(new Set());
  const [loadingTeams, setLoadingTeams] = useState(false);
  const [teamsPagination, setTeamsPagination] = useState({
    currentPage: 1, pageSize: 10, totalCount: 0, totalPages: 0,
  });

  const loadTournament = useCallback(async () => {
    if (!tournamentId) return;
    try {
      setLoadingTournament(true);
      const response = await floorballTournamentService.getById(tournamentId);
      setTournament(response.data);
      setFormData({
        name: response.data.name,
        startDate: response.data.startDate.split('T')[0],
        endDate: response.data.endDate.split('T')[0],
        location: response.data.location || '',
        descriptionHtml: response.data.descriptionHtml || '',
        numberOfPeriods: response.data.matchRules?.numberOfPeriods ?? 2,
        periodDurationMinutes: response.data.matchRules?.periodDurationMinutes ?? 15,
        allowOvertime: response.data.matchRules?.allowOvertime ?? true,
        overtimeDurationMinutes: response.data.matchRules?.overtimeDurationMinutes ?? 5,
        allowShootout: response.data.matchRules?.allowShootout ?? true,
        playoffFormat: response.data.playoffFormat || 'None',
        groupStageAdvancingCount: response.data.groupStageAdvancingCount ?? 1,
      });
    } catch {
      setError(t('tournament.errors.loadFailed', 'Failed to load tournament'));
    } finally {
      setLoadingTournament(false);
    }
  }, [tournamentId, t]);

  const loadAvailableTeams = useCallback(async () => {
    if (activeTab !== 'groups') return;
    try {
      setLoadingTeams(true);
      const response = await floorballTeamService.getAllWithoutRoster({
        page: teamsPagination.currentPage,
        pageSize: teamsPagination.pageSize,
        searchTerm: debouncedSearchTerm || undefined,
      });
      if (response?.data && Array.isArray(response.data)) {
        setAvailableTeams(response.data);
        if (response.pagination) {
          setTeamsPagination(prev => ({
            ...prev,
            totalCount: response.pagination.totalCount,
            totalPages: response.pagination.totalPages,
          }));
        }
      } else {
        setAvailableTeams([]);
      }
    } catch {
      setAvailableTeams([]);
    } finally {
      setLoadingTeams(false);
    }
  }, [activeTab, teamsPagination.currentPage, teamsPagination.pageSize, debouncedSearchTerm]);

  useEffect(() => {
    loadTournament();
  }, [loadTournament]);

  useEffect(() => {
    loadAvailableTeams();
  }, [loadAvailableTeams]);

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearchTerm(searchTerm), 400);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Reset to page 1 when search changes
  useEffect(() => {
    setTeamsPagination(prev => ({ ...prev, currentPage: 1 }));
  }, [debouncedSearchTerm]);

  useEffect(() => {
    if (tournament?.groups && tournament.groups.length > 0 && !selectedGroupForTeam) {
      setSelectedGroupForTeam(tournament.groups[0].id);
    }
  }, [tournament, selectedGroupForTeam]);

  const showSuccess = (message: string) => {
    setSuccessMessage(message);
    setTimeout(() => setSuccessMessage(null), 2000);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'number' ? parseInt(value, 10) || 0 : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!tournamentId) return;
    setLoading(true);
    setError(null);

    try {
      if (!formData.name.trim()) {
        throw new Error(t('tournament.validation.nameRequired', 'Tournament name is required'));
      }
      await floorballTournamentService.update(tournamentId, formData);
      await loadTournament();
      showSuccess(t('tournament.updated', 'Tournament updated successfully'));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update tournament');
    } finally {
      setLoading(false);
    }
  };

  // Group operations
  const handleAddGroup = async () => {
    if (!tournamentId || !newGroupName.trim()) return;
    setAddingGroup(true);
    setError(null);
    try {
      await floorballTournamentService.addGroup(tournamentId, {
        name: newGroupName.trim(),
        phase: newGroupPhase,
        sortOrder: (tournament?.groups?.length ?? 0),
      });
      setNewGroupName('');
      setNewGroupPhase('GroupStage');
      setShowAddGroupModal(false);
      await loadTournament();
      showSuccess(t('tournament.groupAdded', 'Group added'));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add group');
    } finally {
      setAddingGroup(false);
    }
  };

  const handleRemoveGroup = async (groupId: string) => {
    if (!tournamentId) return;
    if (!window.confirm(t('tournament.confirmDeleteGroup', 'Delete this group?'))) return;
    setError(null);
    try {
      await floorballTournamentService.removeGroup(tournamentId, groupId);
      if (selectedGroupForTeam === groupId) setSelectedGroupForTeam(null);
      await loadTournament();
      showSuccess(t('tournament.groupRemoved', 'Group removed'));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to remove group');
    }
  };

  // Team-in-group operations
  const handleAddSingleTeam = async (teamId: string) => {
    if (!tournamentId || !selectedGroupForTeam) return;
    setTeamOperationLoading(true);
    setError(null);
    try {
      await floorballTournamentService.addTeamToGroup(tournamentId, selectedGroupForTeam, teamId);
      await loadTournament();
      showSuccess(t('tournament.teamAdded', 'Team added to group'));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add team');
    } finally {
      setTeamOperationLoading(false);
    }
  };

  const handleAddSelectedTeams = async () => {
    if (!tournamentId || !selectedGroupForTeam || selectedTeamIds.size === 0) return;
    setTeamOperationLoading(true);
    setError(null);
    let successCount = 0;
    let failCount = 0;
    for (const teamId of selectedTeamIds) {
      try {
        await floorballTournamentService.addTeamToGroup(tournamentId, selectedGroupForTeam, teamId);
        successCount++;
      } catch {
        failCount++;
      }
    }
    setSelectedTeamIds(new Set());
    await loadTournament();
    await loadAvailableTeams();
    if (failCount === 0) {
      showSuccess(t('tournament.teamsAdded', '{{count}} team(s) added', { count: successCount }));
    } else {
      setError(t('tournament.someTeamsFailed', '{{success}} added, {{fail}} failed.', { success: successCount, fail: failCount }));
    }
    setTeamOperationLoading(false);
  };

  const handleRemoveTeamFromGroup = async (groupId: string, teamId: string) => {
    if (!tournamentId) return;
    setTeamOperationLoading(true);
    setError(null);
    try {
      await floorballTournamentService.removeTeamFromGroup(tournamentId, groupId, teamId);
      await loadTournament();
      showSuccess(t('tournament.teamRemoved', 'Team removed from group'));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to remove team');
    } finally {
      setTeamOperationLoading(false);
    }
  };

  const toggleTeamSelection = (teamId: string) => {
    setSelectedTeamIds(prev => {
      const next = new Set(prev);
      if (next.has(teamId)) next.delete(teamId); else next.add(teamId);
      return next;
    });
  };

  const allTeamIdsInTournament = useMemo(
    () => new Set(tournament?.groups?.flatMap((g) => g.teams.map((t) => t.teamId)) ?? []),
    [tournament]
  );

  const availableTeamsNotInTournament = useMemo(
    () => availableTeams.filter((t) => !allTeamIdsInTournament.has(t.id)),
    [availableTeams, allTeamIdsInTournament]
  );

  const selectAllAvailable = () => {
    const ids = availableTeamsNotInTournament.map(t => t.id);
    if (selectedTeamIds.size === ids.length) setSelectedTeamIds(new Set());
    else setSelectedTeamIds(new Set(ids));
  };

  const selectedGroup: FloorballTournamentGroupDto | undefined = tournament?.groups?.find(
    (g) => g.id === selectedGroupForTeam
  );

  if (loadingTournament) {
    return (
      <PageTemplate title={t('tournament.edit', 'Edit Tournament')}>
        <div className="edit-season-loading"><p>{t('common.loading', 'Loading...')}</p></div>
      </PageTemplate>
    );
  }

  if (!tournament) {
    return (
      <PageTemplate title={t('tournament.edit', 'Edit Tournament')}>
        <ErrorPopup message={t('tournament.notFound', 'Tournament not found')} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={`${t('tournament.edit', 'Edit Tournament')}: ${tournament.name}`}>
      {successMessage && <div className="success-toast"><p>{successMessage}</p></div>}

      <div className="edit-season-container">
        <div className="tab-navigation">
          <button className={`tab-button ${activeTab === 'details' ? 'active' : ''}`} onClick={() => setActiveTab('details')}>
            {t('tournament.details', 'Tournament Details')}
          </button>
          <button className={`tab-button ${activeTab === 'groups' ? 'active' : ''}`} onClick={() => setActiveTab('groups')}>
            {t('tournament.manageGroups', 'Groups & Teams')} ({tournament.groups?.length ?? 0})
          </button>
        </div>

        <div className="edit-season-content">
          {/* Details Tab */}
          {activeTab === 'details' && (
            <form onSubmit={handleSubmit} className="edit-season-form">
              <ErrorPopup message={error} />

              <div className="form-section">
                <h3 className="form-section__title">{t('tournament.sections.basicInfo', 'Basic Information')}</h3>
                <div className="form-group">
                  <label htmlFor="name">{t('tournament.fields.name', 'Name')} *</label>
                  <input type="text" id="name" name="name" value={formData.name} onChange={handleInputChange} required disabled={loading} />
                </div>
                <div className="form-group">
                  <label htmlFor="location">{t('tournament.fields.location', 'Location')}</label>
                  <input type="text" id="location" name="location" value={formData.location || ''} onChange={handleInputChange} disabled={loading} />
                </div>
              </div>

              <div className="form-section">
                <h3 className="form-section__title">{t('tournament.sections.schedule', 'Schedule')}</h3>
                <div className="form-row">
                  <div className="form-group">
                    <label htmlFor="startDate">{t('tournament.fields.startDate', 'Start Date')} *</label>
                    <input type="date" id="startDate" name="startDate" value={formData.startDate} onChange={handleInputChange} required disabled={loading} />
                  </div>
                  <div className="form-group">
                    <label htmlFor="endDate">{t('tournament.fields.endDate', 'End Date')} *</label>
                    <input type="date" id="endDate" name="endDate" value={formData.endDate} onChange={handleInputChange} required disabled={loading} min={formData.startDate} />
                  </div>
                </div>
              </div>

              <div className="form-section form-section--description">
                <h3 className="form-section__title">{t('tournament.sections.description', 'Description')}</h3>
                <div className="form-group">
                  <ReactQuill theme="snow" value={formData.descriptionHtml || ''} onChange={(val) => setFormData((prev) => ({ ...prev, descriptionHtml: val }))} modules={QUILL_MODULES} />
                </div>
              </div>

              <div className="form-section">
                <h3 className="form-section__title">{t('tournament.sections.matchRules', 'Match Rules')}</h3>
                <div className="form-row">
                  <div className="form-group">
                    <label htmlFor="numberOfPeriods">{t('tournament.fields.numberOfPeriods', 'Periods')}</label>
                    <select id="numberOfPeriods" name="numberOfPeriods" value={formData.numberOfPeriods} onChange={handleInputChange} disabled={loading}>
                      {[1, 2, 3, 4, 5].map((n) => (<option key={n} value={n}>{n}</option>))}
                    </select>
                  </div>
                  <div className="form-group">
                    <label htmlFor="periodDurationMinutes">{t('tournament.fields.periodDuration', 'Period Duration (min)')}</label>
                    <input type="number" id="periodDurationMinutes" name="periodDurationMinutes" value={formData.periodDurationMinutes} onChange={handleInputChange} min={1} max={60} disabled={loading} />
                  </div>
                </div>
                <div className="toggle-container">
                  <label className="toggle-label">{t('tournament.fields.allowOvertime', 'Allow Overtime')}</label>
                  <button type="button" className={`toggle-switch ${formData.allowOvertime ? 'active' : ''}`} onClick={() => setFormData((prev) => ({ ...prev, allowOvertime: !prev.allowOvertime }))} disabled={loading} aria-pressed={formData.allowOvertime}>
                    <span className="toggle-switch__slider" />
                  </button>
                </div>
                {formData.allowOvertime && (
                  <div className="form-group form-group--indented">
                    <label htmlFor="overtimeDurationMinutes">{t('tournament.fields.overtimeDuration', 'Overtime Duration (min)')}</label>
                    <input type="number" id="overtimeDurationMinutes" name="overtimeDurationMinutes" value={formData.overtimeDurationMinutes} onChange={handleInputChange} min={1} max={30} disabled={loading} />
                  </div>
                )}
                <div className="toggle-container">
                  <label className="toggle-label">{t('tournament.fields.allowShootout', 'Allow Shootout')}</label>
                  <button type="button" className={`toggle-switch ${formData.allowShootout ? 'active' : ''}`} onClick={() => setFormData((prev) => ({ ...prev, allowShootout: !prev.allowShootout }))} disabled={loading} aria-pressed={formData.allowShootout}>
                    <span className="toggle-switch__slider" />
                  </button>
                </div>
              </div>

              <div className="form-section">
                <h3 className="form-section__title">{t('tournament.sections.playoffSettings', 'Playoff Settings')}</h3>
                <div className="form-row">
                  <div className="form-group">
                    <label htmlFor="playoffFormat">{t('tournament.fields.playoffFormat', 'Playoff Format')}</label>
                    <select id="playoffFormat" name="playoffFormat" value={formData.playoffFormat} onChange={handleInputChange} disabled={loading}>
                      <option value="None">{t('tournament.playoffFormat.none', 'None')}</option>
                      <option value="SingleElimination">{t('tournament.playoffFormat.singleElimination', 'Single Elimination')}</option>
                      <option value="FinalGroup">{t('tournament.playoffFormat.finalGroup', 'Final Group')}</option>
                    </select>
                  </div>
                  <div className="form-group">
                    <label htmlFor="groupStageAdvancingCount">{t('tournament.fields.advancingCount', 'Teams Advancing per Group')}</label>
                    <input type="number" id="groupStageAdvancingCount" name="groupStageAdvancingCount" value={formData.groupStageAdvancingCount} onChange={handleInputChange} min={1} max={10} disabled={loading} />
                  </div>
                </div>
              </div>

              <div className="form-actions">
                <button type="button" className="btn btn-secondary" onClick={() => navigate('/admin/floorball/tournaments')} disabled={loading}>
                  {t('common.cancel', 'Cancel')}
                </button>
                <button type="submit" className="btn btn-primary" disabled={loading}>
                  {loading ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
                </button>
              </div>
            </form>
          )}

          {/* Groups & Teams Tab */}
          {activeTab === 'groups' && (
            <div className="divisions-management">
              <ErrorPopup message={error} />

              {/* Groups section header */}
              <div className="groups-section-header">
                <div className="groups-section-header__left">
                  <h4>{t('tournament.currentGroups', 'Groups')}</h4>
                  <span className="groups-section-header__count">{tournament.groups?.length ?? 0}</span>
                </div>
                <button type="button" className="btn btn-primary" onClick={() => setShowAddGroupModal(true)}>
                  + {t('tournament.addGroup', 'Add Group')}
                </button>
              </div>

              {/* Groups compact list */}
              {(!tournament.groups || tournament.groups.length === 0) ? (
                <div className="groups-empty-state">
                  <p>{t('tournament.noGroups', 'No groups yet.')}</p>
                  <p className="groups-empty-state__hint">{t('tournament.noGroupsHint', 'Click "Add Group" to get started.')}</p>
                </div>
              ) : (
                <div className="groups-compact-list">
                  {tournament.groups.map((group) => (
                    <div key={group.id} className="groups-compact-list__item">
                      <span className={`group-phase-badge group-phase-badge--${group.phase.toLowerCase()}`}>
                        {group.phase === 'GroupStage'
                          ? t('tournament.phase.groupStage', 'Group Stage')
                          : t('tournament.phase.playoff', 'Playoff')}
                      </span>
                      <span className="groups-compact-list__name">{group.name}</span>
                      <span className="groups-compact-list__count">
                        {group.teams.length} {t('tournament.teamsLabel', 'teams')}
                      </span>
                      <button
                        type="button"
                        className="groups-compact-list__delete"
                        onClick={() => handleRemoveGroup(group.id)}
                        title={t('common.delete', 'Delete')}
                      >
                        &times;
                      </button>
                    </div>
                  ))}
                </div>
              )}

              {/* Team Management per Group */}
              {tournament.groups && tournament.groups.length > 0 && (
                <div className="tm-section" style={{ marginTop: '2rem' }}>
                  {/* Group selector pills */}
                  <div className="tm-division-selector">
                    <span className="tm-division-selector__label">{t('tournament.manageTeamsFor', 'Manage teams for:')}</span>
                    <div className="tm-division-selector__options">
                      {tournament.groups.map((g) => (
                        <button
                          key={g.id}
                          type="button"
                          className={`tm-division-pill ${selectedGroupForTeam === g.id ? 'active' : ''}`}
                          onClick={() => { setSelectedGroupForTeam(g.id); setSelectedTeamIds(new Set()); }}
                        >
                          {g.name}
                          <span className="tm-division-pill__badge">{g.teams.length}</span>
                        </button>
                      ))}
                    </div>
                  </div>

                  {selectedGroup && (
                    <>
                      {/* Teams already in group */}
                      <div className="tm-section">
                        <div className="tm-section__header">
                          <h4>{t('tournament.teamsInGroup', 'Teams in {{group}}', { group: selectedGroup.name })}</h4>
                          <span className="tm-section__count">{selectedGroup.teams.length}</span>
                        </div>
                        {selectedGroup.teams.length === 0 ? (
                          <div className="tm-section__empty">
                            <p>{t('tournament.noTeamsInGroup', 'No teams in this group yet.')}</p>
                          </div>
                        ) : (
                          <div className="tm-team-grid">
                            {selectedGroup.teams.map((gt) => (
                              <div key={gt.teamId} className="tm-team-chip">
                                <div className="tm-team-chip__info">
                                  <span className="tm-team-chip__name">{gt.teamName}</span>
                                </div>
                                <button
                                  type="button"
                                  className="tm-team-chip__remove"
                                  onClick={() => handleRemoveTeamFromGroup(selectedGroup.id, gt.teamId)}
                                  disabled={teamOperationLoading}
                                  title={t('common.remove', 'Remove')}
                                >
                                  &times;
                                </button>
                              </div>
                            ))}
                          </div>
                        )}
                      </div>

                      {/* Add teams */}
                      <div className="tm-section">
                        <div className="tm-section__header">
                          <h4>{t('tournament.availableTeams', 'Add Teams')}</h4>
                        </div>

                        {/* Search */}
                        <div className="tm-filters">
                          <div className="tm-filters__search">
                            <i className="fas fa-search"></i>
                            <input
                              type="text"
                              placeholder={t('tournament.searchTeams', 'Search teams by name...')}
                              value={searchTerm}
                              onChange={(e) => setSearchTerm(e.target.value)}
                            />
                            {searchTerm && (
                              <button type="button" className="tm-filters__clear" onClick={() => setSearchTerm('')}>
                                <i className="fas fa-times"></i>
                              </button>
                            )}
                          </div>
                        </div>

                        {/* Multi-select action bar */}
                        {selectedTeamIds.size > 0 && (
                          <div className="tm-action-bar">
                            <span>{t('tournament.selectedCount', '{{count}} team(s) selected', { count: selectedTeamIds.size })}</span>
                            <button
                              type="button"
                              className="btn btn-primary btn-sm"
                              onClick={handleAddSelectedTeams}
                              disabled={teamOperationLoading}
                            >
                              {teamOperationLoading
                                ? t('common.adding', 'Adding...')
                                : t('tournament.addSelectedToGroup', 'Add to {{group}}', { group: selectedGroup.name })}
                            </button>
                            <button type="button" className="tm-action-bar__clear" onClick={() => setSelectedTeamIds(new Set())}>
                              {t('common.clearSelection', 'Clear')}
                            </button>
                          </div>
                        )}

                        {/* Teams table */}
                        {loadingTeams ? (
                          <div className="tm-loading">
                            <p>{t('common.loading', 'Loading...')}</p>
                          </div>
                        ) : availableTeams.length === 0 ? (
                          <div className="tm-section__empty">
                            <p>
                              {searchTerm
                                ? t('tournament.noMatchingTeams', 'No teams match your search.')
                                : t('tournament.allTeamsAssigned', 'All teams are already in a group.')}
                            </p>
                          </div>
                        ) : (
                          <>
                            <div className="tm-table-wrapper">
                              <table className="tm-table">
                                <thead>
                                  <tr>
                                    <th className="tm-table__checkbox">
                                      <input
                                        type="checkbox"
                                        checked={availableTeamsNotInTournament.length > 0 && selectedTeamIds.size === availableTeamsNotInTournament.length}
                                        onChange={selectAllAvailable}
                                        disabled={availableTeamsNotInTournament.length === 0}
                                        title={t('common.selectAll', 'Select all')}
                                      />
                                    </th>
                                    <th>{t('tournament.fields.teamName', 'Team')}</th>
                                    <th>{t('tournament.fields.club', 'Club')}</th>
                                    <th className="tm-table__status">{t('common.status', 'Status')}</th>
                                    <th className="tm-table__actions"></th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {availableTeams.map((team) => {
                                    const isInTournament = allTeamIdsInTournament.has(team.id);
                                    const isSelected = selectedTeamIds.has(team.id);
                                    return (
                                      <tr key={team.id} className={`${isSelected ? 'selected' : ''} ${isInTournament ? 'in-season' : ''}`}>
                                        <td className="tm-table__checkbox">
                                          <input
                                            type="checkbox"
                                            checked={isSelected}
                                            onChange={() => toggleTeamSelection(team.id)}
                                            disabled={isInTournament || teamOperationLoading}
                                          />
                                        </td>
                                        <td>
                                          <span className="tm-table__team-name">{team.name}</span>
                                          {team.shortName && <span className="tm-table__short-name">({team.shortName})</span>}
                                        </td>
                                        <td className="tm-table__club">{team.club?.name ?? '-'}</td>
                                        <td className="tm-table__status">
                                          {isInTournament && (
                                            <span className="tm-status-badge tm-status-badge--added">
                                              {t('tournament.inTournament', 'In tournament')}
                                            </span>
                                          )}
                                        </td>
                                        <td className="tm-table__actions">
                                          {!isInTournament && (
                                            <button
                                              type="button"
                                              className="btn btn-primary btn-sm"
                                              onClick={() => handleAddSingleTeam(team.id)}
                                              disabled={teamOperationLoading}
                                            >
                                              {t('common.add', 'Add')}
                                            </button>
                                          )}
                                        </td>
                                      </tr>
                                    );
                                  })}
                                </tbody>
                              </table>
                            </div>
                            <Pagination
                              currentPage={teamsPagination.currentPage}
                              totalPages={teamsPagination.totalPages}
                              totalCount={teamsPagination.totalCount}
                              pageSize={teamsPagination.pageSize}
                              onPageChange={(page) => setTeamsPagination(prev => ({ ...prev, currentPage: page }))}
                              onPageSizeChange={(size) => setTeamsPagination(prev => ({ ...prev, pageSize: size, currentPage: 1 }))}
                              pageSizeOptions={[10, 25, 50]}
                              className="compact"
                            />
                          </>
                        )}
                      </div>
                    </>
                  )}
                </div>
              )}
            </div>
          )}
        </div>
      </div>
      {/* Add Group Modal */}
      {showAddGroupModal && (
        <div className="modal-overlay" onClick={() => setShowAddGroupModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>{t('tournament.addGroup', 'Add Group')}</h3>
              <button
                className="modal-close-btn"
                onClick={() => setShowAddGroupModal(false)}
                aria-label={t('common.close', 'Close')}
              >
                &times;
              </button>
            </div>
            <div className="modal-body">
              <div className="form-group">
                <label htmlFor="modal-group-name">{t('tournament.fields.groupName', 'Group Name')} *</label>
                <input
                  id="modal-group-name"
                  type="text"
                  value={newGroupName}
                  onChange={(e) => setNewGroupName(e.target.value)}
                  placeholder={t('tournament.placeholders.groupName', 'e.g. A-lohko')}
                  disabled={addingGroup}
                  autoFocus
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' && newGroupName.trim()) handleAddGroup();
                    if (e.key === 'Escape') setShowAddGroupModal(false);
                  }}
                />
              </div>
              <div className="form-group">
                <label htmlFor="modal-group-phase">{t('tournament.fields.groupPhase', 'Phase')}</label>
                <select
                  id="modal-group-phase"
                  value={newGroupPhase}
                  onChange={(e) => setNewGroupPhase(e.target.value)}
                  disabled={addingGroup}
                >
                  <option value="GroupStage">{t('tournament.phase.groupStage', 'Group Stage')}</option>
                  <option value="Playoff">{t('tournament.phase.playoff', 'Playoff')}</option>
                </select>
              </div>
            </div>
            <div className="modal-footer">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => { setShowAddGroupModal(false); setNewGroupName(''); }}
                disabled={addingGroup}
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button
                type="button"
                className="btn btn-primary"
                onClick={handleAddGroup}
                disabled={addingGroup || !newGroupName.trim()}
              >
                {addingGroup ? t('common.adding', 'Adding...') : t('tournament.addGroup', 'Add Group')}
              </button>
            </div>
          </div>
        </div>
      )}
    </PageTemplate>
  );
};

export default EditTournamentPage;
