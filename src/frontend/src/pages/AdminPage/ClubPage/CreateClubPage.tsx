import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import AdminPageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import ClubForm, { type ClubFormValues } from './ClubForm';
import { clubService, type ClubRequest } from '../../../api/common/clubService';

function CreateClubPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const initialValues: ClubFormValues = {
    name: '',
    city: '',
    country: '',
    foundingDate: '',
    websiteUrl: '',
    logoUrl: '',
    contactEmail: ''
  };

  const handleSubmit = async (payload: ClubRequest) => {
    setError(null);
    setSubmitting(true);
    try {
      await clubService.create(payload);
      navigate('/admin/clubs');
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <AdminPageTemplate title={t('clubs.create.title', 'Create Club')}>
      <div className="clubs-page">
        <h2>{t('clubs.create.title', 'Create Club')}</h2>
        <ErrorPopup message={error} />
        <ClubForm initialValues={initialValues} submitting={submitting} onSubmit={handleSubmit} />
      </div>
    </AdminPageTemplate>
  );
}

export default CreateClubPage;


