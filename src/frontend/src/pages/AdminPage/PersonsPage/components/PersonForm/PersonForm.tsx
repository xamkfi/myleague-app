import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { PersonFormData, EnhancedPersonFormData, Person } from '../../../../../types/admin/personTypes';
import { PersonRole } from '../../../../../types/admin/personTypes';
import { personApi } from '../../../../../api/admin/personApi';
import { floorballPlayerService } from '../../../../../api/floorball/floorballPlayerService';
import { floorballTeamService } from '../../../../../api/floorball/floorballTeamService';
import { floorballTeamSearchService } from '../../../../../api/floorball/floorballTeamSearchService';
import { FloorballPosition } from '../../../../../types/floorball/floorballTypes';
import SearchableInfiniteDropdown from '../../../../../components/SearchableInfiniteDropdown/SearchableInfiniteDropdown';
import './PersonForm.scss';

interface PersonFormProps {
  mode?: 'standalone' | 'embedded';
  personId?: string; // For embedded mode when editing
  onSuccess?: (createdPerson: Person) => void;
  onCancel?: () => void;
  showTeamAssignment?: boolean;
  initialData?: Partial<EnhancedPersonFormData>;
}

const MAX_LENGTHS = {
  firstName: 100,
  lastName: 100,
  street1: 200,
  street2: 200,
  city: 100,
  postalCode: 20,
  country: 100,
  email: 255,
  phone: 50,
  alternativePhone: 50
};

const PersonForm = ({
  mode = 'standalone',
  personId: propPersonId,
  onSuccess,
  onCancel,
  showTeamAssignment = true,
  initialData
}: PersonFormProps) => {
  const { id: urlId } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  
  // Use personId prop in embedded mode, fallback to URL param in standalone mode
  const id = mode === 'embedded' ? propPersonId : urlId;
  const isEditMode = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<{ [key: string]: string }>({});
  
  // Initialize form data with defaults and merge with initialData if provided
  const getInitialFormData = (): EnhancedPersonFormData => {
    const defaultData: EnhancedPersonFormData = {
      firstName: '',
      lastName: '',
      birthDate: '',
      isRegistered: false,
      role: PersonRole.User,
      address: {
        street1: '',
        street2: '',
        city: '',
        postalCode: '',
        country: ''
      },
      contactInfo: {
        email: '',
        phone: '',
        alternativePhone: ''
      },
      teamId: undefined,
      position: undefined,
      jerseyNumber: undefined
    };
    
    if (initialData) {
      return {
        ...defaultData,
        ...initialData,
        address: { ...defaultData.address, ...initialData.address },
        contactInfo: { ...defaultData.contactInfo, ...initialData.contactInfo }
      };
    }
    
    return defaultData;
  };
  
  const [formData, setFormData] = useState<EnhancedPersonFormData>(getInitialFormData());

  useEffect(() => {
    const fetchPerson = async () => {
      if (!isEditMode) return;
      
      try {
        setLoading(true);
        const person = await personApi.getById(id!);
        
        // Convert ISO date to dd-mm-yyyy
        const date = new Date(person.birthDate);
        const formattedDate = `${date.getDate().toString().padStart(2, '0')}-${(date.getMonth() + 1).toString().padStart(2, '0')}-${date.getFullYear()}`;
        
        setFormData({
          firstName: person.firstName,
          lastName: person.lastName,
          birthDate: formattedDate,
          isRegistered: person.isRegistered,
          role: person.role,
          address: person.address || {
            street1: '',
            street2: '',
            city: '',
            postalCode: '',
            country: ''
          },
          contactInfo: person.contactInfo || {
            email: '',
            phone: '',
            alternativePhone: ''
          },
          teamId: undefined,
          position: undefined,
          jerseyNumber: undefined
        });
      } catch (error) {
        console.error('Failed to fetch person:', error);
        setError(t('admin.persons.errors.fetchFailed', 'Failed to fetch person details'));
      } finally {
        setLoading(false);
      }
    };

    fetchPerson();
  }, [id, isEditMode, t]);

  const validateBirthDate = (date: string): string | null => {
    if (!date) {
      return t('admin.persons.validation.birthDateRequired', 'Birth date is required');
    }

    // Parse dd-mm-yyyy format
    const [day, month, year] = date.split('-').map(Number);
    const birthDate = new Date(year, month - 1, day);
    const today = new Date();
    
    if (isNaN(birthDate.getTime())) {
      return t('admin.persons.validation.invalidDate', 'Invalid date format');
    }
    
    if (birthDate > today) {
      return t('admin.persons.validation.birthDateFuture', 'Birth date cannot be in the future');
    }

    const minDate = new Date();
    minDate.setFullYear(minDate.getFullYear() - 120); // Reasonable maximum age
    if (birthDate < minDate) {
      return t('admin.persons.validation.birthDateTooOld', 'Birth date is too far in the past');
    }

    return null;
  };

  const validateEmail = (email: string): string | null => {
    if (!email) return null; // Email is optional
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      return t('admin.persons.validation.invalidEmail', 'Invalid email format');
    }
    if (email.length > MAX_LENGTHS.email) {
      return t('admin.persons.validation.emailTooLong', 'Email cannot exceed {{max}} characters', { max: MAX_LENGTHS.email });
    }
    return null;
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;
    const [section, field] = name.split('.');

    // Clear field error when user starts typing
    setFieldErrors(prev => ({ ...prev, [name]: '' }));

    if (type === 'checkbox') {
      setFormData(prev => ({
        ...prev,
        [name]: checked
      }));
      return;
    }

    if (name === 'birthDate') {
      setFormData(prev => ({
        ...prev,
        birthDate: value
      }));
      
      const dateError = validateBirthDate(value);
      if (dateError) {
        setFieldErrors(prev => ({ ...prev, birthDate: dateError }));
      }
      return;
    }

    // Handle team assignment fields
    if (name === 'position') {
      setFormData(prev => ({
        ...prev,
        position: value || undefined
      }));
      return;
    }

    if (name === 'jerseyNumber') {
      const numValue = value ? parseInt(value) : undefined;
      setFormData(prev => ({
        ...prev,
        jerseyNumber: numValue
      }));
      return;
    }

    if (section === 'address' || section === 'contactInfo') {
      setFormData(prev => ({
        ...prev,
        [section]: {
          ...prev[section],
          [field]: value
        }
      }));

      // Validate email when it changes
      if (section === 'contactInfo' && field === 'email') {
        const emailError = validateEmail(value);
        if (emailError) {
          setFieldErrors(prev => ({ ...prev, 'contactInfo.email': emailError }));
        }
      }
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: value
      }));
    }
  };

  // Handle team selection from dropdown
  const handleTeamChange = (teamId: string) => {
    setFormData(prev => ({
      ...prev,
      teamId: teamId || undefined
    }));
    // Clear field error when team selection changes
    setFieldErrors(prev => ({ ...prev, teamId: '', position: '' }));
  };

  const validateForm = (): boolean => {
    const errors: { [key: string]: string } = {};

    // Required fields validation
    if (!formData.firstName.trim()) {
      errors.firstName = t('admin.persons.validation.firstNameRequired', 'First name is required');
    } else if (formData.firstName.length > MAX_LENGTHS.firstName) {
      errors.firstName = t('admin.persons.validation.firstNameTooLong', 'First name cannot exceed {{max}} characters', { max: MAX_LENGTHS.firstName });
    }

    if (!formData.lastName.trim()) {
      errors.lastName = t('admin.persons.validation.lastNameRequired', 'Last name is required');
    } else if (formData.lastName.length > MAX_LENGTHS.lastName) {
      errors.lastName = t('admin.persons.validation.lastNameTooLong', 'Last name cannot exceed {{max}} characters', { max: MAX_LENGTHS.lastName });
    }

    const birthDateError = validateBirthDate(formData.birthDate);
    if (birthDateError) {
      errors.birthDate = birthDateError;
    }

    // Address validation (all fields required if any field is filled)
    const addressValues = [
      formData.address.street1,
      formData.address.street2,
      formData.address.city,
      formData.address.postalCode,
      formData.address.country
    ];
    const hasAnyAddressField = addressValues.some(value => value.trim() !== '');

    if (hasAnyAddressField) {
      if (!formData.address.street1.trim()) {
        errors['address.street1'] = t('admin.persons.validation.street1Required', 'Street address is required');
      } else if (formData.address.street1.length > MAX_LENGTHS.street1) {
        errors['address.street1'] = t('admin.persons.validation.street1TooLong', 'Street address cannot exceed {{max}} characters', { max: MAX_LENGTHS.street1 });
      }

      if (formData.address.street2 && formData.address.street2.length > MAX_LENGTHS.street2) {
        errors['address.street2'] = t('admin.persons.validation.street2TooLong', 'Street address 2 cannot exceed {{max}} characters', { max: MAX_LENGTHS.street2 });
      }

      if (!formData.address.city.trim()) {
        errors['address.city'] = t('admin.persons.validation.cityRequired', 'City is required');
      } else if (formData.address.city.length > MAX_LENGTHS.city) {
        errors['address.city'] = t('admin.persons.validation.cityTooLong', 'City cannot exceed {{max}} characters', { max: MAX_LENGTHS.city });
      }

      if (!formData.address.postalCode.trim()) {
        errors['address.postalCode'] = t('admin.persons.validation.postalCodeRequired', 'Postal code is required');
      } else if (formData.address.postalCode.length > MAX_LENGTHS.postalCode) {
        errors['address.postalCode'] = t('admin.persons.validation.postalCodeTooLong', 'Postal code cannot exceed {{max}} characters', { max: MAX_LENGTHS.postalCode });
      }

      if (!formData.address.country.trim()) {
        errors['address.country'] = t('admin.persons.validation.countryRequired', 'Country is required');
      } else if (formData.address.country.length > MAX_LENGTHS.country) {
        errors['address.country'] = t('admin.persons.validation.countryTooLong', 'Country cannot exceed {{max}} characters', { max: MAX_LENGTHS.country });
      }
    }

    // Contact info validation
    const emailError = validateEmail(formData.contactInfo.email);
    if (emailError) {
      errors['contactInfo.email'] = emailError;
    }

    if (formData.contactInfo.phone && formData.contactInfo.phone.length > MAX_LENGTHS.phone) {
      errors['contactInfo.phone'] = t('admin.persons.validation.phoneTooLong', 'Phone number cannot exceed {{max}} characters', { max: MAX_LENGTHS.phone });
    }

    if (formData.contactInfo.alternativePhone && formData.contactInfo.alternativePhone.length > MAX_LENGTHS.alternativePhone) {
      errors['contactInfo.alternativePhone'] = t('admin.persons.validation.alternativePhoneTooLong', 'Alternative phone number cannot exceed {{max}} characters', { max: MAX_LENGTHS.alternativePhone });
    }

    // Team assignment validation (only for new persons, not edits)
    if (!isEditMode && formData.teamId && !formData.position) {
      errors.position = t('admin.persons.validation.positionRequired', 'Position is required when team is selected');
    }

    if (formData.jerseyNumber && (formData.jerseyNumber < 1 || formData.jerseyNumber > 99)) {
      errors.jerseyNumber = t('admin.persons.validation.invalidJerseyNumber', 'Jersey number must be between 1 and 99');
    }

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    try {
      setLoading(true);
      setError(null);

      // Convert dd-mm-yyyy to ISO format for API
      const [day, month, year] = formData.birthDate.split('-').map(Number);
      const isoDate = new Date(year, month - 1, day).toISOString();

      // Prepare person data (excluding team assignment fields)
      const personData: PersonFormData = {
        firstName: formData.firstName,
        lastName: formData.lastName,
        birthDate: isoDate,
        isRegistered: formData.isRegistered,
        role: formData.role,
        address: formData.address,
        contactInfo: formData.contactInfo
      };

      let createdPerson;
      if (isEditMode) {
        createdPerson = await personApi.update(id!, personData);
      } else {
        createdPerson = await personApi.create(personData);
      }

      // Step 2: If team is selected, create FloorballPlayer and add to team
      if (formData.teamId && formData.position && !isEditMode) {
        try {
          // Create FloorballPlayer
          const createdPlayer = await floorballPlayerService.create({
            personId: createdPerson.id
          });

          // Add player to team with position and optional jersey number
          await floorballTeamService.addPlayerToTeam(
            formData.teamId,
            createdPlayer.id,
            formData.position as FloorballPosition,
            formData.jerseyNumber
          );

          // Success message could be enhanced here to show team assignment
          console.log(`Person created and added to team with position ${formData.position}`);
        } catch (teamAssignmentError) {
          // Person was created but team assignment failed
          console.warn('Team assignment failed:', teamAssignmentError);
          setError(t('admin.persons.errors.teamAssignmentFailed', 'Person created, but team assignment failed'));
          // Still navigate since person was created successfully
        }
      }

      // Handle success based on mode
      if (mode === 'embedded' && onSuccess) {
        onSuccess(createdPerson);
      } else {
        navigate('/admin/persons');
      }
    } catch (error) {
      console.error('Failed to save person:', error);
      setError(t('admin.persons.errors.saveFailed', 'Failed to save person'));
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    if (mode === 'embedded' && onCancel) {
      onCancel();
    } else {
      navigate('/admin/persons');
    }
  };

  if (loading && isEditMode) {
    return <div className="person-form-loading">{t('admin.persons.loading', 'Loading...')}</div>;
  }

  return (
    <form className="person-form" onSubmit={handleSubmit}>
      <div className="form-section">
        <h3>{t('admin.persons.form.basicInfo', 'Basic Information')}</h3>
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="firstName">
              {t('admin.persons.form.firstName', 'First Name')} <span className="required">*</span>
            </label>
            <input
              type="text"
              id="firstName"
              name="firstName"
              value={formData.firstName}
              onChange={handleInputChange}
              maxLength={MAX_LENGTHS.firstName}
              required
              className={fieldErrors.firstName ? 'error' : ''}
            />
            {fieldErrors.firstName && (
              <div className="field-error">{fieldErrors.firstName}</div>
            )}
          </div>
          <div className="form-group">
            <label htmlFor="lastName">
              {t('admin.persons.form.lastName', 'Last Name')} <span className="required">*</span>
            </label>
            <input
              type="text"
              id="lastName"
              name="lastName"
              value={formData.lastName}
              onChange={handleInputChange}
              maxLength={MAX_LENGTHS.lastName}
              required
              className={fieldErrors.lastName ? 'error' : ''}
            />
            {fieldErrors.lastName && (
              <div className="field-error">{fieldErrors.lastName}</div>
            )}
          </div>
        </div>
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="birthDate">
              {t('admin.persons.form.birthDate', 'Birth Date')} <span className="required">*</span>
            </label>
            <input
              type="text"
              id="birthDate"
              name="birthDate"
              value={formData.birthDate}
              onChange={handleInputChange}
              placeholder="dd-mm-yyyy"
              required
              className={fieldErrors.birthDate ? 'error' : ''}
            />
            {fieldErrors.birthDate && (
              <div className="field-error">{fieldErrors.birthDate}</div>
            )}
            <div className="field-hint">
              {t('admin.persons.form.dateFormat', 'Format: dd-mm-yyyy')}
            </div>
          </div>
          <div className="form-group">
            <label htmlFor="isRegistered" className="checkbox-label">
              {t('admin.persons.form.isRegistered', 'Registered')}
            </label>
            <input
              type="checkbox"
              id="isRegistered"
              name="isRegistered"
              checked={formData.isRegistered}
              onChange={handleInputChange}
            />
          </div>
        </div>
      </div>

      <div className="form-section">
        <h3>{t('admin.persons.form.address', 'Address')}</h3>
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="address.street1">
              {t('admin.persons.form.street1', 'Street Address')}
              {(formData.address.street2 || formData.address.city || formData.address.postalCode || formData.address.country).trim() !== '' && <span className="required">*</span>}
            </label>
            <input
              type="text"
              id="address.street1"
              name="address.street1"
              value={formData.address.street1}
              onChange={handleInputChange}
              maxLength={MAX_LENGTHS.street1}
              className={fieldErrors['address.street1'] ? 'error' : ''}
            />
            {fieldErrors['address.street1'] && (
              <div className="field-error">{fieldErrors['address.street1']}</div>
            )}
          </div>
          <div className="form-group">
            <label htmlFor="address.street2">
              {t('admin.persons.form.street2', 'Street Address 2')}
            </label>
            <input
              type="text"
              id="address.street2"
              name="address.street2"
              value={formData.address.street2}
              onChange={handleInputChange}
              maxLength={MAX_LENGTHS.street2}
              className={fieldErrors['address.street2'] ? 'error' : ''}
            />
            {fieldErrors['address.street2'] && (
              <div className="field-error">{fieldErrors['address.street2']}</div>
            )}
          </div>
        </div>
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="address.city">
              {t('admin.persons.form.city', 'City')}
              {(formData.address.street1 || formData.address.street2 || formData.address.postalCode || formData.address.country).trim() !== '' && <span className="required">*</span>}
            </label>
            <input
              type="text"
              id="address.city"
              name="address.city"
              value={formData.address.city}
              onChange={handleInputChange}
              maxLength={MAX_LENGTHS.city}
              className={fieldErrors['address.city'] ? 'error' : ''}
            />
            {fieldErrors['address.city'] && (
              <div className="field-error">{fieldErrors['address.city']}</div>
            )}
          </div>
          <div className="form-group">
            <label htmlFor="address.postalCode">
              {t('admin.persons.form.postalCode', 'Postal Code')}
              {(formData.address.street1 || formData.address.street2 || formData.address.city || formData.address.country).trim() !== '' && <span className="required">*</span>}
            </label>
            <input
              type="text"
              id="address.postalCode"
              name="address.postalCode"
              value={formData.address.postalCode}
              onChange={handleInputChange}
              maxLength={MAX_LENGTHS.postalCode}
              className={fieldErrors['address.postalCode'] ? 'error' : ''}
            />
            {fieldErrors['address.postalCode'] && (
              <div className="field-error">{fieldErrors['address.postalCode']}</div>
            )}
          </div>
          <div className="form-group">
            <label htmlFor="address.country">
              {t('admin.persons.form.country', 'Country')}
              {(formData.address.street1 || formData.address.street2 || formData.address.city || formData.address.postalCode).trim() !== '' && <span className="required">*</span>}
            </label>
            <input
              type="text"
              id="address.country"
              name="address.country"
              value={formData.address.country}
              onChange={handleInputChange}
              maxLength={MAX_LENGTHS.country}
              className={fieldErrors['address.country'] ? 'error' : ''}
            />
            {fieldErrors['address.country'] && (
              <div className="field-error">{fieldErrors['address.country']}</div>
            )}
          </div>
        </div>
      </div>

      <div className="form-section">
        <h3>{t('admin.persons.form.contact', 'Contact Information')}</h3>
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="contactInfo.email">
              {t('admin.persons.form.email', 'Email')}
            </label>
            <input
              type="email"
              id="contactInfo.email"
              name="contactInfo.email"
              value={formData.contactInfo.email}
              onChange={handleInputChange}
              maxLength={MAX_LENGTHS.email}
              className={fieldErrors['contactInfo.email'] ? 'error' : ''}
            />
            {fieldErrors['contactInfo.email'] && (
              <div className="field-error">{fieldErrors['contactInfo.email']}</div>
            )}
          </div>
        </div>
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="contactInfo.phone">
              {t('admin.persons.form.phone', 'Phone')}
            </label>
            <input
              type="tel"
              id="contactInfo.phone"
              name="contactInfo.phone"
              value={formData.contactInfo.phone}
              onChange={handleInputChange}
              maxLength={MAX_LENGTHS.phone}
              className={fieldErrors['contactInfo.phone'] ? 'error' : ''}
            />
            {fieldErrors['contactInfo.phone'] && (
              <div className="field-error">{fieldErrors['contactInfo.phone']}</div>
            )}
          </div>
          <div className="form-group">
            <label htmlFor="contactInfo.alternativePhone">
              {t('admin.persons.form.alternativePhone', 'Alternative Phone')}
            </label>
            <input
              type="tel"
              id="contactInfo.alternativePhone"
              name="contactInfo.alternativePhone"
              value={formData.contactInfo.alternativePhone}
              onChange={handleInputChange}
              maxLength={MAX_LENGTHS.alternativePhone}
              className={fieldErrors['contactInfo.alternativePhone'] ? 'error' : ''}
            />
            {fieldErrors['contactInfo.alternativePhone'] && (
              <div className="field-error">{fieldErrors['contactInfo.alternativePhone']}</div>
            )}
          </div>
        </div>
      </div>

      {/* Team Assignment Section - Only show for new persons and when enabled */}
      {!isEditMode && showTeamAssignment && (
        <div className="form-section">
          <h3>{t('admin.persons.form.floorballAssignment', 'Floorball Team Assignment (Optional)')}</h3>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="teamId">
                {t('admin.persons.form.team', 'Team')}
              </label>
              <SearchableInfiniteDropdown
                placeholder={t('admin.persons.form.selectTeam', 'Select a team...')}
                value={formData.teamId || ''}
                onChange={handleTeamChange}
                onSearch={floorballTeamSearchService.searchTeams}
                emptyMessage={t('admin.persons.form.noTeams', 'No teams found')}
                searchPlaceholder={t('admin.persons.form.searchTeams', 'Search teams...')}
                className={fieldErrors.teamId ? 'error' : ''}
              />
              {fieldErrors.teamId && (
                <div className="field-error">{fieldErrors.teamId}</div>
              )}
            </div>
          </div>
          
          {formData.teamId && (
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="position">
                  {t('admin.persons.form.position', 'Position')} <span className="required">*</span>
                </label>
                <select
                  id="position"
                  name="position"
                  value={formData.position || ''}
                  onChange={handleInputChange}
                  required
                  className={fieldErrors.position ? 'error' : ''}
                >
                  <option value="">{t('admin.persons.form.selectPosition', 'Select position...')}</option>
                  {Object.values(FloorballPosition).map(pos => (
                    <option key={pos} value={pos}>
                      {t(`admin.persons.form.positions.${pos.toLowerCase()}`, pos)}
                    </option>
                  ))}
                </select>
                {fieldErrors.position && (
                  <div className="field-error">{fieldErrors.position}</div>
                )}
              </div>
              
              <div className="form-group">
                <label htmlFor="jerseyNumber">
                  {t('admin.persons.form.jerseyNumber', 'Jersey Number')}
                </label>
                <input
                  type="number"
                  id="jerseyNumber"
                  name="jerseyNumber"
                  value={formData.jerseyNumber || ''}
                  onChange={handleInputChange}
                  min="1"
                  max="99"
                  className={fieldErrors.jerseyNumber ? 'error' : ''}
                />
                {fieldErrors.jerseyNumber && (
                  <div className="field-error">{fieldErrors.jerseyNumber}</div>
                )}
                <div className="field-hint">
                  {t('admin.persons.form.jerseyNumberHint', 'Optional: 1-99')}
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {error && <div className="form-error">{error}</div>}

      <div className="form-actions">
        <button
          type="button"
          className="cancel-button"
          onClick={handleCancel}
          disabled={loading}
        >
          {t('admin.persons.actions.cancel', 'Cancel')}
        </button>
        <button
          type="submit"
          className="submit-button"
          disabled={loading}
        >
          {loading 
            ? t('admin.persons.actions.saving', 'Saving...')
            : t(
                isEditMode 
                  ? 'admin.persons.actions.update'
                  : 'admin.persons.actions.create',
                isEditMode ? 'Update Person' : 'Create Person'
              )
          }
        </button>
      </div>
    </form>
  );
};

export default PersonForm; 