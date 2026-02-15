import { useState, useEffect, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import NewsList from './components/NewsList';
import Button from '../../../components/Button/Button';
import AddIcon from '../../../assets/basicIcons/add.svg';
import './NewsManagementPage.scss';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import NewsFilter, { type NewsFilters } from './components/NewsFilter';

const NewsManagementPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [filters, setFilters] = useState<NewsFilters>({
    category: '',
    sportCategory: '',
    searchTerm: '',
    includeArchived: true,
  });

  // Only debounce searchTerm
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState(filters.searchTerm);

  useEffect(() => {
    const timeout = setTimeout(() => {
      setDebouncedSearchTerm(filters.searchTerm);
    }, 500); // 500 ms delay for search only

    return () => clearTimeout(timeout);
  }, [filters.searchTerm]);
    
    const handleFiltersChange = (updatedFilters: Partial<NewsFilters>) => {
      setFilters(prevFilters => ({
        ...prevFilters,
        ...updatedFilters,
      }));
    };

  const combinedFilters: NewsFilters = useMemo(() => ({
    category: filters.category,
    sportCategory: filters.sportCategory,
    includeArchived: filters.includeArchived,
    searchTerm: debouncedSearchTerm,
  }), [filters.category, filters.sportCategory, filters.includeArchived, debouncedSearchTerm]);

  const handleClearFilters = () => {
    const resetFilters: NewsFilters = {
      category: '',
      sportCategory: '',
      searchTerm: '',
      includeArchived: true,
    };
    setFilters(resetFilters);
    setDebouncedSearchTerm('');
  };

  const handleCreateNew = () => {
    navigate('/admin/news/create');
  };

  return (
    <PageTemplate title={t('admin.news.pageTitle', 'News Management')}>
    <div className="news-management-page">
      <div className="page-header">
        <h1>{t('admin.news.pageTitle', 'News Management')}</h1>
        <Button
          iconLeft={AddIcon}
          rounded="pill"
          onClick={handleCreateNew}
        >
          {t('admin.news.createNew', 'Create New')}
        </Button>
      </div>
      
      <NewsFilter
        filters={filters}
        onFiltersChange={handleFiltersChange}
        onClearFilters={handleClearFilters}
      />
      <div className="news-content">
        <NewsList filters={combinedFilters} />
      </div>
    </div>
    </PageTemplate>
  );
};

export default NewsManagementPage;
