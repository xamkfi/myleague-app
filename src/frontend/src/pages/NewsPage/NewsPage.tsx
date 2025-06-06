import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './NewsPage.scss';
import mockData from './mockNews.json';
import NewsCard from './components/NewsCard';
import NewsFilter from './components/NewsFilter';

interface NewsData {
  id: string;
  title: string;
  contentHtml: string;
  summary?: string;
  imageUrls: string[]; 
  author?: string; 
  createdAt: string; 
  updatedAt?: string | null; 
  category?: string; 
  sportCategory?: string; 
  tags: string[];
  isArchived: boolean;
}

interface FilterValues {
  category: string;
  sportCategory: string;
  searchTerm: string;
}


function NewsPage() {

  const { t } = useTranslation();
  const newsList: NewsData[] = mockData.news;

  
  const [filters, setFilters] = useState<FilterValues>({
    category: '',
    sportCategory: '',
    searchTerm: '',
  });


  useEffect(()=>{
    //Fetch new data when categories change.
    console.log(filters.category, filters.sportCategory, filters.searchTerm)
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