import { useState, type KeyboardEvent } from 'react';
import { useTranslation } from 'react-i18next';
import FieldError from './FieldError';
import type { NewsFormErrors } from './newsForm';

interface NewsDetailsFieldsProps {
  tags: string[];
  author: string;
  errors: NewsFormErrors;
  onTagsChange: (tags: string[]) => void;
  onAuthorChange: (value: string) => void;
}

/** Tags and author in the editor sidebar. */
export default function NewsDetailsFields({
  tags,
  author,
  errors,
  onTagsChange,
  onAuthorChange,
}: NewsDetailsFieldsProps) {
  const { t } = useTranslation();
  const [newTag, setNewTag] = useState('');
  const trimmedTag: string = newTag.trim();

  const addTag = (): void => {
    if (!trimmedTag) {
      return;
    }
    if (!tags.some((tag) => tag.toLowerCase() === trimmedTag.toLowerCase())) {
      onTagsChange([...tags, trimmedTag]);
    }
    setNewTag('');
  };

  const handleTagKeyDown = (event: KeyboardEvent<HTMLInputElement>): void => {
    if (event.key === 'Enter') {
      event.preventDefault();
      addTag();
    }
  };

  return (
    <div className="news-field-group">
      <div className="news-field">
        <label htmlFor="news-tags" className="news-field__label">
          {t('admin.news.tags')}
        </label>
        <div className="news-tags__input-row">
          <input
            id="news-tags"
            type="text"
            className="news-field__input"
            value={newTag}
            onChange={(event) => setNewTag(event.target.value)}
            onKeyDown={handleTagKeyDown}
            placeholder={t('admin.news.add_tag_placeholder')}
          />
          <button type="button" className="news-field__button" onClick={addTag} disabled={!trimmedTag}>
            {t('common.add')}
          </button>
        </div>
        {tags.length > 0 ? (
          <ul className="news-tags__list">
            {tags.map((tag) => (
              <li key={tag} className="news-tags__tag">
                <span>#{tag}</span>
                <button
                  type="button"
                  className="news-tags__remove"
                  onClick={() => onTagsChange(tags.filter((item) => item !== tag))}
                  aria-label={t('admin.news.remove_tag', { tag })}
                >
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" aria-hidden="true">
                    <path d="M6 6l12 12M18 6 6 18" />
                  </svg>
                </button>
              </li>
            ))}
          </ul>
        ) : (
          <p className="news-field__hint">{t('admin.news.no_tags')}</p>
        )}
      </div>

      <div className="news-field">
        <label htmlFor="news-author" className="news-field__label">
          {t('admin.news.author')}
        </label>
        <input
          id="news-author"
          type="text"
          className={`news-field__input${errors.author ? ' news-field__input--invalid' : ''}`}
          value={author}
          onChange={(event) => onAuthorChange(event.target.value)}
          placeholder={t('admin.news.author_placeholder')}
          aria-invalid={Boolean(errors.author)}
          aria-describedby={errors.author ? 'news-author-error' : undefined}
        />
        <FieldError id="news-author-error" message={errors.author} />
      </div>
    </div>
  );
}
