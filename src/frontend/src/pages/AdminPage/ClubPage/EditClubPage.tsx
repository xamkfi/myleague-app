import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import AdminPageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import ClubForm from './ClubForm';
import ClubAdminsPicker, { type ClubAdminSelection } from './ClubAdminsPicker';
import { resolveClubAdminUserIds } from './resolveClubAdminUserIds';
import { clubService, type Club, type ClubRequest } from '../../../api/common/clubService';
import { clubEmail, clubPublicUrl, clubText } from '../../../utils/clubDisplay';
import { mapDeletionError } from '../../../utils/mapDeletionError';
import './ClubForm.scss';

function toDateInputValue(iso: string | null | undefined): string {
  if (!iso) return '';
  const dt = new Date(iso);
  if (Number.isNaN(dt.getTime())) return '';
  const yyyy = dt.getUTCFullYear();
  if (yyyy <= 1) return '';
  const mm = String(dt.getUTCMonth() + 1).padStart(2, '0');
  const dd = String(dt.getUTCDate()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}`;
}

function EditClubPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id } = useParams();
  const [loading, setLoading] = useState<boolean>(true);
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [club, setClub] = useState<Club | null>(null);
  const [admins, setAdmins] = useState<ClubAdminSelection[]>([]);

  useEffect(() => {
    const load = async () => {
      if (!id) return;
      setError(null);
      try {
        const [data, admins] = await Promise.all([
          clubService.getById(id),
          clubService.getAdmins(id),
        ]);
        setClub(data);
        setAdmins(admins.map((admin) => ({
          userId: admin.userId,
          personId: admin.personId,
          firstName: admin.firstName,
          lastName: admin.lastName,
          email: admin.email,
        })));
      } catch (err) {
        setError(err instanceof Error ? err.message : String(err));
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [id]);

  const initialValues = useMemo<ClubRequest | undefined>(
    () =>
      club
        ? {
            name: club.name ?? '',
            city: clubText(club.city) ?? '',
            country: clubText(club.country) ?? '',
            foundingDate: toDateInputValue(club.foundingDate),
            websiteUrl: clubPublicUrl(club.websiteUrl) ?? '',
            logoUrl: clubPublicUrl(club.logoUrl) ?? '',
            contactEmail: clubEmail(club.contactEmail) ?? '',
          }
        : undefined,
    [club]
  );

  const handleSubmit = async (payload: ClubRequest) => {
    if (!id) return;
    setError(null);
    setSubmitting(true);
    try {
      await clubService.update(id, payload);
      const userIds = await resolveClubAdminUserIds(admins, id);
      await clubService.setAdmins(id, userIds);
      navigate(`/admin/clubs/${id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!id) return;
    try {
      await clubService.remove(id);
      navigate('/admin/clubs');
    } catch (err) {
      setError(mapDeletionError(err, t) ?? (err instanceof Error ? err.message : String(err)));
    }
  };

  return (
    <AdminPageTemplate title={t('clubs.edit.title', 'Edit Club')}>
      <div className="club-editor">
        <header className="club-editor__header">
          {club?.name && <p className="club-editor__club-name">{club.name}</p>}
          <h2>{t('clubs.edit.title')}</h2>
          <p>{t('clubs.form.lead')}</p>
        </header>
        <ErrorPopup message={error} />
        {loading && <p>{t('common.loading')}</p>}
        {!loading && initialValues && (
          <ClubForm
            initialValues={initialValues}
            submitting={submitting}
            onSubmit={handleSubmit}
            onDelete={handleDelete}
            beforeActions={<ClubAdminsPicker selectedAdmins={admins} onChange={setAdmins} />}
          />
        )}
      </div>
    </AdminPageTemplate>
  );
}

export default EditClubPage;
