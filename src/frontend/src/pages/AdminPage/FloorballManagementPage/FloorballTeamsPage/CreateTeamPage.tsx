import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { floorballTeamService } from '../../../../api/floorball/floorballTeamService';
import { clubService } from '../../../../api/common/clubService';
import { divisionService } from '../../../../api/common/divisionService';
import { TeamCategory, type FloorballTeamRequest } from '../../../../types/floorball/floorballTypes';
import type { DivisionType } from '../../../../types/common/divisionType';
import SearchableInfiniteDropdown from '../../../../components/SearchableInfiniteDropdown/SearchableInfiniteDropdown';
import './CreateTeamPage.scss';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';

const CreateTeamPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [divisions, setDivisions] = useState<DivisionType[]>([]);
  
  const [formData, setFormData] = useState<FloorballTeamRequest>({
    name: '',
    divisionId: '',
    clubId: '',
    homeArena: '',
    primaryJerseyColor: '#000000',
    category: 'Adult' as TeamCategory,
    secondaryJerseyColor: ''
  });

  // Load divisions on component mount
  useEffect(() => {
    loadDivisions();
  }, []);

  // Search function for clubs using paginated endpoint
  const searchClubs = async (query: string, page: number) => {
    const pageSize = 50;
    
    try {
      // Use paginated endpoint
      const response = await clubService.getPaged(page, pageSize);
      
      let filteredClubs = response.data;
      
      // If there's a search query, filter client-side
      // Note: For better performance with large datasets, backend should support search pagination
      if (query.trim()) {
        const queryLower = query.toLowerCase();
        filteredClubs = response.data.filter(club => 
          club.name.toLowerCase().includes(queryLower)
        );
      }
      
      return {
        data: filteredClubs.map(club => ({ id: club.id, name: club.name })),
        pagination: {
          hasNextPage: response.pagination.hasNextPage && (!query.trim() || filteredClubs.length === pageSize),
          totalCount: query.trim() ? filteredClubs.length : response.pagination.totalCount
        }
      };
    } catch (err) {
      console.error('Error searching clubs:', err);
      throw err;
    }
  };

  const loadDivisions = async () => {
    try {
      const response = await divisionService.getAll();
      setDivisions(response.data);
    } catch (err) {
      console.error('Error loading divisions:', err);
      setDivisions([]);
    }
  };

  const handleInputChange = (field: keyof FloorballTeamRequest, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    
    try {
      // Prepare create data with proper validation
      const createData: FloorballTeamRequest = {
        name: formData.name,
        divisionId: formData.divisionId,
        clubId: formData.clubId,
        homeArena: formData.homeArena,
        primaryJerseyColor: formData.primaryJerseyColor,
        category: formData.category,
        // Only include secondaryJerseyColor if it's valid (2-50 characters) or omit it entirely
        ...(formData.secondaryJerseyColor && formData.secondaryJerseyColor.length >= 2 && formData.secondaryJerseyColor.length <= 50
          ? { secondaryJerseyColor: formData.secondaryJerseyColor }
          : {})
      };
      
      console.log('Creating team with data:', createData);
      
      await floorballTeamService.create(createData);
      
      // Navigate back to teams list
      navigate('/admin/floorball/teams');
    } catch (error) {
      console.error('Error creating team:', error);
      setError(error instanceof Error ? error.message : 'Failed to create team');
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    navigate('/admin/floorball/teams');
  };

  return (
    <PageTemplate title={t('floorball.teams.createNew', 'Create New Team')}>
      <div className="create-team-page">
        
        <div className="create-team-header">
          <h1>{t('floorball.teams.createNew', 'Create New Team')}</h1>
        </div>

        <ErrorPopup message={error} />

        <form onSubmit={handleSubmit} className="create-team-form">
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
            <SearchableInfiniteDropdown
              placeholder={t('floorball.teams.selectClub', 'Select a club')}
              value={formData.clubId}
              onChange={(value) => handleInputChange('clubId', value)}
              onSearch={searchClubs}
              emptyMessage={t('floorball.teams.noClubsFound', 'No clubs found')}
              searchPlaceholder={t('floorball.teams.searchClubs', 'Search clubs...')}
              required
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="division">{t('floorball.teams.division', 'Division')} *</label>
              <select
                id="division"
                value={formData.divisionId}
                onChange={(e) => handleInputChange('divisionId', e.target.value)}
                required
              >
                <option value="">{t('floorball.teams.selectDivision', 'Select division...')}</option>
                {divisions.map(division => (
                  <option key={division.id} value={division.id}>{division.name}</option>
                ))}
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
                <option value="Adult">{t('floorball.categories.adult', 'Adult')}</option>
                <option value="Youth">{t('floorball.categories.youth', 'Youth')}</option>
                <option value="Women">{t('floorball.categories.women', 'Women')}</option>
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
            <button type="button" onClick={handleCancel} className="cancel-button" disabled={loading}>
              {t('common.cancel', 'Cancel')}
            </button>
            <button type="submit" disabled={loading} className="submit-button">
              {loading ? t('common.creating', 'Creating...') : t('common.create', 'Create')}
            </button>
          </div>
        </form>
      </div>
    </PageTemplate>
  );
};

export default CreateTeamPage;
