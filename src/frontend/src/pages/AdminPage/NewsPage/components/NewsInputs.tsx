import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { handleImageUploadService } from '../../../../api/admin/News/handleImageUploadService';
import '../styles/NewsInputs.scss';

export interface NewsInputsData {
  title: string;
  mainPicture: string;
  author: string;
  tags: string[];
  category: string;
  sportCategory: string;
  summary: string;
  contentHtml: string;
}

interface NewsInputsProps {
  data: NewsInputsData;
  onChange: (data: NewsInputsData) => void;
  errors?: Partial<NewsInputsData>;
}

const CATEGORIES = [
  'General',
  'MatchReports',
  'LeagueNews',
  'PlayerUpdates',
  'TeamNews',
  'Announcements',
  'Events',
  'Transfers',
  'Injuries',
  'Awards',
];

const SPORT_CATEGORIES = [
  'Floorball', 
  'Icehockey',
  'Football'
];

export default function NewsInputs({ data, onChange, errors = {} }: NewsInputsProps) {
  const { t } = useTranslation();
  const [newTag, setNewTag] = useState('');
  const [uploadingImage, setUploadingImage] = useState(false);

  const updateField = <K extends keyof NewsInputsData>(field: K, value: NewsInputsData[K]) => {
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

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
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

      try {
        setUploadingImage(true);
        
        const response = await handleImageUploadService(file);
        updateField('mainPicture', response);
        setUploadingImage(false);
      } catch (error) {
        console.log(error);
        setUploadingImage(false);
      }
    }
  };

  const removeImage = () => {
    updateField('mainPicture', '');
  };

  return (
    <div className="news-inputs">

      <div className="news-inputs__container">
        {/* Title */}
        <div className="news-inputs__field">
          <label htmlFor="title" className="news-inputs__label">
            {t('admin.news.title', 'Title')} <span className="required">*</span>
          </label>
          <input
            type="text"
            id="title"
            value={data.title}
            onChange={(e) => updateField('title', e.target.value)}
            className={`news-inputs__input ${errors.title ? 'error' : ''}`}
            placeholder={t('admin.news.title_placeholder', 'Enter compelling article title...')}
          />
          {errors.title && (
            <p className="news-inputs__error">
              <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {errors.title}
            </p>
          )}
        </div>

        {/* Summary */}
        <div className="news-inputs__field">
          <label htmlFor="summary" className="news-inputs__label">
            {t('admin.news.summary', 'Summary')}
          </label>
          <textarea
            id="summary"
            value={data.summary}
            onChange={(e) => updateField('summary', e.target.value)}
            rows={3}
            className={`news-inputs__textarea ${errors.summary ? 'error' : ''}`}
            placeholder={t('admin.news.summary_placeholder', 'Write a brief summary that captures the essence of your article...')}
          />
          <div className="news-inputs__summary-info">
            <div className="news-inputs__summary-info__characters">
              {data.summary.length}/200 {t('admin.news.characters', 'characters')}
            </div>
            {data.summary.length > 200 && (
              <div className="news-inputs__summary-info__warning">
                {t('admin.news.summary_too_long', 'Summary should be under 200 characters')}
              </div>
            )}
          </div>
          {errors.summary && (
            <p className="news-inputs__error">
              <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {errors.summary}
            </p>
          )}
        </div>

        {/* Main Picture */}
        <div className="news-inputs__field">
          <label className="news-inputs__label">
            {t('admin.news.main_picture', 'Main Picture')}
          </label>
          
          {!data.mainPicture ? (
            <div className={`news-inputs__image-upload__dropzone ${uploadingImage ? 'uploading' : ''}`}>
              <input
                type="file"
                accept="image/*"
                onChange={handleImageUpload}
                className="news-inputs__image-upload__input"
                id="imageUpload"
                disabled={uploadingImage}
              />
              <label
                htmlFor="imageUpload"
                className={`news-inputs__image-upload__label ${uploadingImage ? 'uploading' : ''}`}
              >
                <div className="news-inputs__image-upload__content">
                  <svg className="news-inputs__image-upload__icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                  </svg>
                  <div className="news-inputs__image-upload__text">
                    {uploadingImage ? (
                      <span className="news-inputs__image-upload__upload-text">
                        <svg className="upload-icon" fill="none" viewBox="0 0 24 24">
                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                          <path className="opacity-75" fill="currentColor" d="m4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                        {t('admin.news.uploading', 'Uploading...')}
                      </span>
                    ) : (
                      <>
                        <span className="news-inputs__image-upload__click-text">
                          {t('admin.news.click_to_upload', 'Click to upload')}
                        </span>
                        <span className="news-inputs__image-upload__drag-text"> {t('admin.news.or_drag_drop', 'or drag and drop')}</span>
                      </>
                    )}
                  </div>
                  <div className="news-inputs__image-upload__file-info">
                    PNG, JPG, GIF {t('admin.news.up_to', 'up to')} 5MB
                  </div>
                </div>
              </label>
            </div>
          ) : (
            <div className="news-inputs__image-preview">
              <div className="news-inputs__image-preview__container">
                <img
                  src={data.mainPicture}
                  alt={t('admin.news.main_picture_preview', 'Main picture preview')}
                  className="news-inputs__image-preview__image"
                  onError={(e) => {
                    const target = e.target as HTMLImageElement;
                    target.src = 'https://via.placeholder.com/400x200?text=Invalid+Image';
                  }}
                />
                <div className="news-inputs__image-preview__overlay">
                  <div className="news-inputs__image-preview__actions">
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
              
              <div className="news-inputs__image-preview__info">
                <span className="news-inputs__image-preview__info__status">
                  {t('admin.news.image_uploaded', 'Image uploaded successfully')}
                </span>
                <div className="news-inputs__image-preview__info__replace">
                  <input
                    type="file"
                    accept="image/*"
                    onChange={handleImageUpload}
                    className="news-inputs__image-upload__input"
                    id="imageReplace"
                  />
                  <label
                    htmlFor="imageReplace"
                    className="news-inputs__image-preview__replace-button"
                  >
                    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
                    </svg>
                    {t('admin.news.replace', 'Replace')}
                  </label>
                </div>
              </div>
            </div>
          )}
          
          {errors.mainPicture && (
            <p className="news-inputs__error">
              <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {errors.mainPicture}
            </p>
          )}
        </div>

        {/* Author */}
        <div className="news-inputs__field">
          <label htmlFor="author" className="news-inputs__label">
            {t('admin.news.author', 'Author')}
          </label>
          <input
            type="text"
            id="author"
            value={data.author}
            onChange={(e) => updateField('author', e.target.value)}
            className={`news-inputs__input ${errors.author ? 'error' : ''}`}
            placeholder={t('admin.news.author_placeholder', 'Enter author name...')}
          />
          {errors.author && (
            <p className="news-inputs__error">
              <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {errors.author}
            </p>
          )}
        </div>

        {/* Categories */}
        <div className="news-inputs__categories">
          <div className="news-inputs__field">
            <label htmlFor="category" className="news-inputs__label">
              {t('admin.news.category', 'Category')}
            </label>
            <select
              id="category"
              value={data.category}
              onChange={(e) => updateField('category', e.target.value)}
              className={`news-inputs__select ${errors.category ? 'error' : ''}`}
            >
              <option value="">{t('admin.news.select_category', 'Select category')}</option>
              {CATEGORIES.map(cat => (
                <option key={cat} value={cat}>{cat}</option>
              ))}
            </select>
            {errors.category && (
              <p className="news-inputs__error">
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                {errors.category}
              </p>
            )}
          </div>

          <div className="news-inputs__field">
            <label htmlFor="sportCategory" className="news-inputs__label">
              {t('admin.news.sport_category', 'Sport Category')}
            </label>
            <select
              id="sportCategory"
              value={data.sportCategory}
              onChange={(e) => updateField('sportCategory', e.target.value)}
              className={`news-inputs__select ${errors.sportCategory ? 'error' : ''}`}
            >
              <option value="">{t('admin.news.select_sport', 'Select sport')}</option>
              {SPORT_CATEGORIES.map(sport => (
                <option key={sport} value={sport}>{sport}</option>
              ))}
            </select>
            {errors.sportCategory && (
              <p className="news-inputs__error">
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                {errors.sportCategory}
              </p>
            )}
          </div>
        </div>

        {/* Tags */}
        <div className="news-inputs__field">
          <label className="news-inputs__label">
            {t('admin.news.tags', 'Tags')}
          </label>
          <div className="news-inputs__tags__container">
            <div className="news-inputs__tags__input-group">
              <input
                type="text"
                value={newTag}
                onChange={(e) => setNewTag(e.target.value)}
                onKeyPress={handleKeyPress}
                className="news-inputs__tags__input"
                placeholder={t('admin.news.add_tag_placeholder', 'Type tag and press Enter...')}
              />
              <button
                type="button"
                onClick={addTag}
                disabled={!newTag.trim()}
                className="news-inputs__tags__add-button"
              >
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                {t('common.add', 'Add')}
              </button>
            </div>
            
            {data.tags.length > 0 && (
              <div className="news-inputs__tags__list">
                {data.tags.map((tag, index) => (
                  <span
                    key={index}
                    className="news-inputs__tags__tag"
                  >
                    #{tag}
                    <button
                      type="button"
                      onClick={() => removeTag(tag)}
                      className="remove-button"
                    >
                      <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </span>
                ))}
              </div>
            )}
            
            {data.tags.length === 0 && (
              <p className="news-inputs__tags__empty">
                {t('admin.news.no_tags', 'No tags added yet. Tags help categorize your article.')}
              </p>
            )}
          </div>
          {errors.tags && (
            <p className="news-inputs__error">
              <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
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

