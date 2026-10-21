import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import Button from '../../../components/Button/Button';
import ConfirmationDialog from '../../../components/ConfirmationDialog/ConfirmationDialog';
import { advertisementService } from '../../../api/common/advertisementService';
import type {
  AdSlot,
  Advertisement,
  AdvertisementRequest,
} from '../../../types/admin/advertisementTypes';
import { AD_SLOTS } from '../../../types/admin/advertisementTypes';
import AdvertisementForm from './components/AdvertisementForm';
import './AdvertisementsManagementPage.scss';

type AdStatus = 'active' | 'inactive' | 'scheduled' | 'expired';

function getStatus(ad: Advertisement, now: Date): AdStatus {
  if (!ad.isActive) {
    return 'inactive';
  }

  if (ad.startsAt && new Date(ad.startsAt) > now) {
    return 'scheduled';
  }

  if (ad.endsAt && new Date(ad.endsAt) <= now) {
    return 'expired';
  }

  return 'active';
}

function AdvertisementsManagementPage() {
  const { t, i18n } = useTranslation();
  const [ads, setAds] = useState<Advertisement[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editing, setEditing] = useState<Advertisement | null>(null);
  const [newAdSlot, setNewAdSlot] = useState<AdSlot>('LeftTop');
  const [adToDelete, setAdToDelete] = useState<Advertisement | null>(null);

  const loadAds = useCallback(async (): Promise<void> => {
    setAds(await advertisementService.getAll());
  }, []);

  useEffect(() => {
    let isMounted = true;

    const initialize = async (): Promise<void> => {
      try {
        setIsLoading(true);
        setErrorMessage(null);
        await loadAds();
      } catch (error) {
        if (isMounted) {
          setErrorMessage(
            error instanceof Error
              ? error.message
              : t('admin.siteContent.ads.loadFailed', 'Mainosten lataus epäonnistui.'),
          );
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    void initialize();

    return () => {
      isMounted = false;
    };
  }, [loadAds, t]);

  useEffect(() => {
    if (!successMessage) {
      return undefined;
    }

    const timeout = setTimeout(() => setSuccessMessage(null), 5000);
    return () => clearTimeout(timeout);
  }, [successMessage]);

  const closeForm = (): void => {
    setIsFormOpen(false);
    setEditing(null);
  };

  const handleSave = async (payload: AdvertisementRequest): Promise<boolean> => {
    try {
      setIsSaving(true);
      setErrorMessage(null);

      if (editing) {
        await advertisementService.update(editing.id, payload);
        setSuccessMessage(t('admin.siteContent.ads.updateSuccess', 'Mainos päivitetty.'));
      } else {
        await advertisementService.create(payload);
        setSuccessMessage(t('admin.siteContent.ads.createSuccess', 'Mainos lisätty.'));
      }

      await loadAds();
      closeForm();
      return true;
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : t('admin.siteContent.ads.saveFailed', 'Tallennus epäonnistui.'),
      );
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (): Promise<void> => {
    if (!adToDelete) {
      return;
    }

    try {
      setIsSaving(true);
      await advertisementService.remove(adToDelete.id);
      setSuccessMessage(t('admin.siteContent.ads.deleteSuccess', 'Mainos poistettu.'));
      setAdToDelete(null);
      await loadAds();
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : t('admin.siteContent.ads.deleteFailed', 'Poisto epäonnistui.'),
      );
    } finally {
      setIsSaving(false);
    }
  };

  const formatDate = (iso: string | null): string =>
    iso
      ? new Date(iso).toLocaleString(i18n.language, { dateStyle: 'short', timeStyle: 'short' })
      : '–';

  const now = new Date();

  return (
    <PageTemplate title={t('admin.siteContent.ads.pageTitle', 'Mainokset')}>
      <div className="ads-admin-page">
        {successMessage && (
          <p className="ads-admin-page__alert ads-admin-page__alert--success">{successMessage}</p>
        )}
        {errorMessage && (
          <p className="ads-admin-page__alert ads-admin-page__alert--error">{errorMessage}</p>
        )}

        <p className="ads-admin-page__description">
          {t(
            'admin.siteContent.ads.description',
            'Etusivun mainospaikat. Leveällä näytöllä reunapaikat näkyvät sisällön vasemmalla ja oikealla puolella; kapeammilla näytöillä ja mobiilissa kaikki mainokset näkyvät uutisten alla. Jos paikassa on useampi mainos, ne vuorottelevat järjestyksen mukaan.',
          )}
        </p>

        {isFormOpen ? (
          <AdvertisementForm
            advertisement={editing}
            defaultSlot={newAdSlot}
            isSaving={isSaving}
            onCancel={closeForm}
            onSave={handleSave}
          />
        ) : isLoading ? (
          <p>{t('common.loading', 'Ladataan...')}</p>
        ) : (
          AD_SLOTS.map((slot) => {
            const slotAds = ads.filter((ad) => ad.slot === slot);

            return (
              <section key={slot} className="ads-admin-page__slot">
                <div className="ads-admin-page__slot-header">
                  <h2>{t(`admin.siteContent.ads.slots.${slot}`)}</h2>
                  <Button
                    size="sm"
                    onClick={() => {
                      setEditing(null);
                      setNewAdSlot(slot);
                      setIsFormOpen(true);
                    }}
                  >
                    {t('admin.siteContent.ads.add', 'Lisää mainos')}
                  </Button>
                </div>

                {slotAds.length === 0 ? (
                  <p className="ads-admin-page__empty">
                    {t('admin.siteContent.ads.emptySlot', 'Ei mainoksia tässä paikassa.')}
                  </p>
                ) : (
                  <ul className="ads-admin-page__list">
                    {slotAds.map((ad) => {
                      const status = getStatus(ad, now);

                      return (
                        <li key={ad.id} className="ads-admin-page__card">
                          <img className="ads-admin-page__thumb" src={ad.imageUrl} alt="" />
                          <div className="ads-admin-page__info">
                            <h3>{ad.name}</h3>
                            <span className={`ads-admin-page__status ads-admin-page__status--${status}`}>
                              {t(`admin.siteContent.ads.status.${status}`)}
                            </span>
                            <p>
                              {ad.startsAt || ad.endsAt
                                ? t('admin.siteContent.ads.schedule', 'Näkyvyys: {{start}} – {{end}}', {
                                    start: formatDate(ad.startsAt),
                                    end: formatDate(ad.endsAt),
                                  })
                                : t('admin.siteContent.ads.scheduleAlways', 'Näkyvyys: aina voimassa')}
                            </p>
                            {ad.linkUrl && <p className="ads-admin-page__url">{ad.linkUrl}</p>}
                            <p>
                              {t('admin.siteContent.ads.sortOrderValue', 'Järjestys: {{order}}', {
                                order: ad.sortOrder,
                              })}
                            </p>
                          </div>
                          <div className="ads-admin-page__card-actions">
                            <Button
                              variant="secondary"
                              size="sm"
                              onClick={() => {
                                setEditing(ad);
                                setIsFormOpen(true);
                              }}
                            >
                              {t('common.edit', 'Muokkaa')}
                            </Button>
                            <Button variant="danger" size="sm" onClick={() => setAdToDelete(ad)}>
                              {t('common.delete', 'Poista')}
                            </Button>
                          </div>
                        </li>
                      );
                    })}
                  </ul>
                )}
              </section>
            );
          })
        )}
      </div>

      <ConfirmationDialog
        isOpen={adToDelete !== null}
        icon="⚠️"
        title={t('admin.siteContent.ads.confirmDeleteTitle', 'Poista mainos')}
        message={t('admin.siteContent.ads.confirmDelete', 'Poistetaanko mainos "{{name}}"?', {
          name: adToDelete?.name ?? '',
        })}
        confirmText={t('common.delete', 'Poista')}
        cancelText={t('common.cancel', 'Peruuta')}
        isLoading={isSaving}
        onConfirm={() => {
          void handleDelete();
        }}
        onCancel={() => setAdToDelete(null)}
      />
    </PageTemplate>
  );
}

export default AdvertisementsManagementPage;
