import { useState, useEffect } from "react";
import PageTemplate from "../../../components/PageTemplate/PageTemplate";
import QuillEditor from "./components/QuillEditor";
import { useTranslation } from "react-i18next";
import NewsInputs, { type NewsInputsData } from "./components/NewsInputs";
import PreviewNews from "./components/PreviewNews";
import { LoadingSpinner } from "./components/LoadingSpinner";
import { CreateNewsService } from "../../../api/admin/News/CreateNewsService";
import { UpdateNewsService } from "../../../api/admin/News/UpdateNewsService";
import { useNavigate, useParams } from "react-router-dom";
import { singleNewsService } from "../../../api/news/singleNewsService";
import "./NewsCreateEditPage.scss";

declare global {
  interface Window {
    setQuillNavigatingState?: (isNavigating: boolean) => void;
  }
}

export default function NewsCreateEditPage() {
  const { t } = useTranslation();
  const { id } = useParams();
  const isEditMode = !!id;
  const [value, setValue] = useState("");
  const [preview, setPreview] = useState(false);
  const [loadingAnimation, setLoadingAnimation] = useState(false);
  const [isClearingEditor, setIsClearingEditor] = useState(false);
  const [isLoadingArticle, setIsLoadingArticle] = useState(isEditMode);

  const [newsData, setNewsData] = useState<NewsInputsData>({
    title: '',
    mainPicture: '',
    summary: '',
    author: '',
    category: '',
    sportCategory: '',
    tags: []
  });

  const [errors, setErrors] = useState<Partial<NewsInputsData>>({});
  const [contentError, setContentError] = useState<string>('');

  const navigate = useNavigate();

  // Load existing article data if in edit mode
  useEffect(() => {
    if (isEditMode && id) {
      const fetchNewsArticle = async () => {
        try {
          setIsLoadingArticle(true);
          const article = await singleNewsService(id);
          setNewsData({
            title: article.title || '',
            mainPicture: article.mainImage || '',
            summary: article.summary || '',
            author: article.author || '',
            category: article.category || '',
            sportCategory: article.sportCategory || '',
            tags: article.tags || []
          });
          setValue(article.contentHtml || '');
        } catch (error) {
          console.error('Failed to fetch news article:', error);
          alert('Failed to load news article for editing');
          navigate('/admin/news');
        } finally {
          setIsLoadingArticle(false);
        }
      };
      fetchNewsArticle();
    }
  }, [id, isEditMode, navigate]);

  const validateInputs = (): boolean => {
    const newErrors: Partial<NewsInputsData> = {};
    let newContentError = '';

    if (!newsData.title.trim()) {
      newErrors.title = t('admin.news.error.title_required', 'Title is required');
    } else if (newsData.title.trim().length < 5) {
      newErrors.title = t('admin.news.error.title_too_short', 'Title must be at least 5 characters long');
    } else if (newsData.title.trim().length > 200) {
      newErrors.title = t('admin.news.error.title_too_long', 'Title cannot exceed 200 characters');
    }

    if (!value.trim()) {
      newContentError = t('admin.news.error.content_required', 'Article content is required');
    }

    if (newsData.author && newsData.author.trim().length > 50) {
      newErrors.author = t('admin.news.error.author_too_long', 'Author name cannot exceed 50 characters');
    }

    if (newsData.summary && newsData.summary.trim().length > 200) {
      newErrors.summary = t('admin.news.error.summary_too_long', 'Summary cannot exceed 200 characters');
    }

    setErrors(newErrors);
    setContentError(newContentError);
    
    return Object.keys(newErrors).length === 0 && !newContentError;
  };

  const handlePublish = async () => {
    if (validateInputs()) {
      const confirmMessage = isEditMode 
        ? t('admin.news.confirm_update', 'Are you sure you want to update this news article?')
        : t('admin.news.confirm_publish', 'Are you sure you want to publish this news article?');
      
      const confirmAction = window.confirm(confirmMessage);
      
      if (!confirmAction) {
        return;
      }

      try {
        setLoadingAnimation(true);
        
        const newsToSubmit = convertToNewsData();
        
        if (isEditMode) {
          const updateData = {
            title: newsData.title.trim(),
            contentHtml: value.trim(),
            mainImage: newsData.mainPicture || null,
            summary: newsData.summary?.trim() || null,
            imageUrls: [newsData.mainPicture || null],
            author: newsData.author?.trim() || null,
            category: newsData.category || null,
            sportCategory: newsData.sportCategory || null,
            tags: newsData.tags.filter(tag => tag.trim() !== '').map(tag => tag || null)
          };
          await UpdateNewsService(id, updateData);
          console.log("News updated successfully:", newsToSubmit);
          alert(t('admin.news.update_success', 'News article updated successfully!'));
        } else {
          const response = await CreateNewsService(newsToSubmit);
          console.log("News created successfully:", response);
          alert(t('admin.news.publish_success', 'News article published successfully!'));
          removeInputFields();
        }
        
        console.log("=== Publish Completed ===");
        
        // Aseta navigaatio tila ENNEN navigointia
        if (typeof window !== 'undefined' && window.setQuillNavigatingState) {
          window.setQuillNavigatingState(true);
          console.log("🚀 Setting navigation state to true before navigate");
        }
        
        // Navigoi
        navigate('/admin/news');
        
      } catch (err) {
        console.error("Failed to save news:", err);
        const errorMessage = isEditMode 
          ? t('admin.news.update_error', 'Failed to update news article. Please try again.')
          : t('admin.news.publish_error', 'Failed to publish news article. Please try again.');
        alert(errorMessage);
      } finally {
        setLoadingAnimation(false);
      }
    } else {
      const firstErrorElement = document.querySelector('.border-red-300, .text-red-600');
      if (firstErrorElement) {
        firstErrorElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
    }
  };

  const convertToNewsData = () => {
    return {
      title: newsData.title.trim(),
      mainImage: newsData.mainPicture || null,
      contentHtml: value.trim(),
      summary: newsData.summary?.trim() || null,
      author: newsData.author?.trim() || null,
      category: newsData.category || null,
      sportCategory: newsData.sportCategory || null,
      tags: newsData.tags.filter(tag => tag.trim() !== ''),
    };
  };

  const removeInputFields = () => {
    setIsClearingEditor(true);
    setValue("");
    
    setNewsData({
      title: '',
      mainPicture: '',
      summary: '',
      author: '',
      category: '',
      sportCategory: '',
      tags: []
    });
    setErrors({});
    setContentError('');
    
    // Reset the flag after a short delay
    setTimeout(() => {
      setIsClearingEditor(false);
    }, 100);
  };

  if (isLoadingArticle) {
    return (
      <PageTemplate title={t('admin.news.loading', 'Loading...')}>
        <div className="flex justify-center items-center min-h-screen">
          <LoadingSpinner />
          <span className="ml-2">{t('admin.news.loading_article', 'Loading article...')}</span>
        </div>
      </PageTemplate>
    );
  }

  if (preview) {
    return (
      <PageTemplate title={isEditMode ? t('admin.news.edit', 'Edit News Article') : t('admin.news.create', 'Create News Article')}>
      <div className="min-h-screen">
        <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
          <div className="max-w-6xl mx-auto px-4 py-3">
            <div className="flex justify-between items-center">
              <div className="flex items-center gap-3">
                <span className="bg-blue-100 text-blue-800 text-sm font-medium px-3 py-1 rounded-full">
                  {t('admin.news.preview_mode', 'Preview Mode')}
                </span>
                <span className="text-gray-600 text-sm">
                  {t('admin.news.preview_description', 'This is how your article will appear to readers')}
                </span>
              </div>
              
              <button 
                onClick={() => setPreview(false)}
                className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors flex items-center gap-2"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
                {t('admin.news.edit_content', 'Edit Content')}
              </button>
            </div>
          </div>
        </div>

        <div className="py-8">
          <PreviewNews 
            value={value} 
            newsData={newsData}
          />
        </div>
      </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={isEditMode ? t('admin.news.edit', 'Edit News Article') : t('admin.news.create', 'Create News Article')}>
      
      <div className="max-w-6xl mx-auto space-y-8">
        <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
          <div className="max-w-6xl mx-auto px-4 py-3">
            <div className="flex justify-between items-center">
              <div className="flex items-center gap-3">
                <span className="bg-blue-100 text-blue-800 text-sm font-medium px-3 py-1 rounded-full">
                  {isEditMode ? t('admin.news.edit_mode', 'Edit Mode') : t('admin.news.create_mode', 'Create Mode')}
                </span>
                <span className="text-gray-600 text-sm">
                  {isEditMode ? t('admin.news.edit_description', 'Edit existing news article') : t('admin.news.create_description', 'Create a new news article')}
                </span>
              </div>
              
              <div className="flex gap-3">
                <button
                  onClick={handlePublish}
                  disabled={loadingAnimation}
                  className={`px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2 ${
                    loadingAnimation ? 'opacity-50 cursor-not-allowed' : ''
                  }`}
                >
                  {loadingAnimation && (
                    <svg className="animate-spin w-4 h-4" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="m4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                  )}
                  {isEditMode ? t('admin.news.update', 'Update Article') : t('admin.news.publish', 'Publish')}
                </button>
                <button 
                  onClick={() => setPreview(true)}
                  className="px-4 py-2 bg-green-200 text-green-700 hover:bg-green-200 rounded-lg font-medium transition-colors flex items-center gap-2"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                  </svg>
                  {t('admin.news.preview', 'Preview')}
                </button>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-gray-200">
          <div className="p-6">
            <h2 className="text-xl font-semibold text-gray-900 flex items-center gap-2 mb-6">
              <svg className="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              {t('admin.news.article_details', 'Article Details')}
            </h2>
            
            <NewsInputs 
              data={newsData}
              onChange={setNewsData}
              errors={errors}
            />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-gray-200">
          <div className="p-6">
            <div className="flex justify-between items-center mb-6">
              <div>
                <h2 className="text-xl font-semibold text-gray-900 flex items-center gap-2">
                  <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                  {t('admin.news.content', 'Article Content')}
                  <span className="text-red-500">*</span>
                </h2>
                
                {contentError && (
                  <p className="text-red-600 text-sm mt-2 flex items-center gap-1">
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    {contentError}
                  </p>
                )}
              </div>

              {loadingAnimation && (
                <div className="flex items-center gap-2">
                  <span className="text-sm text-blue-600">Uploading image...</span>
                  <LoadingSpinner/>
                </div>
              )}
            </div>
            
            <div className={`border rounded-lg ${contentError ? 'border-red-300' : 'border-gray-200'}`}>
              <QuillEditor 
                value={value} 
                setValue={setValue} 
                setLoading={setLoadingAnimation}
                isClearing={isClearingEditor}
              />
            </div>
            
          </div>
        </div>
      </div>
    </PageTemplate>
  );
}