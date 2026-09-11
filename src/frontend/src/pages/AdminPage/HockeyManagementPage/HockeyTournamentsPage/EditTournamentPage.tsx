import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import Pagination from '../../../../components/Pagination';
import { hockeyTournamentService } from '../../../../api/hockey/hockeyTournamentService';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import { hockeyMatchService } from '../../../../api/hockey/hockeyMatchService';
import {
  HOCKEY_TEAM_CATEGORIES,
  type HockeyMatchDto,
  type HockeyTeamCategory,
  type HockeyTeamDto,
  type HockeyTournamentDto,
} from '../../../../types/hockey/hockeyTypes';
import { loadClubNameMap, loadTeamNameMap } from '../../../../utils/hockeyLookups';
import MatchTable from '../MatchManagementPage/components/MatchTable/MatchTable';
import '../HockeySeasonsPage/EditSeasonPage.scss';

type TournamentTab = 'details' | 'groups' | 'teams' | 'matches';
const VALID_TABS: TournamentTab[] = ['details', 'groups', 'teams', 'matches'];

function EditHockeyTournamentPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { competitionId } = useParams<{ competitionId: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  const tabParam = searchParams.get('tab');
  const activeTab: TournamentTab = VALID_TABS.includes(tabParam as TournamentTab)
    ? (tabParam as TournamentTab)
    : 'details';
  const setActiveTab = (tab: TournamentTab): void => {
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev);
      if (tab === 'details') {
        next.delete('tab');
      } else {
        next.set('tab', tab);
      }
      return next;
    }, { replace: true });
  };

  const [tournament, setTournament] = useState<HockeyTournamentDto | null>(null);
  const [teams, setTeams] = useState<HockeyTeamDto[]>([]);
  const [teamNames, setTeamNames] = useState<Map<string, string>>(new Map());
  const [clubNames, setClubNames] = useState<Map<string, string>>(new Map());
  const [matches, setMatches] = useState<HockeyMatchDto[]>([]);
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [venue, setVenue] = useState('');
  const [competitionCategory, setCompetitionCategory] = useState<HockeyTeamCategory>('Adult');
  const [groupName, setGroupName] = useState('');
  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [teamCategory, setTeamCategory] = useState<HockeyTeamCategory | ''>('');
  const [selectedTeamIds, setSelectedTeamIds] = useState<Set<string>>(new Set());
  const [teamsPage, setTeamsPage] = useState(1);
  const [teamsPageSize, setTeamsPageSize] = useState(10);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [loadingTournament, setLoadingTournament] = useState(true);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const load = useCallback(async (): Promise<void> => {
    if (!competitionId) {
      return;
    }
    const [loaded, teamList, matchList, clubs] = await Promise.all([
      hockeyTournamentService.getById(competitionId),
      hockeyTeamService.getAll(),
      hockeyMatchService.getByCompetition(competitionId).catch(() => [] as HockeyMatchDto[]),
      loadClubNameMap().catch(() => new Map<string, string>()),
    ]);
    setTournament(loaded);
    setTeams(teamList);
    setTeamNames(await loadTeamNameMap(teamList));
    setClubNames(clubs);
    setMatches(matchList);
    setName(loaded.name);
    setStartDate(loaded.startDate.slice(0, 10));
    setEndDate(loaded.endDate.slice(0, 10));
    setVenue(loaded.venue ?? '');
    setCompetitionCategory(loaded.teamCategory ?? 'Adult');
  }, [competitionId]);

  useEffect(() => {
    void load()
      .catch((err) => setError(err instanceof Error ? err.message : t('hockey.tournaments.errors.loadFailed', 'Failed to load tournament')))
      .finally(() => setLoadingTournament(false));
  }, [load, t]);

  useEffect(() => {
    if (tournament?.groups.length && !selectedGroupId) {
      setSelectedGroupId(tournament.groups[0].id);
    }
  }, [tournament, selectedGroupId]);

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
      setError(err instanceof Error ? err.message : t('hockey.tournaments.errors.updateFailed', 'Operation failed'));
    } finally {
      setSaving(false);
    }
  };

  const assignedIds = useMemo(() => new Set((tournament?.teams ?? []).map((item) => item.teamId)), [tournament]);
  const selectedGroup = tournament?.groups.find((group) => group.id === selectedGroupId) ?? null;

  const teamsInSelectedGroup = useMemo(() => {
    if (!tournament || !selectedGroup) {
      return [] as HockeyTeamDto[];
    }
    return selectedGroup.teams
      .map((member) => {
        const membership = tournament.teams.find((item) => item.id === member.competitionTeamId);
        return membership ? teams.find((team) => team.id === membership.teamId) : undefined;
      })
      .filter((team): team is HockeyTeamDto => Boolean(team));
  }, [tournament, selectedGroup, teams]);

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

  const availableTeamsNotInTournament = filteredAvailableTeams.filter((team) => !assignedIds.has(team.id));
  const teamsTotalPages = Math.max(1, Math.ceil(availableTeamsNotInTournament.length / teamsPageSize));
  const pagedAvailableTeams = availableTeamsNotInTournament.slice(
    (teamsPage - 1) * teamsPageSize,
    teamsPage * teamsPageSize,
  );

  if (loadingTournament) {
    return (
      <PageTemplate title={t('hockey.tournaments.edit', 'Edit Tournament')}>
        <div className="edit-season-loading"><p>{t('common.loading', 'Loading...')}</p></div>
      </PageTemplate>
    );
  }

  if (!tournament) {
    return (
      <PageTemplate title={t('hockey.tournaments.edit', 'Edit Tournament')}>
        <ErrorPopup message={error ?? t('hockey.tournaments.errors.notFound', 'Tournament not found')} />
      </PageTemplate>
    );
  }

  const openMatch = (match: HockeyMatchDto): void => {
    navigate(`/admin/hockey/matches/manage/${match.id}?returnTo=${encodeURIComponent(`/admin/hockey/tournaments/${tournament.id}/edit?tab=matches`)}`);
  };

  return (
    <PageTemplate title={t('hockey.tournaments.edit', 'Edit Tournament')}>
      {successMessage && (
        <div className="success-toast"><p>{successMessage}</p></div>
      )}
      <div className="edit-season-container">
        <div className="edit-season-back edit-season-back--with-actions">
          <button type="button" className="back-button" onClick={() => navigate('/admin/hockey/tournaments')}>
            <i className="fas fa-arrow-left"></i>
            {t('common.back', 'Back')}
          </button>
          <button
            type="button"
            className="back-button back-button--primary"
            onClick={() => navigate(`/admin/hockey/tournaments/matches?competitionId=${tournament.id}`)}
          >
            {t('hockey.tournaments.matches', 'Manage tournament matches')}
          </button>
        </div>
        <div className="tab-navigation">
          <button type="button" className={`tab-button ${activeTab === 'details' ? 'active' : ''}`} onClick={() => setActiveTab('details')}>
            {t('hockey.tournaments.tabs.details', 'Details')}
          </button>
          <button type="button" className={`tab-button ${activeTab === 'groups' ? 'active' : ''}`} onClick={() => setActiveTab('groups')}>
            {t('hockey.tournaments.tabs.groups', 'Groups')} ({tournament.groups.length})
          </button>
          <button type="button" className={`tab-button ${activeTab === 'teams' ? 'active' : ''}`} onClick={() => setActiveTab('teams')}>
            {t('hockey.tournaments.tabs.teams', 'Teams')} ({tournament.teams.length})
          </button>
          <button type="button" className={`tab-button ${activeTab === 'matches' ? 'active' : ''}`} onClick={() => setActiveTab('matches')}>
            {t('hockey.tournaments.tabs.matches', 'Matches')} ({matches.length})
          </button>
        </div>
        <div className="edit-season-content">
          {activeTab === 'details' && (
            <form
              className="edit-season-form"
              onSubmit={(event) => {
                event.preventDefault();
                void run(() => hockeyTournamentService.update(tournament.id, {
                  name,
                  startDate: new Date(startDate).toISOString(),
                  endDate: new Date(endDate).toISOString(),
                  venue: venue || undefined,
                  teamCategory: competitionCategory,
                }), t('hockey.tournaments.updated', 'Tournament updated successfully!'));
              }}
            >
              <ErrorPopup message={error} />
              <div className="form-section">
                <h3 className="form-section__title"><i className="fas fa-info-circle"></i> {t('hockey.tournaments.sections.basicInfo', 'Basic Information')}</h3>
                <div className="form-group">
                  <label htmlFor="name">{t('hockey.tournaments.fields.name', 'Name')} *</label>
                  <input id="name" value={name} onChange={(event) => setName(event.target.value)} required disabled={saving} />
                </div>
                <div className="form-group">
                  <label htmlFor="venue">{t('hockey.tournaments.venue', 'Venue')}</label>
                  <input id="venue" value={venue} onChange={(event) => setVenue(event.target.value)} disabled={saving} />
                </div>
                <div className="form-group">
                  <label htmlFor="tournament-category">{t('hockey.teams.category', 'Category')}</label>
                  <select
                    id="tournament-category"
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
                <h3 className="form-section__title"><i className="fas fa-calendar-alt"></i> {t('hockey.seasons.sections.schedule', 'Schedule')}</h3>
                <div className="form-row">
                  <div className="form-group">
                    <label htmlFor="start">{t('hockey.tournaments.fields.startDate', 'Start Date')} *</label>
                    <input id="start" type="date" value={startDate} onChange={(event) => setStartDate(event.target.value)} required disabled={saving} />
                  </div>
                  <div className="form-group">
                    <label htmlFor="end">{t('hockey.tournaments.fields.endDate', 'End Date')} *</label>
                    <input id="end" type="date" value={endDate} onChange={(event) => setEndDate(event.target.value)} required disabled={saving} min={startDate} />
                  </div>
                </div>
              </div>
              <div className="form-section">
                <h3 className="form-section__title"><i className="fas fa-flag"></i> {t('hockey.seasons.lifecycle', 'Lifecycle')}</h3>
                <p className="form-hint">{tournament.status} / {tournament.currentStage}</p>
                <div className="form-actions" style={{ justifyContent: 'flex-start', flexWrap: 'wrap' }}>
                  <button type="button" className="btn btn-secondary" disabled={saving} onClick={() => void run(() => hockeyTournamentService.publish(tournament.id))}>{t('hockey.tournaments.publish', 'Publish')}</button>
                  <button type="button" className="btn btn-secondary" disabled={saving} onClick={() => void run(() => hockeyTournamentService.openRegistration(tournament.id))}>{t('hockey.tournaments.openRegistration', 'Open registration')}</button>
                  <button type="button" className="btn btn-secondary" disabled={saving} onClick={() => void run(() => hockeyTournamentService.activate(tournament.id))}>{t('hockey.tournaments.activate', 'Activate')}</button>
                  <button type="button" className="btn btn-secondary" disabled={saving} onClick={() => void run(() => hockeyTournamentService.startGroupStage(tournament.id))}>{t('hockey.tournaments.startGroupStage', 'Start group stage')}</button>
                  <button type="button" className="btn btn-secondary" disabled={saving} onClick={() => void run(() => hockeyTournamentService.startPlayoffStage(tournament.id))}>{t('hockey.tournaments.startPlayoffs', 'Start playoffs')}</button>
                  <button type="button" className="btn btn-primary" disabled={saving} onClick={() => void run(() => hockeyTournamentService.complete(tournament.id))}>{t('hockey.tournaments.complete', 'Complete')}</button>
                </div>
              </div>
              <div className="form-actions">
                <button type="button" className="btn btn-secondary" onClick={() => navigate('/admin/hockey/tournaments')} disabled={saving}>{t('common.cancel', 'Cancel')}</button>
                <button type="submit" className="btn btn-primary" disabled={saving}>{saving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}</button>
              </div>
            </form>
          )}

          {activeTab === 'groups' && (
            <div className="divisions-management">
              <ErrorPopup message={error} />
              <div className="current-divisions-section">
                <h4>{t('hockey.tournaments.addGroup', 'Add Group')}</h4>
                <div className="add-entity-row">
                  <input
                    type="text"
                    value={groupName}
                    onChange={(event) => setGroupName(event.target.value)}
                    placeholder={t('hockey.tournaments.groupName', 'e.g. Group A')}
                    disabled={saving}
                  />
                  <button
                    type="button"
                    className="btn btn-primary"
                    disabled={saving || !groupName.trim()}
                    onClick={() => {
                      const value = groupName.trim();
                      setGroupName('');
                      void run(() => hockeyTournamentService.createGroup(tournament.id, value));
                    }}
                  >
                    {saving
                      ? <><i className="fas fa-spinner fa-spin"></i> {t('common.adding', 'Adding...')}</>
                      : <><i className="fas fa-plus"></i> {t('common.add', 'Add')}</>}
                  </button>
                </div>
              </div>
              <div className="available-divisions-section">
                <h4>{t('hockey.tournaments.currentGroups', 'Current Groups')} ({tournament.groups.length})</h4>
                {tournament.groups.length === 0 ? (
                  <p className="no-divisions">{t('hockey.tournaments.noGroupsYet', 'No groups yet. Add one above.')}</p>
                ) : (
                  <div className="divisions-list">
                    {tournament.groups.map((group) => (
                      <div key={group.id} className="division-item">
                        <div className="division-info">
                          <span className="division-name">{group.name}</span>
                          <span className="division-team-count">
                            {t('hockey.tournaments.teamCount', '{{count}} team(s)', { count: group.teams.length })}
                          </span>
                        </div>
                        <button
                          type="button"
                          className="btn btn-danger btn-sm"
                          disabled={saving}
                          onClick={() => void run(() => hockeyTournamentService.deleteGroup(tournament.id, group.id))}
                        >
                          <i className="fas fa-trash-alt"></i>
                          {t('common.remove', 'Remove')}
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
              {tournament.groups.length === 0 ? (
                <div className="tm-empty-state">
                  <i className="fas fa-layer-group"></i>
                  <h4>{t('hockey.tournaments.addGroupsFirst', 'Add groups first')}</h4>
                  <p>{t('hockey.tournaments.addGroupsFirstDesc', 'You need at least one group in this tournament before you can manage teams.')}</p>
                  <button type="button" className="btn btn-primary" onClick={() => setActiveTab('groups')}>
                    <i className="fas fa-plus"></i> {t('hockey.tournaments.goToGroups', 'Go to Manage Groups')}
                  </button>
                </div>
              ) : (
                <>
                  <div className="tm-division-selector">
                    <span className="tm-division-selector__label">
                      {t('hockey.tournaments.addingTeamsTo', 'Managing teams for:')}
                    </span>
                    <div className="tm-division-selector__options">
                      {tournament.groups.map((group) => (
                        <button
                          key={group.id}
                          type="button"
                          className={`tm-division-pill ${selectedGroupId === group.id ? 'active' : ''}`}
                          onClick={() => {
                            setSelectedGroupId(group.id);
                            setSelectedTeamIds(new Set());
                          }}
                        >
                          {group.name}
                          <span className="tm-division-pill__badge">{group.teams.length}</span>
                        </button>
                      ))}
                    </div>
                  </div>
                  {selectedGroup && (
                    <>
                      <div className="tm-section">
                        <div className="tm-section__header">
                          <h4>
                            <i className="fas fa-users"></i>
                            {t('hockey.tournaments.teamsInGroup', 'Teams in {{group}}', { group: selectedGroup.name })}
                          </h4>
                          <span className="tm-section__count">
                            {teamsInSelectedGroup.length} {t('hockey.tournaments.teams', 'teams')}
                          </span>
                        </div>
                        {teamsInSelectedGroup.length === 0 ? (
                          <div className="tm-section__empty">
                            <p>{t('hockey.tournaments.noTeamsInGroup', 'No teams in this group yet. Use the table below to add teams.')}</p>
                          </div>
                        ) : (
                          <div className="tm-team-grid">
                            {teamsInSelectedGroup.map((team) => {
                              const membership = tournament.teams.find((item) => item.teamId === team.id);
                              return (
                                <div key={team.id} className="tm-team-chip">
                                  <div className="tm-team-chip__info">
                                    <span className="tm-team-chip__name">{team.name}</span>
                                    <span className="tm-team-chip__club">{clubNames.get(team.clubId) ?? ''}</span>
                                  </div>
                                  <button
                                    type="button"
                                    className="tm-team-chip__remove"
                                    disabled={saving || !membership}
                                    onClick={() => {
                                      if (membership) {
                                        void run(() => hockeyTournamentService.removeTeamFromGroup(tournament.id, selectedGroup.id, membership.id));
                                      }
                                    }}
                                    title={t('common.remove', 'Remove')}
                                  >
                                    <i className="fas fa-times"></i>
                                  </button>
                                </div>
                              );
                            })}
                          </div>
                        )}
                      </div>
                      <div className="tm-section">
                        <div className="tm-section__header">
                          <h4>
                            <i className="fas fa-plus-circle"></i>
                            {t('hockey.tournaments.addTeams', 'Add Teams')}
                          </h4>
                        </div>
                        <div className="tm-filters">
                          <div className="tm-filters__search">
                            <i className="fas fa-search"></i>
                            <input
                              type="text"
                              placeholder={t('hockey.tournaments.searchTeams', 'Search teams by name...')}
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
                            <option value="">{t('hockey.tournaments.allCategories', 'All Categories')}</option>
                            {HOCKEY_TEAM_CATEGORIES.map((category) => (
                              <option key={category} value={category}>
                                {t(`hockey.teams.categories.${category}`, category)}
                              </option>
                            ))}
                          </select>
                        </div>
                        {selectedTeamIds.size > 0 && (
                          <div className="tm-action-bar">
                            <span>{t('hockey.tournaments.selectedCount', '{{count}} team(s) selected', { count: selectedTeamIds.size })}</span>
                            <button
                              type="button"
                              className="btn btn-primary btn-sm"
                              disabled={saving}
                              onClick={() => {
                                const ids = [...selectedTeamIds];
                                setSelectedTeamIds(new Set());
                                void run(async () => {
                                  let latest = await hockeyTournamentService.getById(tournament.id);
                                  for (const teamId of ids) {
                                    let competitionTeamId = latest.teams.find((item) => item.teamId === teamId)?.id;
                                    if (!competitionTeamId) {
                                      await hockeyTournamentService.addTeam(tournament.id, teamId);
                                      latest = await hockeyTournamentService.getById(tournament.id);
                                      competitionTeamId = latest.teams.find((item) => item.teamId === teamId)?.id;
                                    }
                                    if (competitionTeamId && selectedGroupId) {
                                      latest = await hockeyTournamentService.addTeamToGroup(tournament.id, selectedGroupId, competitionTeamId);
                                    }
                                  }
                                });
                              }}
                            >
                              {saving
                                ? <><i className="fas fa-spinner fa-spin"></i> {t('common.adding', 'Adding...')}</>
                                : <><i className="fas fa-plus"></i> {t('hockey.tournaments.addSelectedToGroup', 'Add to {{group}}', { group: selectedGroup.name })}</>}
                            </button>
                            <button type="button" className="tm-action-bar__clear" onClick={() => setSelectedTeamIds(new Set())}>
                              {t('common.clearSelection', 'Clear')}
                            </button>
                          </div>
                        )}
                        {availableTeamsNotInTournament.length === 0 ? (
                          <div className="tm-section__empty">
                            <p>
                              {searchTerm || teamCategory
                                ? t('hockey.tournaments.noMatchingTeams', 'No teams match your search criteria.')
                                : t('hockey.tournaments.noAvailableTeams', 'No teams available.')}
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
                                        onChange={() => {
                                          if (selectedTeamIds.size === availableTeamsNotInTournament.length) {
                                            setSelectedTeamIds(new Set());
                                          } else {
                                            setSelectedTeamIds(new Set(availableTeamsNotInTournament.map((team) => team.id)));
                                          }
                                        }}
                                        disabled={availableTeamsNotInTournament.length === 0}
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
                                    const isInTournament = assignedIds.has(team.id);
                                    const isSelected = selectedTeamIds.has(team.id);
                                    return (
                                      <tr key={team.id} className={`${isSelected ? 'selected' : ''} ${isInTournament ? 'in-season' : ''}`}>
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
                                            disabled={isInTournament || saving}
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
                                          {isInTournament ? (
                                            <span className="tm-status-badge tm-status-badge--added">
                                              <i className="fas fa-check"></i> {t('hockey.tournaments.inTournament', 'In tournament')}
                                            </span>
                                          ) : null}
                                        </td>
                                        <td className="tm-table__actions">
                                          {!isInTournament && (
                                            <button
                                              type="button"
                                              className="btn btn-primary btn-sm"
                                              disabled={saving}
                                              onClick={() => {
                                                void run(async () => {
                                                  await hockeyTournamentService.addTeam(tournament.id, team.id);
                                                  const latest = await hockeyTournamentService.getById(tournament.id);
                                                  const competitionTeamId = latest.teams.find((item) => item.teamId === team.id)?.id;
                                                  if (competitionTeamId && selectedGroupId) {
                                                    await hockeyTournamentService.addTeamToGroup(tournament.id, selectedGroupId, competitionTeamId);
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
                              totalCount={availableTeamsNotInTournament.length}
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

          {activeTab === 'matches' && (
            <div>
              <ErrorPopup message={error} />
              <MatchTable
                matches={matches}
                teamNames={teamNames}
                competitionNames={new Map([[tournament.id, tournament.name]])}
                loading={false}
                hideActions
                onLiveMatch={openMatch}
                onEditMatch={(match) => navigate(`/admin/hockey/matches/${match.id}/edit`)}
                onOpenMatch={openMatch}
                onStartMatch={openMatch}
                onCancelMatch={() => undefined}
                onReactivateMatch={() => undefined}
              />
            </div>
          )}
        </div>
      </div>
    </PageTemplate>
  );
}

export default EditHockeyTournamentPage;
