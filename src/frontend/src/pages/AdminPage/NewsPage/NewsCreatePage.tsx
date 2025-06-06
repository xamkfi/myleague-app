import { useState } from "react";
import PageTemplate from "../../../components/PageTemplate/PageTemplate";
import QuillEditor from "./components/QuillEditor";
import { useTranslation } from "react-i18next";
import NewsInputs, { type NewsInputsData } from "./components/NewsInputs";
import SingleNewsPage from "../../SingleNewsPage/SingleNewsPage";
import PreviewNews from "./components/PreviewNews";

export default function NewsCreatePage() {
  const { t } = useTranslation();
  const [value, setValue] = useState("");
  const [preview, setPreview] = useState(false);
  
  const [newsData, setNewsData] = useState<NewsInputsData>({
    title: '',
    mainPicture: '',
    author: '',
    tags: [],
    category: '',
    sportCategory: '',
    summary: ''
  });

  const [errors, setErrors] = useState<Partial<NewsInputsData>>({});

  const validateInputs = (): boolean => {
    const newErrors: Partial<NewsInputsData> = {};

    if (!newsData.title.trim()) {
      newErrors.title = t('admin.news.error.title_required', 'Title is required');
    }
    if (!newsData.author.trim()) {
      newErrors.author = t('admin.news.error.author_required', 'Author is required');
    }
    if (!newsData.category) {
      newErrors.category = t('admin.news.error.category_required', 'Category is required');
    }
    if (!newsData.sportCategory) {
      newErrors.sportCategory = t('admin.news.error.sport_category_required', 'Sport category is required');
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = () => {
    if (validateInputs()) {
      // Combine all data
      const fullArticleData = {
        ...newsData,
        contentHtml: value,
        createdAt: new Date().toISOString(),
        id: crypto.randomUUID()
      };
      
      console.log('Saving article:', fullArticleData);
      // Here you would save to your backend
    }
  };

  const handlePublish = () => {
    if (validateInputs() && value.trim()) {
      // Publish the article
      const fullArticleData = {
        ...newsData,
        contentHtml: value,
        isPublished: true
      };
      
      console.log('Publishing article:', fullArticleData);
      // Here you would publish to your backend
    }
  };

  // Convert NewsInputsData to the format expected by SingleNewsPage
  const convertToNewsData = () => {
    return {
      id: 'preview',
      title: newsData.title || 'Untitled Article',
      contentHtml: value,
      summary: newsData.summary,
      imageUrls: newsData.mainPicture ? [newsData.mainPicture] : [],
      author: newsData.author,
      createdAt: new Date().toISOString(),
      updatedAt: null,
      category: newsData.category,
      sportCategory: newsData.sportCategory,
      tags: newsData.tags,
      isArchived: false
    };
  };

  // If in preview mode, show SingleNewsPage directly
  if (preview) {
    return (
      <PageTemplate title={t('admin.news.create', 'Create News Article')}>
      <div className="min-h-screen">
        {/* Preview Header */}
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

        {/* SingleNewsPage Preview */}
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
        {/* Header */}
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
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                {t('admin.news.publish', 'Publish')}
              </button>
              <button
                onClick={handleSave}
                className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
              >
                {t('admin.news.save_draft', 'Save Draft')}
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

        {/* Article Inputs */}
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

        {/* Content Section */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200">
          <div className="p-6">
            <div className="flex justify-between items-center mb-6">
              <h2 className="text-xl font-semibold text-gray-900 flex items-center gap-2">
                <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                {t('admin.news.content', 'Article Content')}
              </h2>
            </div>

            <div className="border border-gray-200 rounded-lg">
              <QuillEditor value={value} setValue={setValue} />
            </div>
          </div>
        </div>
      </div>
    </PageTemplate>
  );
}