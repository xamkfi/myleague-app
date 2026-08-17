import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import AdminPageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import ClubForm from './ClubForm';
import ClubAdminsPicker, { type ClubAdminSelection } from './ClubAdminsPicker';
import { resolveClubAdminUserIds } from './resolveClubAdminUserIds';
import { clubService, type ClubRequest } from '../../../api/common/clubService';

function CreateClubPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [admins, setAdmins] = useState<ClubAdminSelection[]>([]);

  const handleSubmit = async (payload: ClubRequest) => {
    setError(null);
    setSubmitting(true);
    try {
      const created = await clubService.create(payload);
      if (admins.length > 0) {
        const userIds = await resolveClubAdminUserIds(admins, created.id);
        await clubService.setAdmins(created.id, userIds);
      }
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
        <ClubAdminsPicker selectedAdmins={admins} onChange={setAdmins} />
        <ClubForm submitting={submitting} onSubmit={handleSubmit} />
      </div>
    </AdminPageTemplate>
  );
}

export default CreateClubPage;


