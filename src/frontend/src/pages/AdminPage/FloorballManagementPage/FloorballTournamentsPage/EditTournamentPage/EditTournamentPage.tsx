import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';
import { floorballTournamentService } from '../../../../../api/floorball/floorballTournamentService';
import type {
  FloorballTournamentDto,
  CreateFloorballTournamentRequest,
} from '../../../../../types/floorball/tournamentTypes';
import { floorballTeamNameSearchService } from '../../../../../api/floorball/floorballTeamNameSearchService';
import type { FloorballTeamNameResult } from '../../../../../types/floorball/floorballTypes';
import '../../FloorballSeasonsPage/EditSeasonPage/EditSeasonPage.scss';

const EditTournamentPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { competitionId } = useParams<{ competitionId: string }>();

  const [tournament, setTournament] = useState<FloorballTournamentDto | null>(null);
  const [loadingTournament, setLoadingTournament] = useState(true);
  const [formData, setFormData] = useState<CreateFloorballTournamentRequest>({
    name: '',
    startDate: '',
    endDate: '',
    venue: '',
    contentHtml: '',
    groupStageNumberOfPeriods: 2,
    groupStagePeriodDurationMinutes: 15,
    groupStageAllowOvertime: false,
    groupStageOvertimeDurationMinutes: 5,
    groupStageAllowShootout: false,
    playoffNumberOfPeriods: 3,
    playoffPeriodDurationMinutes: 20,
    playoffAllowOvertime: true,
    playoffOvertimeDurationMinutes: 10,
    playoffAllowShootout: true,
    teamsAdvancingPerGroup: 2,
    hasPlayoffStage: true,
    hasThirdPlaceMatch: false,
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'details' | 'groups'>('details');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);

  // Group management
  const [newGroupName, setNewGroupName] = useState('');
  const [addingGroup, setAddingGroup] = useState(false);
  const [removingGroupId, setRemovingGroupId] = useState<string | null>(null);

  // Team-to-group management
  const [allTeams, setAllTeams] = useState<FloorballTeamNameResult[]>([]);
  const [selectedTeamId, setSelectedTeamId] = useState('');
  const [addingTeamToGroupId, setAddingTeamToGroupId] = useState<string | null>(null);
  const [removingTeamKey, setRemovingTeamKey] = useState<string | null>(null);

  // Lifecycle actions
  const [lifecycleLoading, setLifecycleLoading] = useState(false);

  useEffect(() => {
    return () => {
      if (successTimeoutId) clearTimeout(successTimeoutId);
    };
  }, [successTimeoutId]);

  const showSuccess = useCallback(
    (message: string) => {
      if (successTimeoutId) clearTimeout(successTimeoutId);
      setSuccessMessage(message);
      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
      }, 3000);
      setSuccessTimeoutId(timeoutId);
    },
    [successTimeoutId]
  );

  const loadTournament = useCallback(async () => {
    if (!competitionId) return;
    try {
      setLoadingTournament(true);
      const response = await floorballTournamentService.getById(competitionId);
      const data = response.data;
      setTournament(data);
      setFormData({
        name: data.name,
        startDate: data.startDate.split('T')[0],
        endDate: data.endDate.split('T')[0],
        venue: data.venue ?? '',
        contentHtml: data.contentHtml ?? '',
        groupStageNumberOfPeriods: data.tournamentRules?.groupStageMatchRules?.numberOfPeriods ?? 2,
        groupStagePeriodDurationMinutes: data.tournamentRules?.groupStageMatchRules?.periodDurationMinutes ?? 15,
        groupStageAllowOvertime: data.tournamentRules?.groupStageMatchRules?.allowOvertime ?? false,
        groupStageOvertimeDurationMinutes: data.tournamentRules?.groupStageMatchRules?.overtimeDurationMinutes ?? 5,
        groupStageAllowShootout: data.tournamentRules?.groupStageMatchRules?.allowShootout ?? false,
        playoffNumberOfPeriods: data.tournamentRules?.playoffMatchRules?.numberOfPeriods ?? 3,
        playoffPeriodDurationMinutes: data.tournamentRules?.playoffMatchRules?.periodDurationMinutes ?? 20,
        playoffAllowOvertime: data.tournamentRules?.playoffMatchRules?.allowOvertime ?? true,
        playoffOvertimeDurationMinutes: data.tournamentRules?.playoffMatchRules?.overtimeDurationMinutes ?? 10,
        playoffAllowShootout: data.tournamentRules?.playoffMatchRules?.allowShootout ?? true,
        teamsAdvancingPerGroup: data.tournamentRules?.teamsAdvancingPerGroup ?? 2,
        hasPlayoffStage: data.tournamentRules?.hasPlayoffStage ?? true,
        hasThirdPlaceMatch: data.tournamentRules?.hasThirdPlaceMatch ?? false,
      });
    } catch {
      setError(t('floorball.tournaments.errors.loadFailed', 'Failed to load tournament'));
    } finally {
      setLoadingTournament(false);
    }
  }, [competitionId, t]);

  const loadTeams = useCallback(async () => {
    try {
      const response = await floorballTeamNameSearchService.getTeamNames('');
      if (response?.data) {
        setAllTeams(response.data);
      }
    } catch {
      // Teams loading is non-critical
    }
  }, []);

  useEffect(() => {
    loadTournament();
    loadTeams();
  }, [loadTournament, loadTeams]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'number' ? parseInt(value, 10) || 0 : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!competitionId) return;
    setLoading(true);
    setError(null);

    try {
      if (!formData.name.trim()) {
        throw new Error(t('floorball.tournaments.validation.nameRequired', 'Tournament name is required'));
      }
      if (!formData.startDate) {
        throw new Error(t('floorball.tournaments.validation.startDateRequired', 'Start date is required'));
      }
      if (!formData.endDate) {
        throw new Error(t('floorball.tournaments.validation.endDateRequired', 'End date is required'));
      }

      await floorballTournamentService.update(competitionId, formData);
      showSuccess(t('floorball.tournaments.updated', 'Tournament updated successfully!'));
      await loadTournament();
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to update tournament';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  // Group management handlers
  const handleAddGroup = async () => {
    if (!competitionId || !newGroupName.trim()) return;
    setAddingGroup(true);
    setError(null);
    try {
      await floorballTournamentService.addGroup(competitionId, newGroupName.trim());
      setNewGroupName('');
      await loadTournament();
      showSuccess(t('floorball.tournaments.groupAdded', 'Group added!'));
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to add group';
      setError(msg);
    } finally {
      setAddingGroup(false);
    }
  };

  const handleRemoveGroup = async (groupId: string) => {
    if (!competitionId) return;
    setRemovingGroupId(groupId);
    setError(null);
    try {
      await floorballTournamentService.removeGroup(competitionId, groupId);
      await loadTournament();
      showSuccess(t('floorball.tournaments.groupRemoved', 'Group removed!'));
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to remove group';
      setError(msg);
    } finally {
      setRemovingGroupId(null);
    }
  };

  const handleAddTeamToGroup = async (groupId: string) => {
    if (!competitionId || !selectedTeamId) return;
    setAddingTeamToGroupId(groupId);
    setError(null);
    try {
      await floorballTournamentService.addTeamToGroup(competitionId, groupId, selectedTeamId);
      setSelectedTeamId('');
      await loadTournament();
      showSuccess(t('floorball.tournaments.teamAdded', 'Team added to group!'));
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to add team';
      setError(msg);
    } finally {
      setAddingTeamToGroupId(null);
    }
  };

  const handleRemoveTeamFromGroup = async (groupId: string, teamId: string) => {
    if (!competitionId) return;
    const key = `${groupId}-${teamId}`;
    setRemovingTeamKey(key);
    setError(null);
    try {
      await floorballTournamentService.removeTeamFromGroup(competitionId, groupId, teamId);
      await loadTournament();
      showSuccess(t('floorball.tournaments.teamRemoved', 'Team removed from group!'));
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to remove team';
      setError(msg);
    } finally {
      setRemovingTeamKey(null);
    }
  };

  // Lifecycle actions
  const handleLifecycleAction = async (action: 'openRegistration' | 'startGroupStage' | 'startPlayoffStage' | 'complete') => {
    if (!competitionId) return;
    setLifecycleLoading(true);
    setError(null);
    try {
      await floorballTournamentService[action](competitionId);
      await loadTournament();
      showSuccess(t(`floorball.tournaments.lifecycle.${action}Success`, 'Action completed successfully!'));
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Action failed';
      setError(msg);
    } finally {
      setLifecycleLoading(false);
    }
  };

  // Get teams already assigned to any group (to exclude from dropdown)
  const assignedTeamIds = new Set(
    tournament?.groups?.flatMap((g) => g.teams.map((t) => t.teamId)) ?? []
  );

  const availableTeamsForGroup = allTeams.filter((t) => !assignedTeamIds.has(t.id));

  if (loadingTournament) {
    return (
      <PageTemplate title={t('floorball.tournaments.editTitle', 'Edit Tournament')}>
        <div className="edit-season-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  if (!tournament) {
    return (
      <PageTemplate title={t('floorball.tournaments.editTitle', 'Edit Tournament')}>
        <ErrorPopup message={t('floorball.tournaments.errors.notFound', 'Tournament not found')} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.tournaments.editTitle', 'Edit Tournament')}>
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}

      <div className="edit-season-container">
        {/* Tab Navigation */}
        <div className="tab-navigation">
          <button
            className={`tab-button ${activeTab === 'details' ? 'active' : ''}`}
            onClick={() => setActiveTab('details')}
          >
            {t('floorball.tournaments.tabs.details', 'Tournament Details')}
          </button>
          <button
            className={`tab-button ${activeTab === 'groups' ? 'active' : ''}`}
            onClick={() => setActiveTab('groups')}
          >
            {t('floorball.tournaments.tabs.groups', 'Manage Groups')} ({tournament.groups?.length ?? 0})
          </button>
        </div>

        <div className="edit-season-content">
          {/* Details Tab */}
          {activeTab === 'details' && (
            <>
              {/* Lifecycle Actions */}
              <div style={{ padding: '16px 24px', borderBottom: '1px solid #e5e7eb', background: '#f9fafb' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px', flexWrap: 'wrap' }}>
                  <span style={{ fontWeight: 600, fontSize: '13px', color: '#6b7280', textTransform: 'uppercase', letterSpacing: '0.04em', marginRight: '8px' }}>
                    {t('floorball.tournaments.status', 'Status')}: {tournament.tournamentStatus}
                  </span>
                  <button
                    type="button"
                    className="btn btn-primary"
                    style={{ padding: '6px 14px', fontSize: '13px', border: 'none', borderRadius: '6px', cursor: 'pointer', background: '#3b82f6', color: '#fff' }}
                    onClick={() => handleLifecycleAction('openRegistration')}
                    disabled={lifecycleLoading}
                  >
                    {t('floorball.tournaments.lifecycle.openRegistration', 'Open Registration')}
                  </button>
                  <button
                    type="button"
                    style={{ padding: '6px 14px', fontSize: '13px', border: 'none', borderRadius: '6px', cursor: 'pointer', background: '#f59e0b', color: '#fff' }}
                    onClick={() => handleLifecycleAction('startGroupStage')}
                    disabled={lifecycleLoading}
                  >
                    {t('floorball.tournaments.lifecycle.startGroupStage', 'Start Group Stage')}
                  </button>
                  <button
                    type="button"
                    style={{ padding: '6px 14px', fontSize: '13px', border: 'none', borderRadius: '6px', cursor: 'pointer', background: '#f59e0b', color: '#fff' }}
                    onClick={() => handleLifecycleAction('startPlayoffStage')}
                    disabled={lifecycleLoading}
                  >
                    {t('floorball.tournaments.lifecycle.startPlayoffStage', 'Start Playoff')}
                  </button>
                  <button
                    type="button"
                    style={{ padding: '6px 14px', fontSize: '13px', border: 'none', borderRadius: '6px', cursor: 'pointer', background: '#10b981', color: '#fff' }}
                    onClick={() => handleLifecycleAction('complete')}
                    disabled={lifecycleLoading}
                  >
                    {t('floorball.tournaments.lifecycle.complete', 'Complete')}
                  </button>
                </div>
              </div>

              <form onSubmit={handleSubmit} className="edit-season-form">
                <ErrorPopup message={error} />

                {/* Basic Information */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-info-circle"></i>
                    {t('floorball.tournaments.sections.basicInfo', 'Basic Information')}
                  </h3>
                  <div className="form-group">
                    <label htmlFor="edit-name">{t('floorball.tournaments.fields.name', 'Name')} *</label>
                    <input type="text" id="edit-name" name="name" value={formData.name} onChange={handleInputChange} required disabled={loading} />
                  </div>
                  <div className="form-group">
                    <label htmlFor="edit-venue">{t('floorball.tournaments.fields.venue', 'Venue')}</label>
                    <input type="text" id="edit-venue" name="venue" value={formData.venue ?? ''} onChange={handleInputChange} disabled={loading} />
                  </div>
                </div>

                {/* Schedule */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-calendar-alt"></i>
                    {t('floorball.tournaments.sections.schedule', 'Schedule')}
                  </h3>
                  <div className="form-row">
                    <div className="form-group">
                      <label htmlFor="edit-startDate">{t('floorball.tournaments.fields.startDate', 'Start Date')} *</label>
                      <input type="date" id="edit-startDate" name="startDate" value={formData.startDate} onChange={handleInputChange} required disabled={loading} />
                    </div>
                    <div className="form-group">
                      <label htmlFor="edit-endDate">{t('floorball.tournaments.fields.endDate', 'End Date')} *</label>
                      <input type="date" id="edit-endDate" name="endDate" value={formData.endDate} onChange={handleInputChange} required disabled={loading} min={formData.startDate} />
                    </div>
                  </div>
                </div>

                {/* Content */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-file-alt"></i>
                    {t('floorball.tournaments.sections.content', 'Description')}
                  </h3>
                  <div className="form-group">
                    <label htmlFor="edit-contentHtml">{t('floorball.tournaments.fields.contentHtml', 'Content (HTML)')}</label>
                    <textarea
                      id="edit-contentHtml"
                      name="contentHtml"
                      value={formData.contentHtml ?? ''}
                      onChange={handleInputChange}
                      disabled={loading}
                      rows={5}
                      style={{ width: '100%', resize: 'vertical', padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '6px', fontFamily: 'inherit', fontSize: '14px', boxSizing: 'border-box' }}
                    />
                  </div>
                </div>

                {/* Group Stage Match Rules */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-gavel"></i>
                    {t('floorball.tournaments.sections.groupStageRules', 'Group Stage Match Rules')}
                  </h3>
                  <div className="form-row">
                    <div className="form-group">
                      <label htmlFor="edit-groupStageNumberOfPeriods">{t('floorball.tournaments.fields.numberOfPeriods', 'Number of Periods')}</label>
                      <select id="edit-groupStageNumberOfPeriods" name="groupStageNumberOfPeriods" value={formData.groupStageNumberOfPeriods} onChange={handleInputChange} disabled={loading}>
                        {[1, 2, 3, 4, 5].map((n) => <option key={n} value={n}>{n}</option>)}
                      </select>
                    </div>
                    <div className="form-group">
                      <label htmlFor="edit-groupStagePeriodDurationMinutes">{t('floorball.tournaments.fields.periodDuration', 'Period Duration (min)')}</label>
                      <input type="number" id="edit-groupStagePeriodDurationMinutes" name="groupStagePeriodDurationMinutes" value={formData.groupStagePeriodDurationMinutes} onChange={handleInputChange} min={1} max={60} disabled={loading} />
                    </div>
                  </div>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('floorball.tournaments.fields.allowOvertime', 'Allow Overtime')}</label>
                    <button type="button" className={`toggle-switch ${formData.groupStageAllowOvertime ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, groupStageAllowOvertime: !p.groupStageAllowOvertime }))} disabled={loading} aria-pressed={formData.groupStageAllowOvertime}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  {formData.groupStageAllowOvertime && (
                    <div className="form-group form-group--indented">
                      <label htmlFor="edit-groupStageOvertimeDurationMinutes">{t('floorball.tournaments.fields.overtimeDuration', 'Overtime Duration (min)')}</label>
                      <input type="number" id="edit-groupStageOvertimeDurationMinutes" name="groupStageOvertimeDurationMinutes" value={formData.groupStageOvertimeDurationMinutes} onChange={handleInputChange} min={1} max={30} disabled={loading} />
                    </div>
                  )}
                  <div className="toggle-container">
                    <label className="toggle-label">{t('floorball.tournaments.fields.allowShootout', 'Allow Shootout')}</label>
                    <button type="button" className={`toggle-switch ${formData.groupStageAllowShootout ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, groupStageAllowShootout: !p.groupStageAllowShootout }))} disabled={loading} aria-pressed={formData.groupStageAllowShootout}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                </div>

                {/* Playoff Match Rules */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-trophy"></i>
                    {t('floorball.tournaments.sections.playoffRules', 'Playoff Match Rules')}
                  </h3>
                  <div className="form-row">
                    <div className="form-group">
                      <label htmlFor="edit-playoffNumberOfPeriods">{t('floorball.tournaments.fields.numberOfPeriods', 'Number of Periods')}</label>
                      <select id="edit-playoffNumberOfPeriods" name="playoffNumberOfPeriods" value={formData.playoffNumberOfPeriods} onChange={handleInputChange} disabled={loading}>
                        {[1, 2, 3, 4, 5].map((n) => <option key={n} value={n}>{n}</option>)}
                      </select>
                    </div>
                    <div className="form-group">
                      <label htmlFor="edit-playoffPeriodDurationMinutes">{t('floorball.tournaments.fields.periodDuration', 'Period Duration (min)')}</label>
                      <input type="number" id="edit-playoffPeriodDurationMinutes" name="playoffPeriodDurationMinutes" value={formData.playoffPeriodDurationMinutes} onChange={handleInputChange} min={1} max={60} disabled={loading} />
                    </div>
                  </div>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('floorball.tournaments.fields.allowOvertime', 'Allow Overtime')}</label>
                    <button type="button" className={`toggle-switch ${formData.playoffAllowOvertime ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, playoffAllowOvertime: !p.playoffAllowOvertime }))} disabled={loading} aria-pressed={formData.playoffAllowOvertime}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  {formData.playoffAllowOvertime && (
                    <div className="form-group form-group--indented">
                      <label htmlFor="edit-playoffOvertimeDurationMinutes">{t('floorball.tournaments.fields.overtimeDuration', 'Overtime Duration (min)')}</label>
                      <input type="number" id="edit-playoffOvertimeDurationMinutes" name="playoffOvertimeDurationMinutes" value={formData.playoffOvertimeDurationMinutes} onChange={handleInputChange} min={1} max={30} disabled={loading} />
                    </div>
                  )}
                  <div className="toggle-container">
                    <label className="toggle-label">{t('floorball.tournaments.fields.allowShootout', 'Allow Shootout')}</label>
                    <button type="button" className={`toggle-switch ${formData.playoffAllowShootout ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, playoffAllowShootout: !p.playoffAllowShootout }))} disabled={loading} aria-pressed={formData.playoffAllowShootout}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                </div>

                {/* Tournament Rules */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-cogs"></i>
                    {t('floorball.tournaments.sections.tournamentRules', 'Tournament Rules')}
                  </h3>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('floorball.tournaments.fields.hasPlayoffStage', 'Has Playoff Stage')}</label>
                    <button type="button" className={`toggle-switch ${formData.hasPlayoffStage ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, hasPlayoffStage: !p.hasPlayoffStage }))} disabled={loading} aria-pressed={formData.hasPlayoffStage}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  {formData.hasPlayoffStage && (
                    <>
                      <div className="form-group form-group--indented">
                        <label htmlFor="edit-teamsAdvancingPerGroup">{t('floorball.tournaments.fields.teamsAdvancingPerGroup', 'Teams Advancing Per Group')}</label>
                        <input type="number" id="edit-teamsAdvancingPerGroup" name="teamsAdvancingPerGroup" value={formData.teamsAdvancingPerGroup} onChange={handleInputChange} min={1} max={8} disabled={loading} />
                      </div>
                      <div className="toggle-container">
                        <label className="toggle-label">{t('floorball.tournaments.fields.hasThirdPlaceMatch', 'Has Third Place Match')}</label>
                        <button type="button" className={`toggle-switch ${formData.hasThirdPlaceMatch ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, hasThirdPlaceMatch: !p.hasThirdPlaceMatch }))} disabled={loading} aria-pressed={formData.hasThirdPlaceMatch}>
                          <span className="toggle-switch__slider" />
                        </button>
                      </div>
                    </>
                  )}
                </div>

                <div className="form-actions">
                  <button type="button" className="btn btn-secondary" onClick={() => navigate('/admin/floorball/tournaments')} disabled={loading}>
                    {t('common.cancel', 'Cancel')}
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={loading}>
                    {loading ? (<><i className="fas fa-spinner fa-spin"></i> {t('common.saving', 'Saving...')}</>) : t('common.save', 'Save')}
                  </button>
                </div>
              </form>
            </>
          )}

          {/* Groups Tab */}
          {activeTab === 'groups' && (
            <div className="divisions-management">
              <ErrorPopup message={error} />

              {/* Add new group */}
              <div className="current-divisions-section">
                <h4>{t('floorball.tournaments.addGroup', 'Add Group')}</h4>
                <div style={{ display: 'flex', gap: '8px', marginBottom: '16px' }}>
                  <input
                    type="text"
                    value={newGroupName}
                    onChange={(e) => setNewGroupName(e.target.value)}
                    placeholder={t('floorball.tournaments.placeholders.groupName', 'e.g. Group A')}
                    disabled={addingGroup}
                    style={{ flex: 1, padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '6px', fontSize: '14px' }}
                  />
                  <button
                    type="button"
                    className="btn btn-primary"
                    onClick={handleAddGroup}
                    disabled={addingGroup || !newGroupName.trim()}
                  >
                    {addingGroup ? (<><i className="fas fa-spinner fa-spin"></i> {t('common.adding', 'Adding...')}</>) : (<><i className="fas fa-plus"></i> {t('common.add', 'Add')}</>)}
                  </button>
                </div>
              </div>

              {/* Existing groups */}
              <div className="available-divisions-section">
                <h4>{t('floorball.tournaments.groups', 'Groups')} ({tournament.groups?.length ?? 0})</h4>

                {(!tournament.groups || tournament.groups.length === 0) ? (
                  <p className="no-divisions">{t('floorball.tournaments.noGroups', 'No groups yet. Add one above.')}</p>
                ) : (
                  <div className="divisions-list">
                    {tournament.groups.map((group) => (
                      <div key={group.id} style={{ background: '#fff', border: '1px solid #e5e7eb', borderRadius: '8px', padding: '16px', marginBottom: '8px' }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px' }}>
                          <div>
                            <strong style={{ fontSize: '15px' }}>{group.name}</strong>
                            <span style={{ marginLeft: '8px', fontSize: '13px', color: '#6b7280' }}>
                              ({group.teams.length} {t('floorball.tournaments.teams', 'teams')})
                            </span>
                          </div>
                          <button
                            type="button"
                            className="btn btn-danger"
                            onClick={() => handleRemoveGroup(group.id)}
                            disabled={removingGroupId === group.id}
                          >
                            {removingGroupId === group.id ? <i className="fas fa-spinner fa-spin"></i> : <i className="fas fa-trash-alt"></i>}
                          </button>
                        </div>

                        {/* Teams in this group */}
                        {group.teams.length > 0 && (
                          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px', marginBottom: '12px' }}>
                            {group.teams.map((team) => (
                              <span
                                key={team.id}
                                style={{ display: 'inline-flex', alignItems: 'center', gap: '6px', padding: '4px 10px', background: '#eff6ff', border: '1px solid #bfdbfe', borderRadius: '100px', fontSize: '13px' }}
                              >
                                {team.teamName}
                                <button
                                  type="button"
                                  onClick={() => handleRemoveTeamFromGroup(group.id, team.teamId)}
                                  disabled={removingTeamKey === `${group.id}-${team.teamId}`}
                                  style={{ border: 'none', background: 'none', cursor: 'pointer', color: '#ef4444', padding: '0 2px', fontSize: '14px', lineHeight: 1 }}
                                  title={t('common.remove', 'Remove')}
                                >
                                  {removingTeamKey === `${group.id}-${team.teamId}` ? <i className="fas fa-spinner fa-spin" style={{ fontSize: '10px' }}></i> : '×'}
                                </button>
                              </span>
                            ))}
                          </div>
                        )}

                        {/* Add team to this group */}
                        <div style={{ display: 'flex', gap: '8px' }}>
                          <select
                            value={selectedTeamId}
                            onChange={(e) => setSelectedTeamId(e.target.value)}
                            disabled={addingTeamToGroupId === group.id}
                            style={{ flex: 1, padding: '6px 10px', border: '1px solid #d1d5db', borderRadius: '6px', fontSize: '13px' }}
                          >
                            <option value="">{t('floorball.tournaments.selectTeam', '-- Select team --')}</option>
                            {availableTeamsForGroup.map((team) => (
                              <option key={team.id} value={team.id}>{team.name}</option>
                            ))}
                          </select>
                          <button
                            type="button"
                            className="btn btn-primary"
                            onClick={() => handleAddTeamToGroup(group.id)}
                            disabled={addingTeamToGroupId === group.id || !selectedTeamId}
                            style={{ fontSize: '13px', padding: '6px 12px' }}
                          >
                            {addingTeamToGroupId === group.id ? <i className="fas fa-spinner fa-spin"></i> : <><i className="fas fa-plus"></i> {t('common.add', 'Add')}</>}
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </PageTemplate>
  );
};

export default EditTournamentPage;
