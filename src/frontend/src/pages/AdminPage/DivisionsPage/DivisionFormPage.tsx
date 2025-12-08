import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import Button from '../../../components/Button/Button';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import { divisionService } from '../../../api/common/divisionService';
import type { DivisionType } from '../../../types/common/divisionType';
import type { DivisionFormState } from '../../../types/common/divisionUiTypes';
import { ACTIVE_SPORTS, SportsCategory, SPORT_LABELS } from '../../../types/common/sports';
import './DivisionFormPage.scss';

const defaultFormState: DivisionFormState = {
  name: '',
  description: '',
  level: '',
  sportType: '',
};

const DivisionFormPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { divisionId } = useParams<{ divisionId: string }>();

  const isEditMode = Boolean(divisionId);

  const [formState, setFormState] = useState<DivisionFormState>(defaultFormState);
  const [existingDivision, setExistingDivision] = useState<DivisionType | null>(null);
  const [loading, setLoading] = useState(isEditMode);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!divisionId) {
      return;
    }

    const fetchDivision = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await divisionService.getById(divisionId);
        const division = response.data;
        setExistingDivision(division);
        setFormState({
          name: division.name,
          description: division.description ?? '',
          level: division.level.toString(),
          sportType: division.sportType,
        });
      } catch (err) {
        console.error('Failed to load division', err);
        setError(
          err instanceof Error
            ? err.message
            : t('admin.divisions.errors.loadSingle', 'Failed to load division data.'),
        );
      } finally {
        setLoading(false);
      }
    };

    fetchDivision();
  }, [divisionId, t]);

  const handleInputChange = (field: keyof DivisionFormState, value: string | SportsCategory) => {
    setFormState((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      const payloadBase = {
        name: formState.name.trim(),
        description: formState.description.trim(),
        level: Number(formState.level),
      };

      if (Number.isNaN(payloadBase.level)) {
        throw new Error(t('admin.divisions.errors.levelRequired', 'Level must be a number.'));
      }

      if (isEditMode && divisionId) {
        await divisionService.update(divisionId, payloadBase);
      } else {
        if (!formState.sportType) {
          throw new Error(
            t('admin.divisions.errors.sportTypeRequired', 'Sport type is required.'),
          );
        }

        await divisionService.create({
          ...payloadBase,
          sportType: formState.sportType,
        });
      }

      navigate('/admin/divisions');
    } catch (err) {
      console.error('Failed to submit division form', err);
      setError(
        err instanceof Error
          ? err.message
          : t('admin.divisions.errors.save', 'Failed to save division. Please try again.'),
      );
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <PageTemplate title={t('admin.divisions.form.loading', 'Loading division...')}>
        <div className="division-form__loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  const pageTitle = isEditMode
    ? t('admin.divisions.editTitle', 'Edit division')
    : t('admin.divisions.createTitle', 'Create division');

  return (
    <PageTemplate title={pageTitle}>
      <div className="division-form-page">
        <div className="division-form-page__header">
          <div>
            <h2>{pageTitle}</h2>
            <p>
              {isEditMode
                ? t('admin.divisions.form.editSubtitle', 'Update the key details of this division.')
                : t('admin.divisions.form.createSubtitle', 'Provide the information for the new division.')}
            </p>
          </div>

          {isEditMode && existingDivision && (
            <div className="division-form-page__status">
              <span>{t('admin.divisions.table.status', 'Status')}:</span>
              <span
                className={`division-status ${existingDivision.isActive ? 'division-status--active' : 'division-status--inactive'}`}
              >
                {existingDivision.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
            </div>
          )}
        </div>

        <ErrorPopup message={error} />

        <form className="division-form" onSubmit={handleSubmit}>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="divisionName">{t('common.name', 'Name')}</label>
              <input
                id="divisionName"
                type="text"
                value={formState.name}
                onChange={(event) => handleInputChange('name', event.target.value)}
                required
                placeholder={t('admin.divisions.form.namePlaceholder', 'Division name')}
              />
            </div>

            <div className="form-group">
              <label htmlFor="divisionLevel">
                {t('admin.divisions.table.level', 'Level')}
                <span className="helper">({t('admin.divisions.form.levelHelper', '1 = highest')})</span>
              </label>
              <input
                id="divisionLevel"
                type="number"
                min={1}
                max={10}
                value={formState.level}
                onChange={(event) => handleInputChange('level', event.target.value)}
                required
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="divisionDescription">{t('common.description', 'Description')}</label>
            <textarea
              id="divisionDescription"
              rows={4}
              value={formState.description}
              onChange={(event) => handleInputChange('description', event.target.value)}
              required
              placeholder={t('admin.divisions.form.descriptionPlaceholder', 'Short summary of this division')}
            />
          </div>

          <div className="form-group">
            <label htmlFor="divisionSportType">{t('admin.divisions.table.sport', 'Sport')}</label>
            <select
              id="divisionSportType"
              value={formState.sportType}
              onChange={(event) =>
                handleInputChange('sportType', event.target.value as SportsCategory | '')
              }
              required
              disabled={isEditMode}
            >
              {!isEditMode && (
                <option value="">
                  {t('common.selectOption', 'Select sport')}
                </option>
              )}
              {ACTIVE_SPORTS.map((sport) => (
                <option key={sport} value={sport}>
                  {t(`sports.${sport.toLowerCase()}`, SPORT_LABELS[sport])}
                </option>
              ))}
            </select>
            {isEditMode && (
              <small className="field-hint">
                {t('admin.divisions.form.sportHint', 'Sport type cannot be changed for existing divisions.')}
              </small>
            )}
          </div>

          <div className="form-actions">
            <Button
              type="button"
              variant="ghost"
              onClick={() => navigate('/admin/divisions')}
              rounded="pill"
            >
              {t('common.cancel', 'Cancel')}
            </Button>
            <Button
              type="submit"
              isLoading={submitting}
              disabled={submitting}
              rounded="pill"
              className="submit-button"
            >
              {isEditMode ? t('common.saveChanges', 'Save changes') : t('common.create', 'Create')}
            </Button>
          </div>
        </form>
      </div>
    </PageTemplate>
  );
};

export default DivisionFormPage;

