import { useTranslation } from 'react-i18next';
import FieldError from './FieldError';
import { SUMMARY_MAX_LENGTH, type NewsFormErrors } from './newsForm';

interface NewsTitleFieldsProps {
  title: string;
  summary: string;
  errors: NewsFormErrors;
  onTitleChange: (value: string) => void;
  onSummaryChange: (value: string) => void;
}

/** Headline and lead paragraph at the top of the news editor. */
export default function NewsTitleFields({
  title,
  summary,
  errors,
  onTitleChange,
  onSummaryChange,
}: NewsTitleFieldsProps) {
  const { t } = useTranslation();
  const summaryTooLong: boolean = summary.length > SUMMARY_MAX_LENGTH;

  return (
    <div className="news-field-group">
      <div className="news-field">
        <label htmlFor="news-title" className="news-field__label">
          {t('admin.news.title')}
          <span className="news-field__required" aria-hidden="true">*</span>
        </label>
        <input
          id="news-title"
          type="text"
          className={`news-field__input news-field__input--title${errors.title ? ' news-field__input--invalid' : ''}`}
          value={title}
          onChange={(event) => onTitleChange(event.target.value)}
          placeholder={t('admin.news.title_placeholder')}
          aria-invalid={Boolean(errors.title)}
          aria-describedby={errors.title ? 'news-title-error' : undefined}
          aria-required="true"
        />
        <FieldError id="news-title-error" message={errors.title} />
      </div>

      <div className="news-field">
        <div className="news-field__label-row">
          <label htmlFor="news-summary" className="news-field__label">
            {t('admin.news.summary')}
          </label>
          <span
            id="news-summary-count"
            className={`news-field__counter${summaryTooLong ? ' news-field__counter--over' : ''}`}
          >
            {summary.length}/{SUMMARY_MAX_LENGTH}
          </span>
        </div>
        <textarea
          id="news-summary"
          rows={3}
          className={`news-field__input news-field__textarea${errors.summary || summaryTooLong ? ' news-field__input--invalid' : ''}`}
          value={summary}
          onChange={(event) => onSummaryChange(event.target.value)}
          placeholder={t('admin.news.summary_placeholder')}
          aria-invalid={Boolean(errors.summary) || summaryTooLong}
          aria-describedby={errors.summary ? 'news-summary-count news-summary-error' : 'news-summary-count'}
        />
        <FieldError id="news-summary-error" message={errors.summary} />
      </div>
    </div>
  );
}
