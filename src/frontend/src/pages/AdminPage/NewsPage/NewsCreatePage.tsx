import { useState } from "react";
import PageTemplate from "../../../components/PageTemplate/PageTemplate";
import QuillEditor from "./components/QuillEditor";
import { useTranslation } from "react-i18next";
import NewsInputs, { type NewsInputsData } from "./components/NewsInputs";
import PreviewNews from "./components/PreviewNews";
import { LoadingSpinner } from "./components/LoadingSpinner";
import { CreateNewsService } from "../../../api/admin/News/CreateNewsService";
import { useNavigate } from "react-router-dom";

export default function NewsCreatePage() {
  const { t } = useTranslation();
  const [value, setValue] = useState("");
  const [preview, setPreview] = useState(false);
  const [loadingAnimation, setLoadingAnimation] = useState(false);

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
      const confirmPublish = window.confirm(
        t('admin.news.confirm_publish', 'Are you sure you want to publish this news article?')
      );
      
      if (!confirmPublish) {
        return;
      }

      try {
        setLoadingAnimation(true);
        const newsToSubmit = convertToNewsData();
        const response = await CreateNewsService(newsToSubmit);
        console.log("News created successfully:", response);
        
        alert(t('admin.news.publish_success', 'News article published successfully!'));
        removeInputFields();
        navigate('/admin');
        
      } catch (err) {
        console.error("Failed to create news:", err);
        alert(t('admin.news.publish_error', 'Failed to publish news article. Please try again.'));
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
  };

  if (preview) {
    return (
      <PageTemplate title={t('admin.news.create', 'Create News Article')}>
      <div className="min-h-screen">
        <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
          <div className="max-w-6xl mx-auto px-4 py-3">
            <div className="flex justify-between items-center">
              <div className="flex items-center gap-3">
                <span className="bg-blue-100 text-blue-800 text-sm font-medium px-3 py-1 rounded-full">
                  Preview Mode
                </span>
                <span className="text-gray-600 text-sm">
                  This is how your article will appear to readers
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
    <PageTemplate title={t('admin.news.create', 'Create News Article')}>
      
      <div className="max-w-6xl mx-auto space-y-8">
        <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
          <div className="max-w-6xl mx-auto px-4 py-3">
            <div className="flex justify-between items-center">
              <div className="flex items-center gap-3">
                <span className="bg-blue-100 text-blue-800 text-sm font-medium px-3 py-1 rounded-full">
                  Edit Mode
                </span>
                <span className="text-gray-600 text-sm">
                  Create a new news article
                </span>
              </div>
              
            <div className="flex gap-3 ">
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
                {t('admin.news.publish', 'Publish')}
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
              <QuillEditor value={value} setValue={setValue} setLoading={setLoadingAnimation}/>
            </div>
            
          </div>
        </div>
      </div>
    </PageTemplate>
  );
}