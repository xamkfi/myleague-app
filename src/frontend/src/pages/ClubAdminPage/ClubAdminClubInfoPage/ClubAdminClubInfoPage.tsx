import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ClubAdminPageTemplate from '../components/ClubAdminPageTemplate';
import ClubForm from '../../AdminPage/ClubPage/ClubForm';
import { clubService, type Club, type ClubRequest } from '../../../api/common/clubService';
import './ClubAdminClubInfoPage.scss';

/**
 * Lets a club admin edit their club's information (name, city, logo, contacts...).
 * Deleting the club is intentionally not possible here; only site admins can delete clubs.
 */
function ClubAdminClubInfoPage() {
  const { t } = useTranslation();
  const { clubId } = useParams<{ clubId: string }>();

  const [club, setClub] = useState<Club | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      if (!clubId) return;
      try {
        const data = await clubService.getById(clubId);
        if (!cancelled) setClub(data);
      } catch (err: unknown) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : t('clubAdmin.loadClubError', 'Failed to load the club'));
        }
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    };

    void load();
    return () => { cancelled = true; };
  }, [clubId, t]);

  const initialValues = useMemo<ClubRequest | undefined>(() => {
    if (!club) return undefined;
    return {
      name: club.name,
      city: club.city ?? '',
      country: club.country ?? '',
      foundingDate: club.foundingDate ? club.foundingDate.substring(0, 10) : null,
      websiteUrl: club.websiteUrl ?? '',
      logoUrl: club.logoUrl ?? '',
      contactEmail: club.contactEmail ?? '',
    };
  }, [club]);

  const handleSubmit = async (payload: ClubRequest) => {
    if (!clubId) return;
    setIsSaving(true);
    setSuccessMessage(null);
    try {
      await clubService.update(clubId, payload);
      setSuccessMessage(t('clubAdmin.clubInfoSaved', 'Club information has been saved.'));
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <ClubAdminPageTemplate title={club ? `${club.name} – ${t('clubAdmin.clubInfoTitle', 'Club information')}` : t('clubAdmin.clubInfoTitle', 'Club information')}>
      <Link to="/club-admin" className="club-admin-back-link">
        ← {t('clubAdmin.backToClubs', 'Back to my clubs')}
      </Link>

      {isLoading && <div className="club-admin-loading">{t('common.loading', 'Loading...')}</div>}
      {error && <div className="club-admin-error">{error}</div>}
      {successMessage && <div className="club-admin-success">{successMessage}</div>}

      {!isLoading && !error && club && (
        <div className="club-admin-club-info-card">
          <ClubForm
            initialValues={initialValues}
            submitting={isSaving}
            onSubmit={handleSubmit}
          />
        </div>
      )}
    </ClubAdminPageTemplate>
  );
}

export default ClubAdminClubInfoPage;
