import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { hockeySeasonService } from '../../../../api/hockey/hockeySeasonService';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import { divisionService } from '../../../../api/common/divisionService';
import Pagination from '../../../../components/Pagination';
import { SportsCategory } from '../../../../types/common/sports';
import {
  HOCKEY_TEAM_CATEGORIES,
  type HockeySeasonDto,
  type HockeyTeamCategory,
  type HockeyTeamDto,
} from '../../../../types/hockey/hockeyTypes';
import { loadClubNameMap } from '../../../../utils/hockeyLookups';
import SeasonContentBlocksEditor from '../../../../components/SeasonContentBlocksEditor/SeasonContentBlocksEditor';
import {
  toContentBlockDrafts,
  toContentBlockItems,
  type SeasonContentBlockDraft,
} from '../../../../types/common/seasonContent';
import './EditSeasonPage.scss';

type SeasonTab = 'details' | 'divisions' | 'teams';

function EditHockeySeasonPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { competitionId } = useParams<{ competitionId: string }>();
  const [season, setSeason] = useState<HockeySeasonDto | null>(null);
  const [teams, setTeams] = useState<HockeyTeamDto[]>([]);
  const [clubNames, setClubNames] = useState<Map<string, string>>(new Map());
  const [catalogDivisions, setCatalogDivisions] = useState<Array<{ id: string; name: string }>>([]);
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [seasonCode, setSeasonCode] = useState('');
  const [competitionCategory, setCompetitionCategory] = useState<HockeyTeamCategory>('Adult');
  const [selectedDivisionId, setSelectedDivisionId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [loadingSeason, setLoadingSeason] = useState(true);
  const [activeTab, setActiveTab] = useState<SeasonTab>('details');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [teamCategory, setTeamCategory] = useState<HockeyTeamCategory | ''>('');
  const [selectedTeamIds, setSelectedTeamIds] = useState<Set<string>>(new Set());
  const [teamsPage, setTeamsPage] = useState(1);
  const [teamsPageSize, setTeamsPageSize] = useState(10);
  const [contentBlocks, setContentBlocks] = useState<SeasonContentBlockDraft[]>([]);

  const load = useCallback(async (): Promise<void> => {
    if (!competitionId) {
      return;
    }
    const [loaded, teamList, divisionsResponse, clubs, content] = await Promise.all([
      hockeySeasonService.getById(competitionId),
      hockeyTeamService.getAll(),
      divisionService.getBySportType(SportsCategory.Icehockey, true).catch(() => ({ data: [] as Array<{ id: string; name: string }> })),
      loadClubNameMap().catch(() => new Map<string, string>()),
      hockeySeasonService.getContentBlocks(competitionId),
    ]);
    setSeason(loaded);
    setContentBlocks(toContentBlockDrafts(content.blocks));
    setTeams(teamList);
    setClubNames(clubs);
    setCatalogDivisions((divisionsResponse.data ?? []).map((item) => ({ id: item.id, name: item.name })));
    setName(loaded.name);
    setStartDate(loaded.startDate.slice(0, 10));
    setEndDate(loaded.endDate.slice(0, 10));
    setSeasonCode(loaded.seasonCode ?? '');
    setCompetitionCategory(loaded.teamCategory ?? 'Adult');
  }, [competitionId]);

  useEffect(() => {
    void load()
      .catch((err) => setError(err instanceof Error ? err.message : t('hockey.seasons.errors.loadFailed', 'Failed to load season data')))
      .finally(() => setLoadingSeason(false));
  }, [load, t]);

  useEffect(() => {
    if (season?.divisions.length && !selectedDivisionId) {
      setSelectedDivisionId(season.divisions[0].id);
    }
  }, [season, selectedDivisionId]);

  useEffect(() => {
    setTeamsPage(1);
  }, [searchTerm, teamCategory]);

  const run = async (action: () => Promise<unknown>, success?: string): Promise<void> => {
    setSaving(true);
    setError(null);
    try {
      await action();
      await load();
      if (success) {
        setSuccessMessage(success);
        window.setTimeout(() => setSuccessMessage(null), 2000);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : t('hockey.seasons.errors.updateFailed', 'Operation failed'));
    } finally {
      setSaving(false);
    }
  };

  const assignedIds = useMemo(() => new Set((season?.teams ?? []).map((item) => item.teamId)), [season]);
  const assignedDivisionIds = useMemo(() => new Set((season?.divisions ?? []).map((item) => item.divisionId)), [season]);
  const availableDivisions = catalogDivisions.filter((division) => !assignedDivisionIds.has(division.id));
  const selectedDivision = season?.divisions.find((item) => item.id === selectedDivisionId) ?? null;

  const teamsInSelectedDivision = useMemo(() => {
    if (!season || !selectedDivision) {
      return [] as HockeyTeamDto[];
    }
    return selectedDivision.teams
      .map((member) => {
        const membership = season.teams.find((item) => item.id === member.competitionTeamId);
        return membership ? teams.find((team) => team.id === membership.teamId) : undefined;
      })
      .filter((team): team is HockeyTeamDto => Boolean(team));
  }, [season, selectedDivision, teams]);

  const filteredAvailableTeams = useMemo(() => {
    const needle = searchTerm.trim().toLowerCase();
    return teams.filter((team) => {
      if (teamCategory && team.teamCategory !== teamCategory) {
        return false;
      }
      if (needle && !team.name.toLowerCase().includes(needle)) {
        return false;
      }
      return true;
    });
  }, [teams, searchTerm, teamCategory]);

  const availableTeamsNotInSeason = filteredAvailableTeams.filter((team) => !assignedIds.has(team.id));
  const teamsTotalPages = Math.max(1, Math.ceil(availableTeamsNotInSeason.length / teamsPageSize));
  const pagedAvailableTeams = availableTeamsNotInSeason.slice(
    (teamsPage - 1) * teamsPageSize,
    teamsPage * teamsPageSize,
  );

  if (loadingSeason) {
    return (
      <PageTemplate title={t('hockey.seasons.edit', 'Edit Season')}>
        <div className="edit-season-loading"><p>{t('common.loading', 'Loading...')}</p></div>
      </PageTemplate>
    );
  }

  if (!season) {
    return (
      <PageTemplate title={t('hockey.seasons.edit', 'Edit Season')}>
        <ErrorPopup message={error ?? t('hockey.seasons.errors.notFound', 'Season not found')} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('hockey.seasons.edit', 'Edit Season')}>
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}
      <div className="edit-season-container">
        <div className="edit-season-back">
          <button type="button" className="back-button" onClick={() => navigate('/admin/hockey/seasons')}>
            <i className="fas fa-arrow-left"></i>
            {t('common.back', 'Back')}
          </button>
        </div>
        <div className="tab-navigation">
          <button type="button" className={`tab-button ${activeTab === 'details' ? 'active' : ''}`} onClick={() => setActiveTab('details')}>
            {t('hockey.seasons.tabs.details', 'Season Details')}
          </button>
          <button type="button" className={`tab-button ${activeTab === 'divisions' ? 'active' : ''}`} onClick={() => setActiveTab('divisions')}>
            {t('hockey.seasons.manageDivisions', 'Manage Divisions')} ({season.divisions.length})
          </button>
          <button type="button" className={`tab-button ${activeTab === 'teams' ? 'active' : ''}`} onClick={() => setActiveTab('teams')}>
            {t('hockey.seasons.manageTeams', 'Manage Teams')} ({season.teams.length})
          </button>
        </div>
        <div className="edit-season-content">
          {activeTab === 'details' && (
            <form
              className="edit-season-form"
              onSubmit={(event) => {
                event.preventDefault();
                if (contentBlocks.some((block) => !block.title.trim())) {
                  setError(t('seasonContent.titleRequired'));
                  return;
                }
                void run(
                  async () => {
                    await hockeySeasonService.update(season.id, {
                      name,
                      startDate: new Date(startDate).toISOString(),
                      endDate: new Date(endDate).toISOString(),
                      seasonCode: seasonCode || null,
                      teamCategory: competitionCategory,
                    });
                    await hockeySeasonService.replaceContentBlocks(
                      season.id,
                      toContentBlockItems(contentBlocks),
                    );
                  },
                  t('hockey.seasons.seasonUpdated', 'Season updated successfully!'),
                );
              }}
            >
              <ErrorPopup message={error} />
              <div className="form-section">
                <h3 className="form-section__title">
                  <i className="fas fa-info-circle"></i>
                  {t('hockey.seasons.sections.basicInfo', 'Basic Information')}
                </h3>
                <div className="form-group">
                  <label htmlFor="edit-name">{t('hockey.seasons.fields.name', 'Name')} *</label>
                  <input id="edit-name" value={name} onChange={(event) => setName(event.target.value)} required disabled={saving} />
                </div>
                <div className="form-group">
                  <label htmlFor="edit-code">{t('hockey.seasons.seasonCode', 'Season code')}</label>
                  <input id="edit-code" value={seasonCode} onChange={(event) => setSeasonCode(event.target.value)} disabled={saving} />
                </div>
                <div className="form-group">
                  <label htmlFor="edit-category">{t('hockey.teams.category', 'Category')}</label>
                  <select
                    id="edit-category"
                    value={competitionCategory}
                    onChange={(event) => setCompetitionCategory(event.target.value as HockeyTeamCategory)}
                    disabled={saving}
                  >
                    {HOCKEY_TEAM_CATEGORIES.map((category) => (
                      <option key={category} value={category}>
                        {t(`hockey.teams.categories.${category}`, category)}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="form-section">
                <h3 className="form-section__title">
                  <i className="fas fa-calendar-alt"></i>
                  {t('hockey.seasons.sections.schedule', 'Schedule')}
                </h3>
                <div className="form-row">
                  <div className="form-group">
                    <label htmlFor="edit-startDate">{t('hockey.seasons.fields.startDate', 'Start Date')} *</label>
                    <input id="edit-startDate" type="date" value={startDate} onChange={(event) => setStartDate(event.target.value)} required disabled={saving} />
                  </div>
                  <div className="form-group">
                    <label htmlFor="edit-endDate">{t('hockey.seasons.fields.endDate', 'End Date')} *</label>
                    <input id="edit-endDate" type="date" value={endDate} onChange={(event) => setEndDate(event.target.value)} required disabled={saving} min={startDate} />
                  </div>
                </div>
              </div>
              <div className="form-section">
                <h3 className="form-section__title">
                  <i className="fas fa-flag"></i>
                  {t('hockey.seasons.lifecycle', 'Lifecycle')}
                </h3>
                <p className="form-hint">{season.status}</p>
                <div className="form-actions" style={{ justifyContent: 'flex-start', flexWrap: 'wrap' }}>
                  <button type="button" className="btn btn-secondary" disabled={saving} onClick={() => void run(() => hockeySeasonService.publish(season.id))}>
                    {t('hockey.seasons.publish', 'Publish')}
                  </button>
                  <button type="button" className="btn btn-secondary" disabled={saving} onClick={() => void run(() => hockeySeasonService.openRegistration(season.id))}>
                    {t('hockey.seasons.openRegistration', 'Open registration')}
                  </button>
                  <button type="button" className="btn btn-secondary" disabled={saving} onClick={() => void run(() => hockeySeasonService.activate(season.id))}>
                    {t('hockey.seasons.activate', 'Activate')}
                  </button>
                  <button type="button" className="btn btn-secondary" disabled={saving} onClick={() => void run(() => hockeySeasonService.deactivate(season.id))}>
                    {t('hockey.seasons.deactivate', 'Deactivate')}
                  </button>
                  <button type="button" className="btn btn-primary" disabled={saving} onClick={() => void run(() => hockeySeasonService.complete(season.id))}>
                    {t('hockey.seasons.complete', 'Complete')}
                  </button>
                </div>
              </div>
              <div className="form-section">
                <SeasonContentBlocksEditor
                  blocks={contentBlocks}
                  onChange={setContentBlocks}
                  disabled={saving}
                />
              </div>
              <div className="form-actions">
                <button type="button" className="btn btn-secondary" onClick={() => navigate('/admin/hockey/seasons')} disabled={saving}>
                  {t('common.cancel', 'Cancel')}
                </button>
                <button type="submit" className="btn btn-primary" disabled={saving}>
                  {saving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
                </button>
              </div>
            </form>
          )}

          {activeTab === 'divisions' && (
            <div className="divisions-management">
              <ErrorPopup message={error} />
              <div className="current-divisions-section">
                <h4>{t('hockey.seasons.currentDivisions', 'Current Divisions')} ({season.divisions.length})</h4>
                {season.divisions.length === 0 ? (
                  <p className="no-divisions">{t('hockey.seasons.noDivisions', 'No divisions in this season')}</p>
                ) : (
                  <div className="divisions-list">
                    {season.divisions.map((division) => (
                      <div key={division.id} className="division-item">
                        <div className="division-info">
                          <span className="division-name">{division.name}</span>
                          <span className="division-team-count">
                            {t('hockey.seasons.teamCount', '{{count}} team(s)', { count: division.teams.length })}
                          </span>
                        </div>
                        <button
                          type="button"
                          className="btn btn-danger btn-sm"
                          disabled={saving}
                          onClick={() => void run(() => hockeySeasonService.removeDivision(season.id, division.id))}
                        >
                          <i className="fas fa-trash-alt"></i>
                          {t('common.remove', 'Remove')}
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
              <div className="available-divisions-section">
                <h4>{t('hockey.seasons.availableDivisions', 'Available Divisions')} ({availableDivisions.length})</h4>
                {availableDivisions.length === 0 ? (
                  <p className="no-divisions">{t('hockey.seasons.allDivisionsAdded', 'All divisions have been added to this season')}</p>
                ) : (
                  <div className="divisions-list">
                    {availableDivisions.map((division) => (
                      <div key={division.id} className="division-item">
                        <div className="division-info">
                          <span className="division-name">{division.name}</span>
                        </div>
                        <button
                          type="button"
                          className="btn btn-primary btn-sm"
                          disabled={saving}
                          onClick={() => void run(() => hockeySeasonService.addDivision(season.id, division.id, division.name, season.divisions.length + 1))}
                        >
                          {t('common.add', 'Add')}
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}

          {activeTab === 'teams' && (
            <div className="teams-management">
              <ErrorPopup message={error} />
              {season.divisions.length === 0 ? (
                <div className="tm-empty-state">
                  <i className="fas fa-layer-group"></i>
                  <h4>{t('hockey.seasons.addDivisionsFirst', 'Add divisions first')}</h4>
                  <p>{t('hockey.seasons.addDivisionsFirstDesc', 'You need at least one division in this season before you can manage teams.')}</p>
                  <button type="button" className="btn btn-primary" onClick={() => setActiveTab('divisions')}>
                    <i className="fas fa-plus"></i> {t('hockey.seasons.goToDivisions', 'Go to Manage Divisions')}
                  </button>
                </div>
              ) : (
                <>
                  <div className="tm-division-selector">
                    <span className="tm-division-selector__label">
                      {t('hockey.seasons.addingTeamsTo', 'Managing teams for:')}
                    </span>
                    <div className="tm-division-selector__options">
                      {season.divisions.map((division) => (
                        <button
                          key={division.id}
                          type="button"
                          className={`tm-division-pill ${selectedDivisionId === division.id ? 'active' : ''}`}
                          onClick={() => {
                            setSelectedDivisionId(division.id);
                            setSelectedTeamIds(new Set());
                          }}
                        >
                          {division.name}
                          <span className="tm-division-pill__badge">{division.teams.length}</span>
                        </button>
                      ))}
                    </div>
                  </div>
                  {selectedDivisionId && (
                    <>
                      <div className="tm-section">
                        <div className="tm-section__header">
                          <h4>
                            <i className="fas fa-users"></i>
                            {t('hockey.seasons.teamsInDivision', 'Teams in {{division}}', { division: selectedDivision?.name ?? '' })}
                          </h4>
                          <span className="tm-section__count">
                            {teamsInSelectedDivision.length} {t('hockey.seasons.teams', 'teams')}
                          </span>
                        </div>
                        {teamsInSelectedDivision.length === 0 ? (
                          <div className="tm-section__empty">
                            <p>{t('hockey.seasons.noTeamsInDivision', 'No teams in this division yet. Use the table below to add teams.')}</p>
                          </div>
                        ) : (
                          <div className="tm-team-grid">
                            {teamsInSelectedDivision.map((team) => (
                              <div key={team.id} className="tm-team-chip">
                                <div className="tm-team-chip__info">
                                  <span className="tm-team-chip__name">{team.name}</span>
                                  <span className="tm-team-chip__club">{clubNames.get(team.clubId) ?? ''}</span>
                                </div>
                                <button
                                  type="button"
                                  className="tm-team-chip__remove"
                                  onClick={() => void run(() => hockeySeasonService.removeTeam(season.id, team.id))}
                                  disabled={saving}
                                  title={t('common.remove', 'Remove')}
                                >
                                  <i className="fas fa-times"></i>
                                </button>
                              </div>
                            ))}
                          </div>
                        )}
                      </div>
                      <div className="tm-section">
                        <div className="tm-section__header">
                          <h4>
                            <i className="fas fa-plus-circle"></i>
                            {t('hockey.seasons.addTeams', 'Add Teams')}
                          </h4>
                        </div>
                        <div className="tm-filters">
                          <div className="tm-filters__search">
                            <i className="fas fa-search"></i>
                            <input
                              type="text"
                              placeholder={t('hockey.seasons.searchTeams', 'Search teams by name...')}
                              value={searchTerm}
                              onChange={(event) => setSearchTerm(event.target.value)}
                            />
                            {searchTerm && (
                              <button type="button" className="tm-filters__clear" onClick={() => setSearchTerm('')}>
                                <i className="fas fa-times"></i>
                              </button>
                            )}
                          </div>
                          <select
                            value={teamCategory}
                            onChange={(event) => setTeamCategory(event.target.value as HockeyTeamCategory | '')}
                            className="tm-filters__category"
                          >
                            <option value="">{t('hockey.seasons.allCategories', 'All Categories')}</option>
                            {HOCKEY_TEAM_CATEGORIES.map((category) => (
                              <option key={category} value={category}>
                                {t(`hockey.teams.categories.${category}`, category)}
                              </option>
                            ))}
                          </select>
                        </div>
                        {selectedTeamIds.size > 0 && (
                          <div className="tm-action-bar">
                            <span>{t('hockey.seasons.selectedCount', '{{count}} team(s) selected', { count: selectedTeamIds.size })}</span>
                            <button
                              type="button"
                              className="btn btn-primary btn-sm"
                              disabled={saving}
                              onClick={() => {
                                const ids = [...selectedTeamIds];
                                setSelectedTeamIds(new Set());
                                void run(async () => {
                                  let latest = await hockeySeasonService.getById(season.id);
                                  for (const teamId of ids) {
                                    let competitionTeamId = latest.teams.find((item) => item.teamId === teamId)?.id;
                                    if (!competitionTeamId) {
                                      await hockeySeasonService.addTeam(season.id, teamId);
                                      latest = await hockeySeasonService.getById(season.id);
                                      competitionTeamId = latest.teams.find((item) => item.teamId === teamId)?.id;
                                    }
                                    if (competitionTeamId && selectedDivisionId) {
                                      latest = await hockeySeasonService.addTeamToDivision(season.id, selectedDivisionId, competitionTeamId);
                                    }
                                  }
                                });
                              }}
                            >
                              {saving
                                ? <><i className="fas fa-spinner fa-spin"></i> {t('common.adding', 'Adding...')}</>
                                : <><i className="fas fa-plus"></i> {t('hockey.seasons.addSelectedToDivision', 'Add to {{division}}', { division: selectedDivision?.name ?? '' })}</>}
                            </button>
                            <button type="button" className="tm-action-bar__clear" onClick={() => setSelectedTeamIds(new Set())}>
                              {t('common.clearSelection', 'Clear')}
                            </button>
                          </div>
                        )}
                        {availableTeamsNotInSeason.length === 0 ? (
                          <div className="tm-section__empty">
                            <p>
                              {searchTerm || teamCategory
                                ? t('hockey.seasons.noMatchingTeams', 'No teams match your search criteria.')
                                : t('hockey.seasons.noAvailableTeams', 'No teams available.')}
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
                                        checked={availableTeamsNotInSeason.length > 0 && selectedTeamIds.size === availableTeamsNotInSeason.length}
                                        onChange={() => {
                                          if (selectedTeamIds.size === availableTeamsNotInSeason.length) {
                                            setSelectedTeamIds(new Set());
                                          } else {
                                            setSelectedTeamIds(new Set(availableTeamsNotInSeason.map((team) => team.id)));
                                          }
                                        }}
                                        disabled={availableTeamsNotInSeason.length === 0}
                                        title={t('common.selectAll', 'Select all')}
                                      />
                                    </th>
                                    <th>{t('hockey.teams.table.name', 'Team')}</th>
                                    <th>{t('hockey.teams.table.club', 'Club')}</th>
                                    <th>{t('hockey.teams.category', 'Category')}</th>
                                    <th className="tm-table__status">{t('common.status', 'Status')}</th>
                                    <th className="tm-table__actions"></th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {pagedAvailableTeams.map((team) => {
                                    const isInSeason = assignedIds.has(team.id);
                                    const isSelected = selectedTeamIds.has(team.id);
                                    return (
                                      <tr key={team.id} className={`${isSelected ? 'selected' : ''} ${isInSeason ? 'in-season' : ''}`}>
                                        <td className="tm-table__checkbox">
                                          <input
                                            type="checkbox"
                                            checked={isSelected}
                                            onChange={() => {
                                              setSelectedTeamIds((prev) => {
                                                const next = new Set(prev);
                                                if (next.has(team.id)) {
                                                  next.delete(team.id);
                                                } else {
                                                  next.add(team.id);
                                                }
                                                return next;
                                              });
                                            }}
                                            disabled={isInSeason || saving}
                                          />
                                        </td>
                                        <td>
                                          <span className="tm-table__team-name">{team.name}</span>
                                          {team.shortName && <span className="tm-table__short-name">({team.shortName})</span>}
                                        </td>
                                        <td className="tm-table__club">{clubNames.get(team.clubId) ?? '-'}</td>
                                        <td>
                                          <span className="tm-category-badge">
                                            {t(`hockey.teams.categories.${team.teamCategory}`, team.teamCategory)}
                                          </span>
                                        </td>
                                        <td className="tm-table__status">
                                          {isInSeason ? (
                                            <span className="tm-status-badge tm-status-badge--added">
                                              <i className="fas fa-check"></i> {t('hockey.seasons.inSeason', 'In season')}
                                            </span>
                                          ) : null}
                                        </td>
                                        <td className="tm-table__actions">
                                          {!isInSeason && (
                                            <button
                                              type="button"
                                              className="btn btn-primary btn-sm"
                                              disabled={saving}
                                              onClick={() => {
                                                void run(async () => {
                                                  await hockeySeasonService.addTeam(season.id, team.id);
                                                  const latest = await hockeySeasonService.getById(season.id);
                                                  const competitionTeamId = latest.teams.find((item) => item.teamId === team.id)?.id;
                                                  if (competitionTeamId && selectedDivisionId) {
                                                    await hockeySeasonService.addTeamToDivision(season.id, selectedDivisionId, competitionTeamId);
                                                  }
                                                });
                                              }}
                                            >
                                              <i className="fas fa-plus"></i> {t('common.add', 'Add')}
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
                              currentPage={teamsPage}
                              totalPages={teamsTotalPages}
                              totalCount={availableTeamsNotInSeason.length}
                              pageSize={teamsPageSize}
                              onPageChange={setTeamsPage}
                              onPageSizeChange={(size) => {
                                setTeamsPageSize(size);
                                setTeamsPage(1);
                              }}
                              pageSizeOptions={[10, 25, 50]}
                              className="compact"
                            />
                          </>
                        )}
                      </div>
                    </>
                  )}
                </>
              )}
            </div>
          )}
        </div>
      </div>
    </PageTemplate>
  );
}

export default EditHockeySeasonPage;
