import React from 'react';
import { useTranslation } from 'react-i18next';
import type { Club } from '../../../../../api/clubService';
import { FloorballDivision, TeamCategory, type FloorballTeamRequest } from '../../../../../types/floorball/floorballTypes';
import './TeamDetailsForm.scss';

interface TeamDetailsFormProps {
  formData: FloorballTeamRequest;
  handleInputChange: (field: keyof FloorballTeamRequest, value: string) => void;
  clubs: Club[];
  loading: boolean;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
  onClose: () => void;
}

const TeamDetailsForm = ({
  formData,
  handleInputChange,
  clubs,
  loading,
  handleSubmit,
  onClose
}: TeamDetailsFormProps) => {
  const { t } = useTranslation();

  return (
    <form onSubmit={handleSubmit} className="team-form">
      <div className="form-group">
        <label htmlFor="teamName">{t('floorball.teams.name', 'Team Name')} *</label>
        <input
          id="teamName"
          type="text"
          value={formData.name}
          onChange={(e) => handleInputChange('name', e.target.value)}
          required
          placeholder={t('floorball.teams.namePlaceholder', 'Enter team name')}
        />
      </div>

      <div className="form-group">
        <label htmlFor="clubId">{t('floorball.teams.club', 'Club')} *</label>
        <select
          id="clubId"
          value={formData.clubId}
          onChange={(e) => handleInputChange('clubId', e.target.value)}
          required
        >
          <option value="">{t('floorball.teams.selectClub', 'Select a club')}</option>
          {clubs.map(club => (
            <option key={club.id} value={club.id}>{club.name}</option>
          ))}
        </select>
      </div>

      <div className="form-row">
        <div className="form-group">
          <label htmlFor="division">{t('floorball.teams.division', 'Division')} *</label>
          <select
            id="division"
            value={formData.division}
            onChange={(e) => handleInputChange('division', e.target.value as FloorballDivision)}
            required
          >
            <option value={FloorballDivision.Premier}>{t('floorball.divisions.premier', 'Premier')}</option>
            <option value={FloorballDivision.Division1}>{t('floorball.divisions.division1', 'Division 1')}</option>
            <option value={FloorballDivision.Division2}>{t('floorball.divisions.division2', 'Division 2')}</option>
            <option value={FloorballDivision.Division3}>{t('floorball.divisions.division3', 'Division 3')}</option>
            <option value={FloorballDivision.Division4}>{t('floorball.divisions.division4', 'Division 4')}</option>
            <option value={FloorballDivision.Youth}>{t('floorball.divisions.youth', 'Youth')}</option>
            <option value={FloorballDivision.Junior}>{t('floorball.divisions.junior', 'Junior')}</option>
            <option value={FloorballDivision.Veterans}>{t('floorball.divisions.veterans', 'Veterans')}</option>
          </select>
        </div>

        <div className="form-group">
          <label htmlFor="category">{t('floorball.teams.category', 'Category')} *</label>
          <select
            id="category"
            value={formData.category}
            onChange={(e) => handleInputChange('category', e.target.value as TeamCategory)}
            required
          >
            <option value={TeamCategory.Adult}>{t('floorball.categories.adult', 'Adult')}</option>
            <option value={TeamCategory.Youth}>{t('floorball.categories.youth', 'Youth')}</option>
            <option value={TeamCategory.Women}>{t('floorball.categories.women', 'Women')}</option>
          </select>
        </div>
      </div>

      <div className="form-group">
        <label htmlFor="homeArena">{t('floorball.teams.homeArena', 'Home Arena')} *</label>
        <input
          id="homeArena"
          type="text"
          value={formData.homeArena}
          onChange={(e) => handleInputChange('homeArena', e.target.value)}
          required
          placeholder={t('floorball.teams.homeArenaPlaceholder', 'Enter home arena')}
        />
      </div>

      <div className="form-row">
        <div className="form-group">
          <label htmlFor="primaryColor">{t('floorball.teams.primary', 'Primary Jersey Color')} *</label>
          <div className="color-input-group">
            <input
              id="primaryColor"
              type="color"
              value={formData.primaryJerseyColor}
              onChange={(e) => handleInputChange('primaryJerseyColor', e.target.value)}
              required
            />
            <input
              type="text"
              value={formData.primaryJerseyColor}
              onChange={(e) => handleInputChange('primaryJerseyColor', e.target.value)}
              placeholder="#000000"
            />
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="secondaryColor">{t('floorball.teams.secondary', 'Secondary Jersey Color')}</label>
          <div className="color-input-group">
            <input
              id="secondaryColor"
              type="color"
              value={formData.secondaryJerseyColor || '#ffffff'}
              onChange={(e) => handleInputChange('secondaryJerseyColor', e.target.value)}
            />
            <input
              type="text"
              value={formData.secondaryJerseyColor || ''}
              onChange={(e) => handleInputChange('secondaryJerseyColor', e.target.value)}
              placeholder={t('floorball.teams.optional', 'Optional')}
              minLength={2}
              maxLength={50}
            />
          </div>
          {formData.secondaryJerseyColor && formData.secondaryJerseyColor.length > 0 && formData.secondaryJerseyColor.length < 2 && (
            <div className="validation-error">
              {t('floorball.teams.secondaryColorTooShort', 'Secondary color must be at least 2 characters')}
            </div>
          )}
          {formData.secondaryJerseyColor && formData.secondaryJerseyColor.length > 50 && (
            <div className="validation-error">
              {t('floorball.teams.secondaryColorTooLong', 'Secondary color must be no more than 50 characters')}
            </div>
          )}
        </div>
      </div>

      <div className="form-actions">
        <button type="button" onClick={onClose} className="cancel-button">
          {t('common.cancel', 'Cancel')}
        </button>
        <button type="submit" disabled={loading} className="submit-button">
          {loading ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        </button>
      </div>
    </form>
  );
}

export default TeamDetailsForm; 