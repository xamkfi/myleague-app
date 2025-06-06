import { useState } from 'react';
import { useTranslation } from 'react-i18next';

interface NewsInputsData {
  title: string;
  mainPicture: string;
  author: string;
  tags: string[];
  category: string;
  sportCategory: string;
  summary: string;
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
  const [uploadingImage, setUploadingImage] = useState(false);

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

  const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validate file type
      if (!file.type.startsWith('image/')) {
        alert(t('admin.news.error.invalid_image', 'Please select a valid image file'));
        return;
      }

      // Validate file size (e.g., max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        alert(t('admin.news.error.image_too_large', 'Image file must be less than 5MB'));
        return;
      }

      setUploadingImage(true);
      
      const reader = new FileReader();
      reader.onload = (e) => {
        const imageDataUrl = e.target?.result as string;
        updateField('mainPicture', imageDataUrl);
        setUploadingImage(false);
      };
      reader.onerror = () => {
        alert(t('admin.news.error.upload_failed', 'Failed to upload image'));
        setUploadingImage(false);
      };
      reader.readAsDataURL(file);
    }
  };

  const downloadImage = () => {
    if (data.mainPicture) {
      const link = document.createElement('a');
      link.href = data.mainPicture;
      link.download = `news-image-${Date.now()}.jpg`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  };

  const removeImage = () => {
    updateField('mainPicture', '');
  };

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">

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

        {/* Summary */}
        <div>
          <label htmlFor="summary" className="block text-sm font-medium text-gray-700 mb-2">
            {t('admin.news.summary', 'Summary')} <span className="text-red-500">*</span>
          </label>
          <textarea
            id="summary"
            value={data.summary}
            onChange={(e) => updateField('summary', e.target.value)}
            rows={3}
            className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors resize-none ${
              errors.summary ? 'border-red-300 bg-red-50' : 'border-gray-300'
            }`}
            placeholder={t('admin.news.summary_placeholder', 'Write a brief summary that captures the essence of your article...')}
          />
          <div className="flex justify-between items-center mt-1">
            <div className="text-sm text-gray-500">
              {data.summary.length}/200 {t('admin.news.characters', 'characters')}
            </div>
            {data.summary.length > 200 && (
              <div className="text-red-500 text-sm">
                {t('admin.news.summary_too_long', 'Summary should be under 200 characters')}
              </div>
            )}
          </div>
          {errors.summary && (
            <p className="text-red-600 text-sm mt-1 flex items-center gap-1">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {errors.summary}
            </p>
          )}
        </div>

        {/* Main Picture */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            {t('admin.news.main_picture', 'Main Picture')}
          </label>
          
          {!data.mainPicture ? (
            <div className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center hover:border-gray-400 transition-colors">
              <input
                type="file"
                accept="image/*"
                onChange={handleImageUpload}
                className="hidden"
                id="imageUpload"
                disabled={uploadingImage}
              />
              <label
                htmlFor="imageUpload"
                className={`cursor-pointer ${uploadingImage ? 'opacity-50 cursor-not-allowed' : ''}`}
              >
                <div className="flex flex-col items-center">
                  <svg className="w-12 h-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                  </svg>
                  <div className="text-sm text-gray-600">
                    {uploadingImage ? (
                      <span className="flex items-center gap-2">
                        <svg className="animate-spin w-4 h-4" fill="none" viewBox="0 0 24 24">
                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                          <path className="opacity-75" fill="currentColor" d="m4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                        {t('admin.news.uploading', 'Uploading...')}
                      </span>
                    ) : (
                      <>
                        <span className="font-medium text-blue-600">
                          {t('admin.news.click_to_upload', 'Click to upload')}
                        </span>
                        <span className="text-gray-500"> {t('admin.news.or_drag_drop', 'or drag and drop')}</span>
                      </>
                    )}
                  </div>
                  <div className="text-xs text-gray-500 mt-1">
                    PNG, JPG, GIF {t('admin.news.up_to', 'up to')} 5MB
                  </div>
                </div>
              </label>
            </div>
          ) : (
            <div className="space-y-3">
              <div className="relative group">
                <img
                  src={data.mainPicture}
                  alt="Main picture preview"
                  className="w-full h-64 object-cover rounded-lg border border-gray-200"
                  onError={(e) => {
                    const target = e.target as HTMLImageElement;
                    target.src = 'https://via.placeholder.com/400x200?text=Invalid+Image';
                  }}
                />
                <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-30 transition-all duration-200 rounded-lg flex items-center justify-center opacity-0 group-hover:opacity-100">
                  <div className="flex gap-2">
                    <button
                      type="button"
                      onClick={downloadImage}
                      className="bg-white text-gray-700 p-2 rounded-full hover:bg-gray-100 transition-colors flex items-center gap-2"
                      title={t('admin.news.download_image', 'Download image')}
                    >
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                      </svg>
                    </button>
                    <button
                      type="button"
                      onClick={removeImage}
                      className="bg-red-500 text-white p-2 rounded-full hover:bg-red-600 transition-colors"
                      title={t('admin.news.remove_image', 'Remove image')}
                    >
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </button>
                  </div>
                </div>
              </div>
              
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-600">
                  {t('admin.news.image_uploaded', 'Image uploaded successfully')}
                </span>
                <div className="flex gap-2">
                  <button
                    type="button"
                    onClick={downloadImage}
                    className="text-sm text-blue-600 hover:text-blue-800 flex items-center gap-1"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                    {t('admin.news.download', 'Download')}
                  </button>
                  <input
                    type="file"
                    accept="image/*"
                    onChange={handleImageUpload}
                    className="hidden"
                    id="imageReplace"
                  />
                  <label
                    htmlFor="imageReplace"
                    className="text-sm text-green-600 hover:text-green-800 cursor-pointer flex items-center gap-1"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
                    </svg>
                    {t('admin.news.replace', 'Replace')}
                  </label>
                </div>
              </div>
            </div>
          )}
          
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