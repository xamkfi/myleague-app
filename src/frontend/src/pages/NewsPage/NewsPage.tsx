import { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './NewsPage.scss';
import NewsCard from './components/NewsCard';
import NewsFilter from './components/NewsFilter';
import { newsService, type NewsArticleDto, type NewsParameters } from '../../api/news/newsService';


function NewsPage() {

  const { t } = useTranslation();
  const [newsList, setNewsList] = useState<NewsArticleDto[]>([]);
  
  const [filters, setFilters] = useState<NewsParameters>({
    category: '',
    sportCategory: '',
    searchTerm: ''
  });

  const RetrieveNews = useCallback(async () => {
    console.log("Filters changed:", filters);
    const response = await newsService(filters);
    setNewsList(response);
  }, [filters]);

  useEffect(() => {
    //Fetch new data when categories change.
    RetrieveNews();
  }, [RetrieveNews]);

  return (
    <PageTemplate title={t('nav.news')}>

      <div className="space-y-10">
        <NewsFilter onFilterChange={setFilters}/>

        {newsList.map((item) => (
          <NewsCard key={item.id} news={item}/>
        ))}
      </div>
    </PageTemplate>
  );
}

export default NewsPage; 