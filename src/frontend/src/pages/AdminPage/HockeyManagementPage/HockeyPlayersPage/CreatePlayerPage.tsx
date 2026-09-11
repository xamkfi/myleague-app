import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import SearchableInfiniteDropdown from '../../../../components/SearchableInfiniteDropdown/SearchableInfiniteDropdown';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { personApi } from '../../../../api/admin/personApi';
import { hockeyPlayerService } from '../../../../api/hockey/hockeyPlayerService';
import { HOCKEY_POSITIONS, HOCKEY_SHOOTS, type HockeyPosition, type HockeyShoots } from '../../../../types/hockey/hockeyTypes';
import '../HockeyTeamsPage/CreateTeamPage.scss';

function CreateHockeyPlayerPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const createdPerson = (location.state as { newPersonCreated?: { id: string }; successMessage?: string } | null);
  const [personId, setPersonId] = useState(createdPerson?.newPersonCreated?.id ?? '');
  const [position, setPosition] = useState<HockeyPosition>('Center');
  const [shoots, setShoots] = useState<HockeyShoots>('Unknown');
  const [licenseNumber, setLicenseNumber] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const searchPersons = async (query: string, page: number) => {
    const response = query.trim()
      ? await personApi.search(query, page, 25)
      : await personApi.getAll(page, 25);
    return {
      data: response.data.map((person) => ({ id: person.id, name: person.fullName })),
      pagination: {
        hasNextPage: response.pagination.hasNextPage,
        totalCount: response.pagination.totalCount,
      },
    };
  };

  const handleSubmit = async (event: React.FormEvent): Promise<void> => {
    event.preventDefault();
    setLoading(true);
    setError(null);
    try {
      await hockeyPlayerService.create({
        personId,
        primaryPosition: position,
        shoots,
        licenseNumber: licenseNumber || undefined,
      });
      navigate('/admin/hockey/players');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create player');
    } finally {
      setLoading(false);
    }
  };

  return (
    <PageTemplate title={t('hockey.players.create', 'Create player')}>
      <div className="create-team-page">
        <div className="create-team-header">
          <h1>{t('hockey.players.create', 'Create player')}</h1>
        </div>
        <ErrorPopup message={error} />
        {createdPerson?.successMessage && <p>{createdPerson.successMessage}</p>}
        <form className="create-team-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label>{t('hockey.roster.person', 'Person')} *</label>
            <SearchableInfiniteDropdown
              placeholder={t('hockey.roster.searchPerson', 'Search person')}
              value={personId}
              onChange={setPersonId}
              onSearch={searchPersons}
              emptyMessage={t('hockey.roster.noPersons', 'No persons found')}
              searchPlaceholder={t('hockey.roster.searchPerson', 'Search person')}
              required
            />
            <button type="button" className="cancel-button" onClick={() => navigate('/admin/hockey/players/create-person')}>
              {t('hockey.players.createNewPerson', 'Create New Person')}
            </button>
          </div>
          <div className="form-group">
            <label htmlFor="position">{t('hockey.roster.position', 'Position')}</label>
            <select id="position" value={position} onChange={(e) => setPosition(e.target.value as HockeyPosition)}>
              {HOCKEY_POSITIONS.map((item) => <option key={item} value={item}>{item}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="shoots">{t('hockey.players.shoots', 'Shoots')}</label>
            <select id="shoots" value={shoots} onChange={(e) => setShoots(e.target.value as HockeyShoots)}>
              {HOCKEY_SHOOTS.map((item) => <option key={item} value={item}>{item}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="license">{t('hockey.players.license', 'License number')}</label>
            <input id="license" value={licenseNumber} onChange={(e) => setLicenseNumber(e.target.value)} />
          </div>
          <div className="form-actions">
            <button type="button" className="cancel-button" onClick={() => navigate('/admin/hockey/players')}>
              {t('common.cancel', 'Cancel')}
            </button>
            <button type="submit" className="submit-button" disabled={loading || !personId}>
              {loading ? t('common.creating', 'Creating...') : t('common.create', 'Create')}
            </button>
          </div>
        </form>
      </div>
    </PageTemplate>
  );
}

export default CreateHockeyPlayerPage;
