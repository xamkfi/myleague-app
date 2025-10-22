import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import BackButton from '../../../../../components/BackButton/BackButton';
import type { CreateFloorballSeasonRequest } from '../../../../../api/floorball/floorballSeasonService';
import { floorballSeasonService } from '../../../../../api/floorball/floorballSeasonService';
import { useDivisions } from '../../../../../hooks/useDivisions';
import './CreateSeasonPage.scss';



export const CreateSeasonPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { divisions } = useDivisions();
  const [formData, setFormData] = useState<CreateFloorballSeasonRequest>({
    name: '',
    startDate: '',
    endDate: '',
    divisionId: ''
  });
  const [isActive, setIsActive] = useState(false);
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);

  // Cleanup timeout on unmount
  React.useEffect(() => {
    return () => {
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
    };
  }, [successTimeoutId]);

  const handleInputChange = async (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };



  const parseApiError = (error: unknown): string => {
    const errorMessage = error instanceof Error ? error.message : String(error);
    
    // Handle network errors
    if (errorMessage.includes('Failed to fetch') || errorMessage.includes('NetworkError')) {
      return t('floorball.seasons.errors.networkError', 'Network error. Please check your connection and try again.');
    }

    // Handle HTTP errors with specific status codes
    if (errorMessage.includes('HTTP 400')) {
      return t('floorball.seasons.errors.validationError', 'Invalid data provided. Please check your input and try again.');
    }
    
    if (errorMessage.includes('HTTP 409')) {
      return t('floorball.seasons.errors.conflictError', 'A season with overlapping dates already exists.');
    }
    
    if (errorMessage.includes('HTTP 500')) {
      return t('floorball.seasons.errors.serverError', 'Server error. Please try again later.');
    }

    // Handle specific business logic errors
    if (errorMessage.includes('overlapping dates') || errorMessage.includes('overlaps with')) {
      return t('floorball.seasons.errors.overlappingDates', 'A season already exists that overlaps with the specified dates.');
    }
    
    if (errorMessage.includes('Season name is required')) {
      return t('floorball.seasons.validation.nameRequired', 'Season name is required.');
    }
    
    if (errorMessage.includes('Season name cannot exceed 100 characters')) {
      return t('floorball.seasons.validation.nameTooLong', 'Season name cannot exceed 100 characters.');
    }
    
    if (errorMessage.includes('Division is required') || errorMessage.includes('Invalid division')) {
      return t('floorball.seasons.validation.divisionRequired', 'Please select a valid division.');
    }
    
    if (errorMessage.includes('Start date is required')) {
      return t('floorball.seasons.validation.startDateRequired', 'Start date is required.');
    }
    
    if (errorMessage.includes('End date is required')) {
      return t('floorball.seasons.validation.endDateRequired', 'End date is required.');
    }
    
    if (errorMessage.includes('End date must be after start date')) {
      return t('floorball.seasons.validation.endDateAfterStart', 'End date must be after start date.');
    }

    // Default error message
    return errorMessage || t('floorball.seasons.errors.createFailed', 'Failed to create season. Please try again.');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccessMessage(null);

    try {
      // Client-side validation
      if (!formData.name.trim()) {
        throw new Error(t('floorball.seasons.validation.nameRequired', 'Season name is required'));
      }

      if (formData.name.trim().length > 100) {
        throw new Error(t('floorball.seasons.validation.nameTooLong', 'Season name cannot exceed 100 characters'));
      }

      if (!formData.divisionId) {
        throw new Error(t('floorball.seasons.validation.divisionRequired', 'Division is required'));
      }

      if (!formData.startDate) {
        throw new Error(t('floorball.seasons.validation.startDateRequired', 'Start date is required'));
      }

      if (!formData.endDate) {
        throw new Error(t('floorball.seasons.validation.endDateRequired', 'End date is required'));
      }

      // Validate dates
      const startDate = new Date(formData.startDate);
      const endDate = new Date(formData.endDate);

      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        throw new Error(t('floorball.seasons.validation.invalidDate', 'Please enter valid dates'));
      }

      if (endDate <= startDate) {
        throw new Error(t('floorball.seasons.validation.endDateAfterStart', 'End date must be after start date'));
      }

      // Check if season is too long (more than 2 years)
      const maxDuration = 2 * 365 * 24 * 60 * 60 * 1000; // 2 years in milliseconds
      if (endDate.getTime() - startDate.getTime() > maxDuration) {
        throw new Error(t('floorball.seasons.validation.seasonTooLong', 'Season duration cannot exceed 2 years'));
      }

      await floorballSeasonService.create(formData);

      // Clear any existing timeout to prevent flickering
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }

      // Show success message
      const message = t('floorball.seasons.seasonCreated', 'Season "{{seasonName}}" has been created successfully!', { 
        seasonName: formData.name 
      });
      setSuccessMessage(message);
      
      // Auto-hide success message after 3 seconds and then navigate back
      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
        navigate('/admin/floorball/seasons');
      }, 3000);
      setSuccessTimeoutId(timeoutId);

      // Reset form
      setFormData({
        name: '',
        startDate: '',
        endDate: '',
        divisionId: ''
      });
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setLoading(false);
    }
  };

  // Set default dates (current year season)
  const currentYear = new Date().getFullYear();
  const defaultStartDate = `${currentYear}-01-01`;
  const defaultEndDate = `${currentYear}-12-31`;

  React.useEffect(() => {
    if (!formData.startDate) {
      setFormData(prev => ({ ...prev, startDate: defaultStartDate }));
    }
    if (!formData.endDate) {
      setFormData(prev => ({ ...prev, endDate: defaultEndDate }));
    }
  }, [defaultStartDate, defaultEndDate, formData.startDate, formData.endDate]);

  return (
    <PageTemplate title={t('floorball.seasons.create.title', 'Create New Season')}>
      {/* Floating Success Toast */}
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}

      <div className="create-season-container">
        {/* Back button */}
        <BackButton 
          to="/admin/floorball/seasons" 
          text={t('common.back', 'Back to Seasons')} 
        />

        <div className="create-season-form-container">
          <form onSubmit={handleSubmit} className="create-season-form">
            {error && (
              <div className="error-message">
                <i className="fas fa-exclamation-circle"></i>
                {error}
              </div>
            )}

            <div className="form-group">
              <label htmlFor="create-name">
                {t('floorball.seasons.fields.name', 'Name')} *
              </label>
              <input
                type="text"
                id="create-name"
                name="name"
                value={formData.name}
                onChange={handleInputChange}
                required
                disabled={loading}
                placeholder={t('floorball.seasons.placeholders.name', 'Enter season name')}
              />
            </div>

            <div className="form-group">
              <label htmlFor="create-division">
                {t('floorball.seasons.fields.division', 'Division')} *
              </label>
              <select
                id="create-division"
                name="divisionId"
                value={formData.divisionId}
                onChange={handleInputChange}
                required
                disabled={loading}
              >
                <option value="">{t('floorball.seasons.placeholders.selectDivision', 'Select division')}</option>
                {divisions.map(division => (
                  <option key={division.id} value={division.id}>{division.name}</option>
                ))}
              </select>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="create-startDate">
                  {t('floorball.seasons.fields.startDate', 'Start Date')} *
                </label>
                <input
                  type="date"
                  id="create-startDate"
                  name="startDate"
                  value={formData.startDate}
                  onChange={handleInputChange}
                  required
                  disabled={loading}
                />
              </div>

              <div className="form-group">
                <label htmlFor="create-endDate">
                  {t('floorball.seasons.fields.endDate', 'End Date')} *
                </label>
                <input
                  type="date"
                  id="create-endDate"
                  name="endDate"
                  value={formData.endDate}
                  onChange={handleInputChange}
                  required
                  disabled={loading}
                  min={formData.startDate}
                />
              </div>
            </div>


            <div className="toggle-container">
              <label className="toggle-label">
                {t('floorball.seasons.fields.isActive', 'Active')}
              </label>
              <button
                type="button"
                className={`toggle-button ${isActive ? 'active' : ''}`}
                onClick={() => setIsActive(!isActive)}
                disabled={loading}
                aria-pressed={isActive}
              >
                {isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </button>
            </div>
            <div className="info-message">
              <i className="fas fa-info-circle"></i>
              {t('floorball.seasons.create.info', 'The season will be created as inactive by default')}
            </div>

            <div className="form-actions">
              <button 
                type="button"
                className="btn btn-secondary"
                onClick={() => navigate('/admin/floorball/seasons')}
                disabled={loading}
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button 
                type="submit"
                className="btn btn-primary"
                disabled={loading}
              >
                {loading ? (
                  <>
                    <i className="fas fa-spinner fa-spin"></i>
                    {t('common.creating', 'Creating...')}
                  </>
                ) : (
                  t('common.create', 'Create')
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </PageTemplate>
  );
};

export default CreateSeasonPage;
