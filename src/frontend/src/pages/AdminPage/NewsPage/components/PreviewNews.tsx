import { useTranslation } from 'react-i18next';
import { type NewsInputsData } from './NewsInputs';

interface PreviewNewsProps {
  value: string;
  newsData?: NewsInputsData;
}

export default function PreviewNews({ value, newsData }: PreviewNewsProps) {
  const { t } = useTranslation();

  return (
    <div className="max-w-4xl mx-auto">
      {/* Categories */}
      {newsData && (newsData.sportCategory || newsData.category) && (
        <div className="flex flex-wrap gap-2 mb-6">
          {newsData.sportCategory && (
            <span className="bg-blue-100 text-blue-800 text-sm font-medium px-3 py-1 rounded-full">
              {newsData.sportCategory}
            </span>
          )}
          {newsData.category && (
            <span className="bg-green-100 text-green-800 text-sm font-medium px-3 py-1 rounded-full">
              {newsData.category}
            </span>
          )}
        </div>
      )}

      {/* Title */}
      <h1 className="text-3xl md:text-4xl font-bold text-gray-900 mb-4 leading-tight">
        {newsData?.title || t('admin.news.untitled', 'Untitled Article')}
      </h1>

      {/* Author and Date */}
      <div className="flex flex-wrap items-center gap-4 text-sm text-gray-500 mb-6 pb-6 border-b border-gray-200">
        {newsData?.author && (
          <div className="flex items-center gap-2">
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
            <span>{newsData.author}</span>
          </div>
        )}
        <div className="flex items-center gap-2">
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <span>{new Date().toLocaleDateString()}</span>
        </div>
      </div>

      {/* Main Picture */}
      {newsData?.mainPicture && (
        <div className="mb-8">
          <img
            src={newsData.mainPicture}
            alt={newsData.title}
            className="w-full h-64 md:h-96 object-cover rounded-lg"
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              target.src = 'https://via.placeholder.com/800x400?text=Image+Not+Found';
            }}
          />
        </div>
      )}

      {/* Content */}
      <div className="prose prose-lg max-w-none mb-8">
        {value ? (
          <div 
            dangerouslySetInnerHTML={{ __html: value }}
            className="text-gray-800 leading-relaxed"
          />
        ) : (
          <p className="text-gray-400 italic">
            {t('admin.news.no_content', 'No content written yet...')}
          </p>
        )}
      </div>

      {/* Tags */}
      {newsData?.tags && newsData.tags.length > 0 && (
        <div className="pt-6 border-t border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900 mb-3">
            {t('admin.news.tags', 'Tags')}
          </h3>
          <div className="flex flex-wrap gap-2">
            {newsData.tags.map((tag, index) => (
              <span
                key={index}
                className="bg-gray-100 text-gray-700 text-sm px-3 py-1 rounded-full"
              >
                #{tag}
              </span>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}