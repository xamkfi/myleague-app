import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './NewsPage.scss';
import NewsCard from './components/NewsCard';
import NewsFilter from './components/NewsFilter';
import { newsService, type NewsArticleDto } from '../../api/news/newsService';


interface FilterValues {
  category: string;
  sportCategory: string;
  searchTerm: string;
}


function NewsPage() {

  const { t } = useTranslation();
  const [newsList, setNewsList] = useState<NewsArticleDto[]>([]);
  
  const [filters, setFilters] = useState<FilterValues>({
    category: '',
    sportCategory: '',
    searchTerm: '',
  });

  async function RetrieveNews() {
    const response = await newsService();
    setNewsList(response);
  }

  useEffect(()=>{
    //Fetch new data when categories change.
    RetrieveNews();
  },[filters])

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