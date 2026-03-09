import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import ReactQuill from 'react-quill';
import 'react-quill/dist/quill.snow.css';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';
import { floorballTournamentService } from '../../../../../api/floorball/floorballTournamentService';
import type { CreateFloorballTournamentRequest } from '../../../../../types/floorball/floorballTypes';
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

const CreateTournamentPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [formData, setFormData] = useState<CreateFloorballTournamentRequest>({
    name: '',
    startDate: new Date().toISOString().split('T')[0],
    endDate: new Date().toISOString().split('T')[0],
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

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'number' ? parseInt(value, 10) || 0 : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      if (!formData.name.trim()) {
        throw new Error(t('tournament.validation.nameRequired', 'Tournament name is required'));
      }
      if (!formData.startDate || !formData.endDate) {
        throw new Error(t('tournament.validation.datesRequired', 'Start and end dates are required'));
      }
      if (new Date(formData.endDate) < new Date(formData.startDate)) {
        throw new Error(t('tournament.validation.endAfterStart', 'End date must be after start date'));
      }

      const result = await floorballTournamentService.create(formData);

      if (result?.data?.id) {
        navigate(`/admin/floorball/tournaments/${result.data.id}/edit`, { replace: true });
      } else {
        navigate('/admin/floorball/tournaments');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create tournament');
    } finally {
      setLoading(false);
    }
  };

  return (
    <PageTemplate title={t('tournament.create', 'Create Tournament')}>
      <div className="edit-season-container">
        <div className="tab-navigation">
          <button className="tab-button active">
            {t('tournament.details', 'Tournament Details')}
          </button>
          <button className="tab-button" disabled title={t('tournament.saveFirst', 'Save tournament first to manage groups')}>
            {t('tournament.manageGroups', 'Manage Groups')}
          </button>
        </div>

        <div className="edit-season-content">
          <form onSubmit={handleSubmit} className="edit-season-form">
            <ErrorPopup message={error} />

            <div className="form-section">
              <h3 className="form-section__title">{t('tournament.sections.basicInfo', 'Basic Information')}</h3>
              <div className="form-group">
                <label htmlFor="name">{t('tournament.fields.name', 'Name')} *</label>
                <input type="text" id="name" name="name" value={formData.name} onChange={handleInputChange} required disabled={loading} placeholder={t('tournament.placeholders.name', 'e.g. Duuniturnaus 2026')} />
              </div>
              <div className="form-group">
                <label htmlFor="location">{t('tournament.fields.location', 'Location')}</label>
                <input type="text" id="location" name="location" value={formData.location || ''} onChange={handleInputChange} disabled={loading} placeholder={t('tournament.placeholders.location', 'e.g. Liikuntahalli')} />
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
                {loading ? t('common.creating', 'Creating...') : t('common.create', 'Create')}
              </button>
            </div>
          </form>
        </div>
      </div>
    </PageTemplate>
  );
};

export default CreateTournamentPage;
