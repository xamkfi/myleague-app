import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { footballTeamService } from '../../../../api/football/footballTeamService';
import { getClubs, type Club } from '../../../../api/common/clubService';
import { 
  TeamCategory,
  type FootballTeam, 
  type FootballTeamRequest
} from '../../../../types/football/footballTypes';
import './EditTeamPage.scss';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';

const EditTeamPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id: teamId } = useParams<{ id: string }>();
  
  const [loading, setLoading] = useState(false);
  const [loadingTeam, setLoadingTeam] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [clubs, setClubs] = useState<Club[]>([]);
  const [currentTeam, setCurrentTeam] = useState<FootballTeam | null>(null);
  
  const [formData, setFormData] = useState<FootballTeamRequest>({
    name: '',
    shortName: '',
    divisionId: undefined,
    clubId: '',
    homeArena: '',
    primaryJerseyColor: '#000000',
    category: 'Adult' as TeamCategory,
    secondaryJerseyColor: ''
  });

  // Add a separate state to track the existing divisionId
  const [existingDivisionId, setExistingDivisionId] = useState<string | null>(null);
  
  const loadTeamData = useCallback(async () => {
    if (!teamId) return;
    
    try {
      setLoadingTeam(true);
      const team = await footballTeamService.getById(teamId);
      
      setCurrentTeam(team);
      // Store the existing divisionId separately
      setExistingDivisionId(team.divisionId || null);
      setFormData({
        name: team.name,
        shortName: team.shortName,
        divisionId: team.divisionId ?? undefined,
        clubId: team.club.id,
        homeArena: team.homeArena,
        primaryJerseyColor: team.primaryJerseyColor,
        category: 'Adult' as TeamCategory, // Default since it's not in the response
        secondaryJerseyColor: team.secondaryJerseyColor || ''
      });

    } catch (err) {
      console.error('Error loading team data:', err);
      setError(String(err));
    } finally {
      setLoadingTeam(false);
    }
  }, [teamId]);

  // Load team data when component mounts
  useEffect(() => {
    if (teamId) {
      loadTeamData();
      loadClubs();
    }
  }, [teamId, loadTeamData]);

  const loadClubs = async () => {
    try {
      const response = await getClubs();
      setClubs(response);
    } catch (err) {
      console.error('Error loading clubs:', err);
      setClubs([]);
    }
  };

  const handleInputChange = (field: keyof FootballTeamRequest, value: string) => {
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
      // Prepare update data with proper validation
      const resolvedDivisionId = formData.divisionId ?? existingDivisionId ?? undefined;
      const updateData: FootballTeamRequest = {
        name: formData.name,
        shortName: formData.shortName?.trim() || undefined,
        clubId: formData.clubId,
        homeArena: formData.homeArena,
        primaryJerseyColor: formData.primaryJerseyColor,
        category: formData.category,
        ...(resolvedDivisionId !== undefined ? { divisionId: resolvedDivisionId } : {}),
        // Only include secondaryJerseyColor if it's valid (2-50 characters) or omit it entirely
        ...(formData.secondaryJerseyColor && formData.secondaryJerseyColor.length >= 2 && formData.secondaryJerseyColor.length <= 50
          ? { secondaryJerseyColor: formData.secondaryJerseyColor }
          : {})
      };
      
      await footballTeamService.update(teamId!, updateData);
      
      // Navigate back to teams list
      navigate('/admin/football/teams');
    } catch (error) {
      console.error('Error saving team:', error);
      setError(error instanceof Error ? error.message : 'Failed to save team');
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    navigate('/admin/football/teams');
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

  if (!teamId) {
    return (
      <PageTemplate title={t('football.teams.editTeam', 'Edit Team')}>
        <ErrorPopup message={'Team ID is required'} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('football.teams.editTeam', 'Edit Team')}>
      <div className="edit-team-page">
        
        <div className="edit-team-header">
          <h1>{t('football.teams.editTeam', 'Edit Team')}: {currentTeam?.name}</h1>
        </div>

        <ErrorPopup message={error} />

        <form onSubmit={handleSubmit} className="edit-team-form">
          <div className="form-row name-row">
            <div className="form-group">
              <label htmlFor="teamName">{t('football.teams.name', 'Team Name')} *</label>
              <input
                id="teamName"
                type="text"
                value={formData.name}
                onChange={(e) => handleInputChange('name', e.target.value)}
                required
                placeholder={t('football.teams.namePlaceholder', 'Enter team name')}
              />
            </div>

            <div className="form-group">
              <label htmlFor="teamShortName">{t('football.teams.shortName', 'Short Name / Acronym')}</label>
              <input
                id="teamShortName"
                type="text"
                value={formData.shortName || ''}
                onChange={(e) => handleInputChange('shortName', e.target.value.toUpperCase())}
                placeholder={t('football.teams.shortNamePlaceholder', 'e.g., NYR')}
                maxLength={4}
              />
              {formData.shortName && formData.shortName.length > 4 && (
                <div className="validation-error">
                  {t('football.teams.shortNameTooLong', 'Short name cannot exceed 4 characters')}
                </div>
              )}
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="clubId">{t('football.teams.club', 'Club')} *</label>
            <select
              id="clubId"
              value={formData.clubId}
              onChange={(e) => handleInputChange('clubId', e.target.value)}
              required
            >
              <option value="">{t('football.teams.selectClub', 'Select a club')}</option>
              {clubs.map(club => (
                <option key={club.id} value={club.id}>{club.name}</option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="category">{t('football.teams.category', 'Category')} *</label>
            <select
              id="category"
              value={formData.category}
              onChange={(e) => handleInputChange('category', e.target.value as TeamCategory)}
              required
            >
              <option value="Adult">{t('football.categories.adult', 'Adult')}</option>
              <option value="Youth">{t('football.categories.youth', 'Youth')}</option>
              <option value="Women">{t('football.categories.women', 'Women')}</option>
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="homeArena">{t('football.teams.homeArena', 'Home Arena')} *</label>
            <input
              id="homeArena"
              type="text"
              value={formData.homeArena}
              onChange={(e) => handleInputChange('homeArena', e.target.value)}
              required
              placeholder={t('football.teams.homeArenaPlaceholder', 'Enter home arena')}
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="primaryColor">{t('football.teams.primary', 'Primary Jersey Color')} *</label>
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
              <label htmlFor="secondaryColor">{t('football.teams.secondary', 'Secondary Jersey Color')}</label>
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
                  placeholder={t('football.teams.optional', 'Optional')}
                  minLength={2}
                  maxLength={50}
                />
              </div>
              {formData.secondaryJerseyColor && formData.secondaryJerseyColor.length > 0 && formData.secondaryJerseyColor.length < 2 && (
                <div className="validation-error">
                  {t('football.teams.secondaryColorTooShort', 'Secondary color must be at least 2 characters')}
                </div>
              )}
              {formData.secondaryJerseyColor && formData.secondaryJerseyColor.length > 50 && (
                <div className="validation-error">
                  {t('football.teams.secondaryColorTooLong', 'Secondary color must be no more than 50 characters')}
                </div>
              )}
            </div>
          </div>

          <div className="form-actions">
            <button type="button" onClick={handleCancel} className="cancel-button" disabled={loading}>
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
};

export default EditTeamPage;
