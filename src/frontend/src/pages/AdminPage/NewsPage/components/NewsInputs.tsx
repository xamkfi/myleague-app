import { useState } from 'react';
import { useTranslation } from 'react-i18next';

interface NewsInputsData {
  title: string;
  mainPicture: string;
  author: string;
  tags: string[];
  category: string;
  sportCategory: string;
}

interface NewsInputsProps {
  data: NewsInputsData;
  onChange: (data: NewsInputsData) => void;
  errors?: Partial<NewsInputsData>;
}

const CATEGORIES = [
  'Transfer',
  'GameResult', 
  'Announcement',
  'Interview',
  'Analysis',
  'Breaking News',
  'Match Preview',
  'Player Profile'
];

const SPORT_CATEGORIES = [
  'Football',
  'Basketball', 
  'Tennis',
  'Baseball',
  'Hockey',
  'Swimming',
  'Athletics',
  'Volleyball',
  'Other'
];

export default function NewsInputs({ data, onChange, errors = {} }: NewsInputsProps) {
  const { t } = useTranslation();
  const [newTag, setNewTag] = useState('');

  const updateField = (field: keyof NewsInputsData, value: any) => {
    onChange({ ...data, [field]: value });
  };

  const addTag = () => {
    if (newTag.trim() && !data.tags.includes(newTag.trim())) {
      updateField('tags', [...data.tags, newTag.trim()]);
      setNewTag('');
    }
  };

  const removeTag = (tagToRemove: string) => {
    updateField('tags', data.tags.filter(tag => tag !== tagToRemove));
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      addTag();
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
      <h2 className="text-xl font-semibold text-gray-900 mb-6 flex items-center gap-2">
        <svg className="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
        </svg>
        {t('admin.news.article_details', 'Article Details')}
      </h2>

      <div className="space-y-6">
        {/* Title */}
        <div>
          <label htmlFor="title" className="block text-sm font-medium text-gray-700 mb-2">
            {t('admin.news.title', 'Title')} <span className="text-red-500">*</span>
          </label>
          <input
            type="text"
            id="title"
            value={data.title}
            onChange={(e) => updateField('title', e.target.value)}
            className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors ${
              errors.title ? 'border-red-300 bg-red-50' : 'border-gray-300'
            }`}
            placeholder={t('admin.news.title_placeholder', 'Enter compelling article title...')}
          />
          {errors.title && (
            <p className="text-red-600 text-sm mt-1 flex items-center gap-1">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {errors.title}
            </p>
          )}
        </div>

        {/* Main Picture */}
        <div>
          <label htmlFor="mainPicture" className="block text-sm font-medium text-gray-700 mb-2">
            {t('admin.news.main_picture', 'Main Picture URL')}
          </label>
          <div className="space-y-3">
            <input
              type="url"
              id="mainPicture"
              value={data.mainPicture}
              onChange={(e) => updateField('mainPicture', e.target.value)}
              className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors ${
                errors.mainPicture ? 'border-red-300 bg-red-50' : 'border-gray-300'
              }`}
              placeholder={t('admin.news.picture_placeholder', 'https://example.com/image.jpg')}
            />
            {data.mainPicture && (
              <div className="relative">
                <img
                  src={data.mainPicture}
                  alt="Main picture preview"
                  className="w-full h-48 object-cover rounded-lg border border-gray-200"
                  onError={(e) => {
                    const target = e.target as HTMLImageElement;
                    target.src = 'https://via.placeholder.com/400x200?text=Invalid+Image+URL';
                  }}
                />
                <div className="absolute top-2 right-2">
                  <button
                    type="button"
                    onClick={() => updateField('mainPicture', '')}
                    className="bg-red-500 text-white p-1 rounded-full hover:bg-red-600 transition-colors"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              </div>
            )}
          </div>
          {errors.mainPicture && (
            <p className="text-red-600 text-sm mt-1 flex items-center gap-1">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {errors.mainPicture}
            </p>
          )}
        </div>

        {/* Author */}
        <div>
          <label htmlFor="author" className="block text-sm font-medium text-gray-700 mb-2">
            {t('admin.news.author', 'Author')} <span className="text-red-500">*</span>
          </label>
          <input
            type="text"
            id="author"
            value={data.author}
            onChange={(e) => updateField('author', e.target.value)}
            className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors ${
              errors.author ? 'border-red-300 bg-red-50' : 'border-gray-300'
            }`}
            placeholder={t('admin.news.author_placeholder', 'Enter author name...')}
          />
          {errors.author && (
            <p className="text-red-600 text-sm mt-1 flex items-center gap-1">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {errors.author}
            </p>
          )}
        </div>

        {/* Categories */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label htmlFor="category" className="block text-sm font-medium text-gray-700 mb-2">
              {t('admin.news.category', 'Category')} <span className="text-red-500">*</span>
            </label>
            <select
              id="category"
              value={data.category}
              onChange={(e) => updateField('category', e.target.value)}
              className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors ${
                errors.category ? 'border-red-300 bg-red-50' : 'border-gray-300'
              }`}
            >
              <option value="">{t('admin.news.select_category', 'Select category')}</option>
              {CATEGORIES.map(cat => (
                <option key={cat} value={cat}>{cat}</option>
              ))}
            </select>
            {errors.category && (
              <p className="text-red-600 text-sm mt-1 flex items-center gap-1">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                {errors.category}
              </p>
            )}
          </div>

          <div>
            <label htmlFor="sportCategory" className="block text-sm font-medium text-gray-700 mb-2">
              {t('admin.news.sport_category', 'Sport Category')} <span className="text-red-500">*</span>
            </label>
            <select
              id="sportCategory"
              value={data.sportCategory}
              onChange={(e) => updateField('sportCategory', e.target.value)}
              className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors ${
                errors.sportCategory ? 'border-red-300 bg-red-50' : 'border-gray-300'
              }`}
            >
              <option value="">{t('admin.news.select_sport', 'Select sport')}</option>
              {SPORT_CATEGORIES.map(sport => (
                <option key={sport} value={sport}>{sport}</option>
              ))}
            </select>
            {errors.sportCategory && (
              <p className="text-red-600 text-sm mt-1 flex items-center gap-1">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                {errors.sportCategory}
              </p>
            )}
          </div>
        </div>

        {/* Tags */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            {t('admin.news.tags', 'Tags')}
          </label>
          <div className="space-y-3">
            <div className="flex gap-2">
              <input
                type="text"
                value={newTag}
                onChange={(e) => setNewTag(e.target.value)}
                onKeyPress={handleKeyPress}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors"
                placeholder={t('admin.news.add_tag_placeholder', 'Type tag and press Enter...')}
              />
              <button
                type="button"
                onClick={addTag}
                disabled={!newTag.trim()}
                className="bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                {t('common.add', 'Add')}
              </button>
            </div>
            
            {data.tags.length > 0 && (
              <div className="flex flex-wrap gap-2">
                {data.tags.map((tag, index) => (
                  <span
                    key={index}
                    className="bg-blue-100 text-blue-800 px-3 py-1 rounded-full text-sm flex items-center gap-2 group hover:bg-blue-200 transition-colors"
                  >
                    #{tag}
                    <button
                      type="button"
                      onClick={() => removeTag(tag)}
                      className="text-blue-600 hover:text-blue-800 opacity-0 group-hover:opacity-100 transition-opacity"
                    >
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </span>
                ))}
              </div>
            )}
            
            {data.tags.length === 0 && (
              <p className="text-gray-500 text-sm italic">
                {t('admin.news.no_tags', 'No tags added yet. Tags help categorize your article.')}
              </p>
            )}
          </div>
          {errors.tags && (
            <p className="text-red-600 text-sm mt-1 flex items-center gap-1">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {errors.tags}
            </p>
          )}
        </div>
      </div>
    </div>
  );
}

export { type NewsInputsData };