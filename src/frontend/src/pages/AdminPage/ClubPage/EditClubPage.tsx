import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import AdminPageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import ClubForm from './ClubForm';
import { clubService, type Club, type ClubRequest } from '../../../api/common/clubService';

function toDmyFromIso(iso: string): string {
  const dt = new Date(iso);
  if (Number.isNaN(dt.getTime())) return '';
  const dd = String(dt.getUTCDate()).padStart(2, '0');
  const mm = String(dt.getUTCMonth() + 1).padStart(2, '0');
  const yyyy = dt.getUTCFullYear();
  return `${dd}-${mm}-${yyyy}`;
}

function EditClubPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id } = useParams();
  const [loading, setLoading] = useState<boolean>(true);
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [club, setClub] = useState<Club | null>(null);

  useEffect(() => {
    const load = async () => {
      if (!id) return;
      setError(null);
      try {
        const data = await clubService.getById(id);
        setClub(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : String(err));
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [id]);

  const initialValues: ClubRequest | undefined = club
    ? {
        name: club.name,
        city: club.city,
        country: club.country,
        foundingDate: toDmyFromIso(club.foundingDate),
        websiteUrl: club.websiteUrl,
        logoUrl: club.logoUrl,
        contactEmail: club.contactEmail
      }
    : undefined;

  const handleSubmit = async (payload: ClubRequest) => {
    if (!id) return;
    setError(null);
    setSubmitting(true);
    try {
      await clubService.update(id, payload);
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
      setError(err instanceof Error ? err.message : String(err));
    }
  };

  return (
    <AdminPageTemplate title={t('clubs.edit.title', 'Edit Club')}>
      <div className="clubs-page">
        <h2>{t('clubs.edit.title', 'Edit Club')}</h2>
        <ErrorPopup message={error} />
        {loading && <p>{t('common.loading', 'Loading...')}</p>}
        {!loading && initialValues && (
          <ClubForm
            initialValues={initialValues}
            submitting={submitting}
            onSubmit={handleSubmit}
            onDelete={handleDelete}
          />
        )}
      </div>
    </AdminPageTemplate>
  );
}

export default EditClubPage;


