import { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import RichTextEditor, { extractRichTextImageUrls } from '../../../components/RichTextEditor';
import Button from '../../../components/Button/Button';
import ConfirmationDialog from '../../../components/ConfirmationDialog/ConfirmationDialog';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import LoadingSpinner from '../../../components/LoadingSpinner/LoadingSpinner';
import NewsInputs, { type NewsInputsData } from './components/NewsInputs';
import PreviewNews from './components/PreviewNews';
import { CreateNewsService } from '../../../api/admin/News/CreateNewsService';
import { UpdateNewsService } from '../../../api/admin/News/UpdateNewsService';
import { handleImageDeleteService } from '../../../api/admin/News/handleImageDeleteService';
import { singleNewsService } from '../../../api/news/singleNewsService';
import { useAdminReturnTo } from '../../../hooks/useAdminReturnTo';
import { unwrapApiErrorMessage } from '../../../api/utils/ParseErrorResponse';
import './NewsCreateEditPage.scss';

const emptyNewsData = (): NewsInputsData => ({
  title: '',
  mainPicture: '',
  summary: '',
  author: '',
  category: '',
  sportCategory: '',
  teamCategory: '',
  tags: [],
  contentHtml: '',
});

const collectStoredImageUrls = (html: string, mainPicture: string): string[] => {
  const urls = extractRichTextImageUrls(html);
  if (mainPicture.trim()) {
    urls.push(mainPicture.trim());
  }
  return Array.from(new Set(urls));
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
  const [newsData, setNewsData] = useState<NewsInputsData>(emptyNewsData);
  const [errors, setErrors] = useState<Partial<Record<keyof NewsInputsData, string>>>({});
  const [contentError, setContentError] = useState('');
  const originalImageUrlsRef = useRef<string[]>([]);
  const leaveTimeoutRef = useRef<number | null>(null);

  const deleteOrphanedImages = (keptHtml: string, keptMainPicture: string): void => {
    const kept = new Set(collectStoredImageUrls(keptHtml, keptMainPicture));
    originalImageUrlsRef.current
      .filter((url) => !kept.has(url))
      .forEach((url) => {
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
          contentHtml: article.contentHtml || '',
        });
        setValue(article.contentHtml || '');
        originalImageUrlsRef.current = collectStoredImageUrls(
          article.contentHtml || '',
          article.mainImage || '',
        );
      } catch (loadError: unknown) {
        console.error('Failed to fetch news article:', loadError);
        setError(t('admin.news.errors.loadFailed', 'Failed to load news article for editing'));
      } finally {
        setIsLoadingArticle(false);
      }
    };

    void fetchNewsArticle();
  }, [id, isEditMode, t]);

  const validateInputs = (): boolean => {
    const newErrors: Partial<Record<keyof NewsInputsData, string>> = {};
    let newContentError = '';

    if (!newsData.title.trim()) {
      newErrors.title = t('admin.news.errors.title_required');
    } else if (newsData.title.trim().length < 5) {
      newErrors.title = t('admin.news.errors.title_too_short');
    } else if (newsData.title.trim().length > 200) {
      newErrors.title = t('admin.news.errors.title_too_long');
    }

    if (!value.trim()) {
      newContentError = t('admin.news.errors.content_required');
    }

    if (newsData.author && newsData.author.trim().length > 50) {
      newErrors.author = t('admin.news.errors.author_too_long');
    }

    if (newsData.summary && newsData.summary.trim().length > 200) {
      newErrors.summary = t('admin.news.errors.summary_too_long');
    }

    setErrors(newErrors);
    setContentError(newContentError);

    return Object.keys(newErrors).length === 0 && !newContentError;
  };

  const scrollToFirstError = (): void => {
    const firstErrorElement = document.querySelector('.news-inputs__error, .news-editor__content-error');
    if (firstErrorElement) {
      firstErrorElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
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

  const toNullIfEmpty = (input: string | undefined | null): string | null => {
    if (!input || input.trim() === '') {
      return null;
    }
    return input.trim();
  };

  const convertToNewsData = () => ({
    title: newsData.title.trim(),
    mainImage: newsData.mainPicture || null,
    contentHtml: value.trim(),
    summary: newsData.summary?.trim() || null,
    author: newsData.author?.trim() || null,
    category: newsData.category || null,
    sportCategory: newsData.sportCategory || null,
    teamCategory: newsData.teamCategory || null,
    tags: newsData.tags.filter((tag) => tag.trim() !== ''),
  });

  const leaveAfterSuccess = (message: string): void => {
    setSuccessMessage(message);
    leaveTimeoutRef.current = window.setTimeout(() => {
      navigate(returnTo);
    }, 900);
  };

  const handleConfirmPublish = async (): Promise<void> => {
    try {
      setIsSaving(true);
      setError(null);

      if (isEditMode && id) {
        const trimmedMainPicture = toNullIfEmpty(newsData.mainPicture);
        await UpdateNewsService(id, {
          title: newsData.title.trim(),
          contentHtml: value.trim(),
          mainImage: trimmedMainPicture,
          summary: toNullIfEmpty(newsData.summary),
          imageUrls: trimmedMainPicture ? [trimmedMainPicture] : null,
          author: toNullIfEmpty(newsData.author),
          category: toNullIfEmpty(newsData.category),
          sportCategory: toNullIfEmpty(newsData.sportCategory),
          teamCategory: toNullIfEmpty(newsData.teamCategory),
          tags: newsData.tags.filter((tag) => tag.trim() !== ''),
        });
        deleteOrphanedImages(value.trim(), trimmedMainPicture ?? '');
        setConfirmOpen(false);
        leaveAfterSuccess(t('admin.news.success.update_success'));
        return;
      }

      await CreateNewsService(convertToNewsData());
      setConfirmOpen(false);
      leaveAfterSuccess(t('admin.news.success.publish_success'));
    } catch (saveError: unknown) {
      const fallback = isEditMode
        ? t('admin.news.errors.update_error')
        : t('admin.news.errors.publish_error');
      setError(unwrapApiErrorMessage(saveError, fallback));
    } finally {
      setIsSaving(false);
    }
  };

  const pageTitle = isEditMode ? t('admin.news.edit') : t('admin.news.create');
  const busy = isSaving || isUploadingContent;

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

        <header className="news-editor__toolbar">
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

        <div hidden={preview} className="news-editor__form">
          <section className="news-editor__card">
            <h2 className="news-editor__section-title">{t('admin.news.article_details')}</h2>
            <NewsInputs data={newsData} onChange={setNewsData} errors={errors} />
          </section>

          <section className="news-editor__card">
            <div className="news-editor__section-heading">
              <h2 className="news-editor__section-title">
                {t('admin.news.content')}
                <span className="news-editor__required" aria-hidden="true">*</span>
              </h2>
              {isUploadingContent && (
                <LoadingSpinner size="sm" text={t('admin.news.uploading')} />
              )}
            </div>
            {contentError && (
              <p className="news-editor__content-error">{contentError}</p>
            )}
            <div className={`news-editor__richtext ${contentError ? 'news-editor__richtext--error' : ''}`}>
              <RichTextEditor
                value={value}
                onChange={setValue}
                onUploadingChange={setIsUploadingContent}
                showMatchInsert
              />
            </div>
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
