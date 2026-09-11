import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import { clubService } from '../../../../api/common/clubService';
import {
  HOCKEY_TEAM_CATEGORIES,
  type HockeyTeamCategory,
  type UpdateHockeyTeamRequest,
} from '../../../../types/hockey/hockeyTypes';
import './EditTeamPage.scss';

function EditHockeyTeamPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id: teamId } = useParams<{ id: string }>();
  const [loading, setLoading] = useState(false);
  const [loadingTeam, setLoadingTeam] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [teamName, setTeamName] = useState('');
  const [clubName, setClubName] = useState('');
  const [form, setForm] = useState<UpdateHockeyTeamRequest>({
    name: '',
    shortName: '',
    teamCategory: 'Adult',
    homeArena: '',
    primaryJerseyColor: '#000000',
    secondaryJerseyColor: '',
  });

  const setField = (field: keyof UpdateHockeyTeamRequest, value: string): void => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const load = useCallback(async (): Promise<void> => {
    if (!teamId) {
      return;
    }
    try {
      setLoadingTeam(true);
      const team = await hockeyTeamService.getById(teamId);
      setTeamName(team.name);
      setForm({
        name: team.name,
        shortName: team.shortName,
        teamCategory: team.teamCategory,
        divisionId: team.divisionId,
        homeArena: team.homeArena,
        primaryJerseyColor: team.primaryJerseyColor || '#000000',
        secondaryJerseyColor: team.secondaryJerseyColor,
      });
      const clubs = await clubService.getAll().catch(() => []);
      setClubName(clubs.find((club) => club.id === team.clubId)?.name ?? '');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load team');
    } finally {
      setLoadingTeam(false);
    }
  }, [teamId]);

  useEffect(() => {
    void load();
  }, [load]);

  const handleSubmit = async (event: React.FormEvent): Promise<void> => {
    event.preventDefault();
    if (!teamId) {
      return;
    }
    setLoading(true);
    setError(null);
    try {
      await hockeyTeamService.update(teamId, {
        ...form,
        secondaryJerseyColor:
          form.secondaryJerseyColor && form.secondaryJerseyColor.length >= 2
            ? form.secondaryJerseyColor
            : undefined,
      });
      navigate('/admin/hockey/teams');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save team');
    } finally {
      setLoading(false);
    }
  };

  if (loadingTeam) {
    return (
      <PageTemplate title={t('common.loading', 'Loading...')}>
        <div className="edit-team-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('hockey.teams.editTeam', 'Edit Team')}>
      <div className="edit-team-page">
        <div className="edit-team-header">
          <h1>{t('hockey.teams.editTeam', 'Edit Team')}: {teamName}</h1>
        </div>
        <ErrorPopup message={error} />
        <form onSubmit={handleSubmit} className="edit-team-form">
          <div className="form-row name-row">
            <div className="form-group">
              <label htmlFor="teamName">{t('hockey.teams.name', 'Team Name')} *</label>
              <input
                id="teamName"
                type="text"
                value={form.name}
                onChange={(event) => setField('name', event.target.value)}
                required
                placeholder={t('hockey.teams.namePlaceholder', 'Enter team name')}
              />
            </div>
            <div className="form-group">
              <label htmlFor="teamShortName">{t('hockey.teams.shortName', 'Short Name / Acronym')}</label>
              <input
                id="teamShortName"
                type="text"
                value={form.shortName ?? ''}
                onChange={(event) => setField('shortName', event.target.value.toUpperCase())}
                placeholder={t('hockey.teams.shortNamePlaceholder', 'e.g., HIFK')}
                maxLength={4}
              />
            </div>
          </div>
          <div className="form-group">
            <label htmlFor="clubId">{t('hockey.teams.club', 'Club')}</label>
            <input id="clubId" type="text" value={clubName} disabled />
          </div>
          <div className="form-group">
            <label htmlFor="category">{t('hockey.teams.category', 'Category')} *</label>
            <select
              id="category"
              value={form.teamCategory}
              onChange={(event) => setField('teamCategory', event.target.value as HockeyTeamCategory)}
              required
            >
              {HOCKEY_TEAM_CATEGORIES.map((category) => (
                <option key={category} value={category}>{category}</option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="homeArena">{t('hockey.teams.homeArena', 'Home Arena')} *</label>
            <input
              id="homeArena"
              type="text"
              value={form.homeArena ?? ''}
              onChange={(event) => setField('homeArena', event.target.value)}
              required
              placeholder={t('hockey.teams.homeArenaPlaceholder', 'Enter home arena')}
            />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="primaryColor">{t('hockey.teams.primary', 'Primary Jersey Color')} *</label>
              <div className="color-input-group">
                <input
                  id="primaryColor"
                  type="color"
                  value={form.primaryJerseyColor || '#000000'}
                  onChange={(event) => setField('primaryJerseyColor', event.target.value)}
                  required
                />
                <input
                  type="text"
                  value={form.primaryJerseyColor ?? ''}
                  onChange={(event) => setField('primaryJerseyColor', event.target.value)}
                  placeholder="#000000"
                />
              </div>
            </div>
            <div className="form-group">
              <label htmlFor="secondaryColor">{t('hockey.teams.secondary', 'Secondary Jersey Color')}</label>
              <div className="color-input-group">
                <input
                  id="secondaryColor"
                  type="color"
                  value={form.secondaryJerseyColor || '#ffffff'}
                  onChange={(event) => setField('secondaryJerseyColor', event.target.value)}
                />
                <input
                  type="text"
                  value={form.secondaryJerseyColor ?? ''}
                  onChange={(event) => setField('secondaryJerseyColor', event.target.value)}
                  placeholder={t('hockey.teams.optional', 'Optional')}
                />
              </div>
            </div>
          </div>
          <div className="form-actions">
            <button type="button" onClick={() => navigate('/admin/hockey/teams')} className="cancel-button" disabled={loading}>
              {t('common.cancel', 'Cancel')}
            </button>
            <button type="submit" disabled={loading} className="submit-button">
              {loading ? t('common.saving', 'Saving...') : t('common.save', 'Save Changes')}
            </button>
          </div>
        </form>
      </div>
    </PageTemplate>
  );
}

export default EditHockeyTeamPage;
