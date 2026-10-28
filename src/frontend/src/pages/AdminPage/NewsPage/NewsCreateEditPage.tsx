import { useCallback, useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import RichTextEditor, { extractRichTextImageUrls } from '../../../components/RichTextEditor';
import Button from '../../../components/Button/Button';
import ConfirmationDialog from '../../../components/ConfirmationDialog/ConfirmationDialog';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import LoadingSpinner from '../../../components/LoadingSpinner/LoadingSpinner';
import NewsTitleFields from './components/NewsTitleFields';
import NewsImageField from './components/NewsImageField';
import NewsClassificationFields from './components/NewsClassificationFields';
import NewsDetailsFields from './components/NewsDetailsFields';
import PreviewNews from './components/PreviewNews';
import {
  AUTHOR_MAX_LENGTH,
  SUMMARY_MAX_LENGTH,
  TITLE_MAX_LENGTH,
  TITLE_MIN_LENGTH,
  emptyNewsForm,
  sportKindForCategory,
  type NewsFormData,
  type NewsFormErrors,
} from './components/newsForm';
import { CreateNewsService } from '../../../api/news/CreateNewsService';
import { UpdateNewsService } from '../../../api/news/UpdateNewsService';
import { handleImageDeleteService } from '../../../api/news/handleImageDeleteService';
import { singleNewsService } from '../../../api/news/singleNewsService';
import { useAdminReturnTo } from '../../../hooks/useAdminReturnTo';
import { unwrapApiErrorMessage } from '../../../api/utils/ParseErrorResponse';
import './NewsCreateEditPage.scss';
import './styles/NewsFields.scss';

const collectStoredImageUrls = (html: string, mainPicture: string): string[] => {
  const urls: string[] = extractRichTextImageUrls(html);
  if (mainPicture.trim()) {
    urls.push(mainPicture.trim());
  }
  return Array.from(new Set(urls));
};

const toNullIfEmpty = (input: string | undefined | null): string | null => {
  const trimmed: string = input?.trim() ?? '';
  return trimmed === '' ? null : trimmed;
};

export default function NewsCreateEditPage() {
  const { t } = useTranslation();
  const { id } = useParams();
  const isEditMode = Boolean(id);
  const navigate = useNavigate();
  const { returnTo } = useAdminReturnTo();

  const [value, setValue] = useState('');
  const [preview, setPreview] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isUploadingContent, setIsUploadingContent] = useState(false);
  const [isLoadingArticle, setIsLoadingArticle] = useState(isEditMode);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [newsData, setNewsData] = useState<NewsFormData>(emptyNewsForm);
  const [errors, setErrors] = useState<NewsFormErrors>({});
  const [contentError, setContentError] = useState('');
  /** Images the saved article referenced when it was loaded. */
  const originalImageUrlsRef = useRef<string[]>([]);
  /** Images uploaded in this session, whether or not they are still used. */
  const uploadedImageUrlsRef = useRef<Set<string>>(new Set());
  const leaveTimeoutRef = useRef<number | null>(null);

  const updateField = useCallback(<K extends keyof NewsFormData>(field: K, fieldValue: NewsFormData[K]): void => {
    setNewsData((previous) => ({ ...previous, [field]: fieldValue }));
  }, []);

  const rememberUpload = useCallback((url: string): void => {
    uploadedImageUrlsRef.current.add(url);
  }, []);

  /** Deletes stored images the saved article no longer uses, including uploads that were removed again. */
  const deleteOrphanedImages = (keptHtml: string, keptMainPicture: string): void => {
    const kept = new Set(collectStoredImageUrls(keptHtml, keptMainPicture));
    const candidates = new Set([...originalImageUrlsRef.current, ...uploadedImageUrlsRef.current]);
    candidates.forEach((url) => {
      if (kept.has(url)) {
        return;
      }
      handleImageDeleteService(url).catch((deleteError: unknown) => {
        console.error('Failed to delete orphaned image:', deleteError);
      });
    });
  };

  useEffect(() => {
    return () => {
      if (leaveTimeoutRef.current !== null) {
        window.clearTimeout(leaveTimeoutRef.current);
      }
    };
  }, []);

  useEffect(() => {
    if (!isEditMode || !id) {
      return;
    }

    const fetchNewsArticle = async (): Promise<void> => {
      try {
        setIsLoadingArticle(true);
        setError(null);
        const article = await singleNewsService(id);
        setNewsData({
          title: article.title || '',
          mainPicture: article.mainImage || '',
          summary: article.summary || '',
          author: article.author || '',
          category: article.category || '',
          sportCategory: article.sportCategory || '',
          teamCategory: article.teamCategory || '',
          tags: article.tags || [],
        });
        setValue(article.contentHtml || '');
        originalImageUrlsRef.current = collectStoredImageUrls(
          article.contentHtml || '',
          article.mainImage || '',
        );
      } catch (loadError: unknown) {
        console.error('Failed to fetch news article:', loadError);
        setError(t('admin.news.errors.loadFailed'));
      } finally {
        setIsLoadingArticle(false);
      }
    };

    void fetchNewsArticle();
  }, [id, isEditMode, t]);

  const validateInputs = (): boolean => {
    const newErrors: NewsFormErrors = {};
    const title: string = newsData.title.trim();

    if (!title) {
      newErrors.title = t('admin.news.errors.title_required');
    } else if (title.length < TITLE_MIN_LENGTH) {
      newErrors.title = t('admin.news.errors.title_too_short');
    } else if (title.length > TITLE_MAX_LENGTH) {
      newErrors.title = t('admin.news.errors.title_too_long');
    }

    if (newsData.author.trim().length > AUTHOR_MAX_LENGTH) {
      newErrors.author = t('admin.news.errors.author_too_long');
    }

    if (newsData.summary.trim().length > SUMMARY_MAX_LENGTH) {
      newErrors.summary = t('admin.news.errors.summary_too_long');
    }

    const newContentError: string = value.trim() ? '' : t('admin.news.errors.content_required');
    setErrors(newErrors);
    setContentError(newContentError);

    return Object.keys(newErrors).length === 0 && !newContentError;
  };

  const scrollToFirstError = (): void => {
    // Wait for the error messages to render before looking for them.
    window.requestAnimationFrame(() => {
      document
        .querySelector('.news-field__error, .news-editor__content-error')
        ?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    });
  };

  const handlePublishClick = (): void => {
    setError(null);
    if (validateInputs()) {
      setConfirmOpen(true);
      return;
    }
    setPreview(false);
    scrollToFirstError();
  };

  const buildPayload = () => ({
    title: newsData.title.trim(),
    contentHtml: value.trim(),
    mainImage: toNullIfEmpty(newsData.mainPicture),
    summary: toNullIfEmpty(newsData.summary),
    author: toNullIfEmpty(newsData.author),
    category: toNullIfEmpty(newsData.category),
    sportCategory: toNullIfEmpty(newsData.sportCategory),
    teamCategory: toNullIfEmpty(newsData.teamCategory),
    tags: newsData.tags.map((tag) => tag.trim()).filter((tag) => tag !== ''),
  });

  const leaveAfterSuccess = (message: string): void => {
    setSuccessMessage(message);
    leaveTimeoutRef.current = window.setTimeout(() => {
      navigate(returnTo);
    }, 900);
  };

  const handleConfirmPublish = async (): Promise<void> => {
    const payload = buildPayload();
    try {
      setIsSaving(true);
      setError(null);

      if (isEditMode && id) {
        await UpdateNewsService(id, {
          ...payload,
          imageUrls: payload.mainImage ? [payload.mainImage] : null,
        });
      } else {
        await CreateNewsService(payload);
      }

      deleteOrphanedImages(payload.contentHtml, payload.mainImage ?? '');
      setConfirmOpen(false);
      leaveAfterSuccess(
        isEditMode ? t('admin.news.success.update_success') : t('admin.news.success.publish_success'),
      );
    } catch (saveError: unknown) {
      const fallback: string = isEditMode
        ? t('admin.news.errors.update_error')
        : t('admin.news.errors.publish_error');
      setError(unwrapApiErrorMessage(saveError, fallback));
    } finally {
      setIsSaving(false);
    }
  };

  const pageTitle: string = isEditMode ? t('admin.news.edit') : t('admin.news.create');
  const busy: boolean = isSaving || isUploadingContent;

  if (isLoadingArticle) {
    return (
      <PageTemplate title={t('admin.news.loading')}>
        <div className="news-editor news-editor--loading">
          <LoadingSpinner text={t('admin.news.loading_article')} />
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={pageTitle}>
      <div className="news-editor">
        {successMessage && (
          <div className="success-toast" role="status">
            <p>{successMessage}</p>
          </div>
        )}

        <ErrorPopup message={error} />

        <header className="news-editor__topbar">
          <div className="news-editor__intro">
            <p className="news-editor__eyebrow">
              {isEditMode ? t('admin.news.edit_mode') : t('admin.news.create_mode')}
            </p>
            <h1 className="news-editor__title">{pageTitle}</h1>
            <p className="news-editor__hint">{t('admin.news.required_hint')}</p>
          </div>

          <div className="news-editor__actions">
            <Button
              variant="secondary"
              onClick={() => setPreview((current) => !current)}
              disabled={busy}
            >
              {preview ? t('admin.news.edit_content') : t('admin.news.preview')}
            </Button>
            <Button
              variant="primary"
              onClick={handlePublishClick}
              disabled={busy}
              isLoading={isSaving}
            >
              {isEditMode ? t('admin.news.update') : t('admin.news.publish')}
            </Button>
          </div>
        </header>

        <div hidden={preview} className="news-editor__layout">
          <section className="news-editor__card news-editor__card--title">
            <NewsTitleFields
              title={newsData.title}
              summary={newsData.summary}
              errors={errors}
              onTitleChange={(fieldValue) => updateField('title', fieldValue)}
              onSummaryChange={(fieldValue) => updateField('summary', fieldValue)}
            />
          </section>

          <section className="news-editor__card news-editor__card--content" aria-labelledby="news-content-heading">
            <div className="news-editor__section-heading">
              <h2 id="news-content-heading" className="news-editor__section-title">
                {t('admin.news.section_content')}
                <span className="news-editor__required" aria-hidden="true">*</span>
              </h2>
              {isUploadingContent && <LoadingSpinner size="sm" text={t('admin.news.uploading')} />}
            </div>
            {contentError && <p className="news-editor__content-error">{contentError}</p>}
            <div className={`news-editor__richtext${contentError ? ' news-editor__richtext--error' : ''}`}>
              <RichTextEditor
                value={value}
                onChange={setValue}
                onUploadingChange={setIsUploadingContent}
                onImageUploaded={rememberUpload}
                showMatchInsert
                matchSport={sportKindForCategory(newsData.sportCategory)}
              />
            </div>
          </section>

          <section className="news-editor__card news-editor__card--image" aria-labelledby="news-image-heading">
            <h2 id="news-image-heading" className="news-editor__section-title news-editor__section-title--small">
              {t('admin.news.main_picture')}
            </h2>
            <NewsImageField
              value={newsData.mainPicture}
              onChange={(url) => updateField('mainPicture', url)}
              onUploaded={rememberUpload}
            />
          </section>

          <section
            className="news-editor__card news-editor__card--classification"
            aria-labelledby="news-classification-heading"
          >
            <h2 id="news-classification-heading" className="news-editor__section-title news-editor__section-title--small">
              {t('admin.news.section_classification')}
            </h2>
            <NewsClassificationFields
              sportCategory={newsData.sportCategory}
              category={newsData.category}
              teamCategory={newsData.teamCategory}
              onSportCategoryChange={(fieldValue) => updateField('sportCategory', fieldValue)}
              onCategoryChange={(fieldValue) => updateField('category', fieldValue)}
              onTeamCategoryChange={(fieldValue) => updateField('teamCategory', fieldValue)}
            />
          </section>

          <section className="news-editor__card news-editor__card--details" aria-labelledby="news-details-heading">
            <h2 id="news-details-heading" className="news-editor__section-title news-editor__section-title--small">
              {t('admin.news.section_details')}
            </h2>
            <NewsDetailsFields
              tags={newsData.tags}
              author={newsData.author}
              errors={errors}
              onTagsChange={(tags) => updateField('tags', tags)}
              onAuthorChange={(fieldValue) => updateField('author', fieldValue)}
            />
          </section>
        </div>

        {preview && (
          <section className="news-editor__preview" aria-live="polite">
            <p className="news-editor__preview-note">{t('admin.news.preview_description')}</p>
            <PreviewNews value={value} newsData={newsData} />
          </section>
        )}

        <ConfirmationDialog
          isOpen={confirmOpen}
          icon="📰"
          title={isEditMode ? t('admin.news.confirmUpdateTitle') : t('admin.news.confirmPublishTitle')}
          message={isEditMode ? t('admin.news.confirm_update') : t('admin.news.confirm_publish')}
          confirmText={isEditMode ? t('admin.news.update') : t('admin.news.publish')}
          cancelText={t('common.cancel')}
          isLoading={isSaving}
          onConfirm={() => {
            void handleConfirmPublish();
          }}
          onCancel={() => setConfirmOpen(false)}
        />
      </div>
    </PageTemplate>
  );
}
