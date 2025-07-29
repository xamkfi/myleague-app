import { useTranslation } from 'react-i18next';
import { useState, useEffect, useMemo, useCallback } from 'react';

interface NewsFilterProps {
  onFilterChange: (filters: {
    category: string;
    sportCategory: string;
    searchTerm: string;
  }) => void;
}

export const NewsCategory = {
  None: 'None',
  General: 'General',
  MatchReports: 'MatchReports',
  LeagueNews: 'LeagueNews',
  PlayerUpdates: 'PlayerUpdates',
  TeamNews: 'TeamNews',
  Announcements: 'Announcements',
  Events: 'Events',
  Transfers: 'Transfers',
  Injuries: 'Injuries',
  Awards: 'Awards',
};

export const SportsCategory = {
  None: 'None',
  Floorball: 'Floorball',
  Icehockey: 'Icehockey',
  Football: 'Football',
};

export default function NewsFilter({onFilterChange}: NewsFilterProps) {

  const { t } = useTranslation();
  const [category, setCategory] = useState('');
  const [sportCategory, setSportCategory] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');

  // Memoize the filtered categories to avoid unnecessary recalculations
  const categories = useMemo(() => 
    Object.values(NewsCategory).filter(value => value !== NewsCategory.None), 
    []
  );
  
  const sportCategories = useMemo(() => 
    Object.values(SportsCategory).filter(value => value !== SportsCategory.None), 
    []
  );

  // Debounce search term to avoid too many API calls
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, 300); // 300ms delay

    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Memoize the filter change handler to prevent unnecessary re-renders
  const handleFilterChange = useCallback((
    updated: Partial<{ category: string; sportCategory: string; searchTerm: string }>
  ) => {
    const newFilters = {
      category: updated.category ?? category,
      sportCategory: updated.sportCategory ?? sportCategory,
      searchTerm: updated.searchTerm ?? debouncedSearchTerm,
    };

    setCategory(newFilters.category);
    setSportCategory(newFilters.sportCategory);
    setSearchTerm(newFilters.searchTerm);

    onFilterChange(newFilters);
  }, [category, sportCategory, debouncedSearchTerm, onFilterChange]);

  // Auto-trigger filter change when debounced search term changes
  useEffect(() => {
    handleFilterChange({ searchTerm: debouncedSearchTerm });
  }, [debouncedSearchTerm, handleFilterChange]);

  return (
    <div className="news-filter-panel">
      {/* Category dropdown */}
      <select
        value={category}
        onChange={(e) => handleFilterChange({ category: e.target.value })}
        className="category-dropdown"
      >
        <option value="">All Categories</option>
        {categories.map((cat) => (
          <option key={cat} value={cat}>
            {cat}
          </option>
        ))}
      </select>

      {/* Sport category dropdown */}
      <select
        value={sportCategory}
        onChange={(e) => handleFilterChange({ sportCategory: e.target.value })}
        className="category-dropdown"
      >
        <option value="">All Sports</option>
        {sportCategories.map((sport) => (
          <option key={sport} value={sport}>
            {sport}
          </option>
        ))}
      </select>

      {/* Search input */}
      <div className="search-input">
        <input
          type="search"
          placeholder={t("newsPage.searchPlaceholder")}
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)} 
          className="flex-grow px-4 py-2 outline-none"
        />
      </div>

    </div>
  );
}
