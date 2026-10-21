import { useRef, useState, type ChangeEvent, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import Button from '../../../../components/Button/Button';
import { advertisementService } from '../../../../api/common/advertisementService';
import { handleImageDeleteService } from '../../../../api/news/handleImageDeleteService';
import type {
  AdSlot,
  Advertisement,
  AdvertisementRequest,
} from '../../../../types/admin/advertisementTypes';
import { AD_SLOTS } from '../../../../types/admin/advertisementTypes';
import './AdvertisementForm.scss';

interface AdvertisementFormProps {
  advertisement: Advertisement | null;
  defaultSlot: AdSlot;
  isSaving: boolean;
  onCancel: () => void;
  /** Resolves to true when the advertisement was saved. */
  onSave: (payload: AdvertisementRequest) => Promise<boolean>;
}

/** ISO string → value for <input type="datetime-local"> in the browser's time zone. */
function toLocalInput(iso: string | null): string {
  if (!iso) {
    return '';
  }

  const date = new Date(iso);
  const offsetMs = date.getTimezoneOffset() * 60_000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}

function fromLocalInput(value: string): string | null {
  return value ? new Date(value).toISOString() : null;
}

function deleteUnusedImages(urls: string[]): void {
  urls.forEach((url) => {
    handleImageDeleteService(url).catch(() => {
      // Best effort: an orphaned upload does no harm on the public site.
    });
  });
}

function AdvertisementForm({
  advertisement,
  defaultSlot,
  isSaving,
  onCancel,
  onSave,
}: AdvertisementFormProps) {
  const { t } = useTranslation();
  const [name, setName] = useState(advertisement?.name ?? '');
  const [imageUrl, setImageUrl] = useState(advertisement?.imageUrl ?? '');
  const [linkUrl, setLinkUrl] = useState(advertisement?.linkUrl ?? '');
  const [altText, setAltText] = useState(advertisement?.altText ?? '');
  const [slot, setSlot] = useState<AdSlot>(advertisement?.slot ?? defaultSlot);
  const [isActive, setIsActive] = useState(advertisement?.isActive ?? true);
  const [sortOrder, setSortOrder] = useState(String(advertisement?.sortOrder ?? 0));
  const [startsAt, setStartsAt] = useState(toLocalInput(advertisement?.startsAt ?? null));
  const [endsAt, setEndsAt] = useState(toLocalInput(advertisement?.endsAt ?? null));
  const [isUploading, setIsUploading] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  // Images uploaded while this form was open. The saved advertisement keeps one of them
  // (or its original image); the rest are deleted so they do not pile up in storage.
  const uploadedUrls = useRef<string[]>([]);

  const isSideSlot = slot !== 'BottomCenter';
  const scheduleInvalid = Boolean(startsAt && endsAt && new Date(endsAt) <= new Date(startsAt));

  const handleImageChange = async (event: ChangeEvent<HTMLInputElement>): Promise<void> => {
    const file = event.target.files?.[0];
    event.target.value = '';

    if (!file) {
      return;
    }

    try {
      setIsUploading(true);
      setUploadError(null);
      const url = await advertisementService.uploadImage(file);
      uploadedUrls.current.push(url);
      setImageUrl(url);
    } catch (error) {
      setUploadError(
        error instanceof Error
          ? error.message
          : t('admin.siteContent.ads.uploadFailed', 'Kuvan lataus epäonnistui.'),
      );
    } finally {
      setIsUploading(false);
    }
  };

  const handleCancel = (): void => {
    deleteUnusedImages(uploadedUrls.current);
    uploadedUrls.current = [];
    onCancel();
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>): Promise<void> => {
    event.preventDefault();

    const parsedOrder = Number.parseInt(sortOrder, 10);
    const saved = await onSave({
      name: name.trim(),
      imageUrl,
      linkUrl: linkUrl.trim() || null,
      altText: altText.trim() || null,
      slot,
      isActive,
      sortOrder: Number.isNaN(parsedOrder) ? 0 : parsedOrder,
      startsAt: fromLocalInput(startsAt),
      endsAt: fromLocalInput(endsAt),
    });

    if (saved) {
      deleteUnusedImages(uploadedUrls.current.filter((url) => url !== imageUrl));
      uploadedUrls.current = [];
    }
  };

  return (
    <form
      className="advertisement-form"
      onSubmit={(event) => {
        void handleSubmit(event);
      }}
    >
      <h3>
        {advertisement
          ? t('admin.siteContent.ads.editTitle', 'Muokkaa mainosta')
          : t('admin.siteContent.ads.addTitle', 'Uusi mainos')}
      </h3>

      <label className="advertisement-form__field">
        <span>{t('admin.siteContent.ads.fields.name', 'Nimi')}</span>
        <input
          type="text"
          value={name}
          onChange={(event) => setName(event.target.value)}
          required
          maxLength={100}
        />
      </label>

      <label className="advertisement-form__field">
        <span>{t('admin.siteContent.ads.fields.slot', 'Paikka')}</span>
        <select value={slot} onChange={(event) => setSlot(event.target.value as AdSlot)}>
          {AD_SLOTS.map((value) => (
            <option key={value} value={value}>
              {t(`admin.siteContent.ads.slots.${value}`)}
            </option>
          ))}
        </select>
      </label>

      <div className="advertisement-form__field">
        <span>{t('admin.siteContent.ads.fields.image', 'Kuva')}</span>
        <p className="advertisement-form__hint">
          {isSideSlot
            ? t('admin.siteContent.ads.sizeHintSide', 'Suositus: 180 px leveä (esim. 360 × 600 px terävyyden vuoksi).')
            : t('admin.siteContent.ads.sizeHintBottom', 'Suositus: uutispalstan levyinen vaakabanneri (esim. 1600 × 400 px).')}
        </p>
        {imageUrl && (
          <img
            className={`advertisement-form__preview ${isSideSlot ? 'advertisement-form__preview--side' : ''}`}
            src={imageUrl}
            alt=""
          />
        )}
        <input
          id="advertisement-image"
          className="advertisement-form__file-input"
          type="file"
          accept="image/png,image/jpeg,image/gif,image/webp"
          onChange={(event) => {
            void handleImageChange(event);
          }}
          disabled={isUploading || isSaving}
        />
        <label htmlFor="advertisement-image" className="advertisement-form__file-button">
          {isUploading
            ? t('common.loading', 'Ladataan...')
            : imageUrl
              ? t('admin.siteContent.ads.replaceImage', 'Vaihda kuva')
              : t('admin.siteContent.ads.uploadImage', 'Lataa kuva')}
        </label>
        {uploadError && <p className="advertisement-form__error">{uploadError}</p>}
      </div>

      <label className="advertisement-form__field">
        <span>{t('admin.siteContent.ads.fields.altText', 'Kuvan vaihtoehtoinen teksti')}</span>
        <input
          type="text"
          value={altText}
          onChange={(event) => setAltText(event.target.value)}
          maxLength={200}
        />
      </label>

      <label className="advertisement-form__field">
        <span>{t('admin.siteContent.ads.fields.linkUrl', 'Linkki (avautuu uuteen välilehteen)')}</span>
        <input
          type="url"
          value={linkUrl}
          onChange={(event) => setLinkUrl(event.target.value)}
          maxLength={500}
          placeholder="https://"
        />
      </label>

      <div className="advertisement-form__row">
        <label className="advertisement-form__field">
          <span>{t('admin.siteContent.ads.fields.startsAt', 'Näkyy alkaen')}</span>
          <input
            type="datetime-local"
            value={startsAt}
            onChange={(event) => setStartsAt(event.target.value)}
          />
        </label>
        <label className="advertisement-form__field">
          <span>{t('admin.siteContent.ads.fields.endsAt', 'Näkyy asti')}</span>
          <input
            type="datetime-local"
            value={endsAt}
            onChange={(event) => setEndsAt(event.target.value)}
          />
        </label>
      </div>
      {scheduleInvalid && (
        <p className="advertisement-form__error">
          {t('admin.siteContent.ads.scheduleInvalid', 'Päättymisajan pitää olla alkamisajan jälkeen.')}
        </p>
      )}

      <div className="advertisement-form__row">
        <label className="advertisement-form__field">
          <span>{t('admin.siteContent.ads.fields.sortOrder', 'Järjestys paikan sisällä')}</span>
          <input
            type="number"
            min={0}
            value={sortOrder}
            onChange={(event) => setSortOrder(event.target.value)}
          />
        </label>
        <label className="advertisement-form__checkbox">
          <input
            type="checkbox"
            checked={isActive}
            onChange={(event) => setIsActive(event.target.checked)}
          />
          <span>{t('admin.siteContent.ads.fields.isActive', 'Aktiivinen')}</span>
        </label>
      </div>

      <div className="advertisement-form__actions">
        <Button type="button" variant="secondary" onClick={handleCancel} disabled={isSaving}>
          {t('common.cancel', 'Peruuta')}
        </Button>
        <Button
          type="submit"
          isLoading={isSaving}
          disabled={!name.trim() || !imageUrl || isUploading || scheduleInvalid}
        >
          {t('common.save', 'Tallenna')}
        </Button>
      </div>
    </form>
  );
}

export default AdvertisementForm;
