import { useEffect, useRef, useState, type ChangeEvent, type DragEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { handleImageUploadService } from '../../../../api/news/handleImageUploadService';
import FieldError from './FieldError';
import { MAX_IMAGE_BYTES } from './newsForm';

interface NewsImageFieldProps {
  value: string;
  onChange: (url: string) => void;
  /** Called with every uploaded URL so the editor can delete uploads that end up unused. */
  onUploaded: (url: string) => void;
}

function UploadIcon() {
  return (
    <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <rect x="3" y="3" width="18" height="18" rx="3" />
      <circle cx="8.5" cy="8.5" r="1.5" />
      <path d="m21 15-5-5L5 21" />
    </svg>
  );
}

/** Main picture of an article: drag and drop or pick a file, then replace or remove it. */
export default function NewsImageField({ value, onChange, onUploaded }: NewsImageFieldProps) {
  const { t } = useTranslation();
  const inputRef = useRef<HTMLInputElement>(null);
  const [uploading, setUploading] = useState(false);
  const [uploadError, setUploadError] = useState('');
  const [dragActive, setDragActive] = useState(false);
  const [broken, setBroken] = useState(false);

  useEffect(() => {
    setBroken(false);
  }, [value]);

  const upload = async (file: File): Promise<void> => {
    if (!file.type.startsWith('image/')) {
      setUploadError(t('admin.news.errors.invalid_image'));
      return;
    }
    if (file.size > MAX_IMAGE_BYTES) {
      setUploadError(t('admin.news.errors.image_too_large'));
      return;
    }

    try {
      setUploadError('');
      setUploading(true);
      const url: string = await handleImageUploadService(file);
      onUploaded(url);
      onChange(url);
    } catch (uploadFailure: unknown) {
      console.error('Main image upload failed:', uploadFailure);
      setUploadError(t('admin.news.errors.upload_failed'));
    } finally {
      setUploading(false);
    }
  };

  const handleFileInput = (event: ChangeEvent<HTMLInputElement>): void => {
    const file: File | undefined = event.target.files?.[0];
    event.target.value = '';
    if (file) {
      void upload(file);
    }
  };

  const handleDragOver = (event: DragEvent<HTMLDivElement>): void => {
    event.preventDefault();
    if (!uploading) {
      setDragActive(true);
    }
  };

  const handleDragLeave = (event: DragEvent<HTMLDivElement>): void => {
    if (!event.currentTarget.contains(event.relatedTarget as Node | null)) {
      setDragActive(false);
    }
  };

  const handleDrop = (event: DragEvent<HTMLDivElement>): void => {
    event.preventDefault();
    setDragActive(false);
    const file: File | undefined = event.dataTransfer.files?.[0];
    if (file && !uploading) {
      void upload(file);
    }
  };

  const openPicker = (): void => {
    inputRef.current?.click();
  };

  return (
    <div className="news-field">
      <input
        ref={inputRef}
        type="file"
        accept="image/*"
        className="news-image-field__input"
        onChange={handleFileInput}
        tabIndex={-1}
        aria-hidden="true"
      />

      {value ? (
        <div className="news-image-field__preview">
          {broken ? (
            <div className="news-image-field__broken">{t('admin.news.image_broken')}</div>
          ) : (
            <img
              src={value}
              alt={t('admin.news.main_picture_preview')}
              className="news-image-field__image"
              onError={() => setBroken(true)}
            />
          )}
          <div className="news-image-field__actions">
            <button type="button" className="news-field__button" onClick={openPicker} disabled={uploading}>
              {uploading ? t('admin.news.uploading') : t('admin.news.replace')}
            </button>
            <button
              type="button"
              className="news-field__button news-field__button--danger"
              onClick={() => {
                onChange('');
                setUploadError('');
              }}
              disabled={uploading}
            >
              {t('admin.news.remove_image')}
            </button>
          </div>
        </div>
      ) : (
        <div
          className={[
            'news-image-field__dropzone',
            dragActive ? 'news-image-field__dropzone--active' : '',
            uploading ? 'news-image-field__dropzone--uploading' : '',
          ]
            .filter(Boolean)
            .join(' ')}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onDrop={handleDrop}
        >
          <span className="news-image-field__icon">
            <UploadIcon />
          </span>
          <p className="news-image-field__prompt">
            {uploading
              ? t('admin.news.uploading')
              : dragActive
                ? t('admin.news.drop_image')
                : t('admin.news.drag_image_here')}
          </p>
          <button type="button" className="news-field__button" onClick={openPicker} disabled={uploading}>
            {t('admin.news.choose_image')}
          </button>
          <p className="news-image-field__hint">{t('admin.news.image_hint')}</p>
        </div>
      )}

      <FieldError id="news-image-error" message={uploadError} />
    </div>
  );
}
