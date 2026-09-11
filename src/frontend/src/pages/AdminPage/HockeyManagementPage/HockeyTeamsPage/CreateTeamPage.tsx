import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import SearchableInfiniteDropdown from '../../../../components/SearchableInfiniteDropdown/SearchableInfiniteDropdown';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import { clubService } from '../../../../api/common/clubService';
import { HOCKEY_TEAM_CATEGORIES, type CreateHockeyTeamRequest, type HockeyTeamCategory } from '../../../../types/hockey/hockeyTypes';
import './CreateTeamPage.scss';

function CreateHockeyTeamPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState<CreateHockeyTeamRequest>({
    name: '',
    shortName: '',
    clubId: '',
    homeArena: '',
    primaryJerseyColor: '#000000',
    secondaryJerseyColor: '',
    teamCategory: 'Adult',
  });

  const searchClubs = async (query: string, page: number) => {
    const response = await clubService.getPaged(page, 50);
    const needle = query.trim().toLowerCase();
    const clubs = needle
      ? response.data.filter((club) => club.name.toLowerCase().includes(needle))
      : response.data;
    return {
      data: clubs.map((club) => ({ id: club.id, name: club.name })),
      pagination: {
        hasNextPage: response.pagination.hasNextPage && !needle,
        totalCount: needle ? clubs.length : response.pagination.totalCount,
      },
    };
  };

  const setField = (field: keyof CreateHockeyTeamRequest, value: string): void => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (event: React.FormEvent): Promise<void> => {
    event.preventDefault();
    setLoading(true);
    setError(null);
    try {
      await hockeyTeamService.create({
        name: form.name,
        clubId: form.clubId,
        teamCategory: form.teamCategory,
        homeArena: form.homeArena,
        primaryJerseyColor: form.primaryJerseyColor,
        shortName: form.shortName?.trim() || undefined,
        secondaryJerseyColor:
          form.secondaryJerseyColor && form.secondaryJerseyColor.length >= 2
            ? form.secondaryJerseyColor
            : undefined,
      });
      navigate('/admin/hockey/teams');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create team');
    } finally {
      setLoading(false);
    }
  };

  return (
    <PageTemplate title={t('hockey.teams.createNew', 'Create New Team')}>
      <div className="create-team-page">
        <div className="create-team-header">
          <h1>{t('hockey.teams.createNew', 'Create New Team')}</h1>
        </div>
        <ErrorPopup message={error} />
        <form onSubmit={handleSubmit} className="create-team-form">
          <div className="form-row name-row">
            <div className="form-group">
              <label htmlFor="teamName">{t('hockey.teams.name', 'Team Name')} *</label>
              <input id="teamName" value={form.name} onChange={(e) => setField('name', e.target.value)} required />
            </div>
            <div className="form-group">
              <label htmlFor="shortName">{t('hockey.teams.shortName', 'Short name')}</label>
              <input
                id="shortName"
                value={form.shortName ?? ''}
                maxLength={4}
                onChange={(e) => setField('shortName', e.target.value.toUpperCase())}
              />
            </div>
          </div>
          <div className="form-group">
            <label>{t('hockey.teams.club', 'Club')} *</label>
            <SearchableInfiniteDropdown
              placeholder={t('hockey.teams.selectClub', 'Select a club')}
              value={form.clubId}
              onChange={(value) => setField('clubId', value)}
              onSearch={searchClubs}
              emptyMessage={t('hockey.teams.noClubsFound', 'No clubs found')}
              searchPlaceholder={t('hockey.teams.searchClubs', 'Search clubs...')}
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="category">{t('hockey.teams.category', 'Category')} *</label>
            <select
              id="category"
              value={form.teamCategory}
              onChange={(e) => setField('teamCategory', e.target.value as HockeyTeamCategory)}
            >
              {HOCKEY_TEAM_CATEGORIES.map((category) => (
                <option key={category} value={category}>{category}</option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="homeArena">{t('hockey.teams.homeArena', 'Home Arena')}</label>
            <input id="homeArena" value={form.homeArena ?? ''} onChange={(e) => setField('homeArena', e.target.value)} />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="primaryColor">{t('hockey.teams.primary', 'Primary color')}</label>
              <div className="color-input-group">
                <input id="primaryColor" type="color" value={form.primaryJerseyColor ?? '#000000'} onChange={(e) => setField('primaryJerseyColor', e.target.value)} />
                <input type="text" value={form.primaryJerseyColor ?? ''} onChange={(e) => setField('primaryJerseyColor', e.target.value)} />
              </div>
            </div>
            <div className="form-group">
              <label htmlFor="secondaryColor">{t('hockey.teams.secondary', 'Secondary color')}</label>
              <div className="color-input-group">
                <input id="secondaryColor" type="color" value={form.secondaryJerseyColor || '#ffffff'} onChange={(e) => setField('secondaryJerseyColor', e.target.value)} />
                <input type="text" value={form.secondaryJerseyColor ?? ''} onChange={(e) => setField('secondaryJerseyColor', e.target.value)} />
              </div>
            </div>
          </div>
          <div className="form-actions">
            <button type="button" className="cancel-button" onClick={() => navigate('/admin/hockey/teams')} disabled={loading}>
              {t('common.cancel', 'Cancel')}
            </button>
            <button type="submit" className="submit-button" disabled={loading}>
              {loading ? t('common.creating', 'Creating...') : t('common.create', 'Create')}
            </button>
          </div>
        </form>
      </div>
    </PageTemplate>
  );
}

export default CreateHockeyTeamPage;
