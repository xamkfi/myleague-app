import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import TeamCategoryPicker from '../../../../components/TeamCategoryPicker/TeamCategoryPicker';
import { useAudience } from '../../../../context/AudienceContext';
import { hockeySeasonService } from '../../../../api/hockey/hockeySeasonService';
import { divisionService } from '../../../../api/common/divisionService';
import { SportsCategory } from '../../../../types/common/sports';
import { HOCKEY_STANDING_POINT_DEFAULTS, type HockeySeasonPointSettings, type HockeyTeamCategory } from '../../../../types/hockey/hockeyTypes';
import HockeySeasonPointFields from './HockeySeasonPointFields';
import SeasonStandingsSettings from '../../../../components/SeasonStandingsSettings/SeasonStandingsSettings';
import { DEFAULT_STANDING_SORT, STANDING_SORT_OPTIONS } from '../../../../components/SeasonStandingsSettings/standingSort';
import '../HockeyTeamsPage/CreateTeamPage.scss';

function CreateHockeySeasonPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [seasonCode, setSeasonCode] = useState('');
  const [logoUrl, setLogoUrl] = useState('');
  const { audience } = useAudience();
  // Starts from the group the admin is browsing; the picker at the top makes it explicit.
  const [teamCategory, setTeamCategory] = useState<HockeyTeamCategory>(audience.teamCategory);
  const [selectedDivisionIds, setSelectedDivisionIds] = useState<string[]>([]);
  const [teamsAdvancing, setTeamsAdvancing] = useState(0);
  const [rankingCriteria, setRankingCriteria] = useState<string[]>([...DEFAULT_STANDING_SORT]);
  const [points, setPoints] = useState<HockeySeasonPointSettings>({ ...HOCKEY_STANDING_POINT_DEFAULTS });
  const [divisions, setDivisions] = useState<Array<{ id: string; name: string }>>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    void divisionService.getBySportType(SportsCategory.Icehockey, true).then((response) => {
      setDivisions(response.data.map((item) => ({ id: item.id, name: item.name })));
    }).catch(() => undefined);
  }, []);

  const toggleDivision = (id: string): void => {
    setSelectedDivisionIds((prev) => (prev.includes(id) ? prev.filter((item) => item !== id) : [...prev, id]));
  };

  const handleSubmit = async (event: React.FormEvent): Promise<void> => {
    event.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const season = await hockeySeasonService.create({
        name,
        startDate: new Date(startDate).toISOString(),
        endDate: new Date(endDate).toISOString(),
        seasonCode: seasonCode || undefined,
        teamCategory,
        logoUrl: logoUrl || undefined,
        teamsAdvancing,
        rankingCriteria,
        ...points,
      });
      for (const [index, divisionId] of selectedDivisionIds.entries()) {
        const division = divisions.find((item) => item.id === divisionId);
        await hockeySeasonService.addDivision(season.id, divisionId, division?.name ?? 'Division', index + 1);
      }
      navigate(`/admin/hockey/seasons/${season.id}/edit`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create season');
    } finally {
      setLoading(false);
    }
  };

  return (
    <PageTemplate title={t('hockey.seasons.create', 'Create season')}>
      <div className="create-team-page">
        <div className="create-team-header">
          <h1>{t('hockey.seasons.create', 'Create season')}</h1>
        </div>
        <ErrorPopup message={error} />
        <form className="create-team-form" onSubmit={handleSubmit}>
          <TeamCategoryPicker
            name="create-season-group"
            value={teamCategory}
            onChange={setTeamCategory}
            hint={t('seasonGroup.createHint')}
            disabled={loading}
          />
          <div className="form-group">
            <label htmlFor="name">{t('hockey.seasons.name', 'Name')} *</label>
            <input id="name" value={name} onChange={(e) => setName(e.target.value)} required />
          </div>
          <div className="form-group">
            <label htmlFor="logoUrl">{t('competition.logoUrl', 'Logo URL')}</label>
            <input id="logoUrl" type="url" value={logoUrl} onChange={(e) => setLogoUrl(e.target.value)} placeholder="https://" />
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
            <label htmlFor="code">{t('hockey.seasons.code', 'Season code')}</label>
            <input id="code" value={seasonCode} onChange={(e) => setSeasonCode(e.target.value)} placeholder="2026-27" />
          </div>
          <div className="form-group">
            <label>{t('hockey.seasons.divisions', 'Divisions')}</label>
            {divisions.map((division) => (
              <label key={division.id} style={{ display: 'block', marginTop: '0.35rem' }}>
                <input type="checkbox" checked={selectedDivisionIds.includes(division.id)} onChange={() => toggleDivision(division.id)} />
                {' '}{division.name}
              </label>
            ))}
          </div>
          <HockeySeasonPointFields
            idPrefix="create-hockey-points"
            values={points}
            disabled={loading}
            onChange={setPoints}
          />
          <SeasonStandingsSettings
            idPrefix="create-hockey"
            teamsAdvancing={teamsAdvancing}
            criteria={rankingCriteria}
            options={STANDING_SORT_OPTIONS}
            allowAddRemove
            disabled={loading}
            onTeamsAdvancingChange={setTeamsAdvancing}
            onCriteriaChange={setRankingCriteria}
          />
          <div className="form-actions">
            <button type="button" className="cancel-button" onClick={() => navigate('/admin/hockey/seasons')}>
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

export default CreateHockeySeasonPage;
