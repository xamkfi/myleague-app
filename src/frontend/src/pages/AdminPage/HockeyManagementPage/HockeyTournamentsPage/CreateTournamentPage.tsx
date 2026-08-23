import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { hockeyTournamentService } from '../../../../api/hockey/hockeyTournamentService';
import { HOCKEY_TEAM_CATEGORIES, type HockeyTeamCategory } from '../../../../types/hockey/hockeyTypes';
import '../HockeyTeamsPage/CreateTeamPage.scss';

function CreateHockeyTournamentPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [venue, setVenue] = useState('');
  const [teamCategory, setTeamCategory] = useState<HockeyTeamCategory>('Adult');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: React.FormEvent): Promise<void> => {
    event.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const tournament = await hockeyTournamentService.create({
        name,
        startDate: new Date(startDate).toISOString(),
        endDate: new Date(endDate).toISOString(),
        venue: venue || undefined,
        teamCategory,
      });
      navigate(`/admin/hockey/tournaments/${tournament.id}/edit`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create tournament');
    } finally {
      setLoading(false);
    }
  };

  return (
    <PageTemplate title={t('hockey.tournaments.create', 'Create tournament')}>
      <div className="create-team-page">
        <div className="create-team-header">
          <h1>{t('hockey.tournaments.create', 'Create tournament')}</h1>
        </div>
        <ErrorPopup message={error} />
        <form className="create-team-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="name">{t('hockey.tournaments.name', 'Name')} *</label>
            <input id="name" value={name} onChange={(e) => setName(e.target.value)} required />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="start">{t('hockey.seasons.startDate', 'Start')}</label>
              <input id="start" type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} required />
            </div>
            <div className="form-group">
              <label htmlFor="end">{t('hockey.seasons.endDate', 'End')}</label>
              <input id="end" type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} required />
            </div>
          </div>
          <div className="form-group">
            <label htmlFor="venue">{t('hockey.tournaments.venue', 'Venue')}</label>
            <input id="venue" value={venue} onChange={(e) => setVenue(e.target.value)} />
          </div>
          <div className="form-group">
            <label htmlFor="category">{t('hockey.teams.category', 'Category')}</label>
            <select
              id="category"
              value={teamCategory}
              onChange={(event) => setTeamCategory(event.target.value as HockeyTeamCategory)}
            >
              {HOCKEY_TEAM_CATEGORIES.map((category) => (
                <option key={category} value={category}>
                  {t(`hockey.teams.categories.${category}`, category)}
                </option>
              ))}
            </select>
          </div>
          <div className="form-actions">
            <button type="button" className="cancel-button" onClick={() => navigate('/admin/hockey/tournaments')}>
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

export default CreateHockeyTournamentPage;
