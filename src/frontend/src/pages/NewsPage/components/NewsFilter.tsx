
import React, { useState } from 'react';

interface NewsFilterProps {
  onFilterChange: (filters: {
    category: string;
    sportCategory: string;
    searchTerm: string;
  }) => void;
}

export default function NewsFilter({onFilterChange}: NewsFilterProps) {

  const [category, setCategory] = useState('');
  const [sportCategory, setSportCategory] = useState('');
  const [searchTerm, setSearchTerm] = useState('');

    const categories = ["Transfer", "GameResult", "Announcement"];
    const sportCategories = ["Floorball", "Hockey", "Football"];

  const handleFilterChange = (
    updated: Partial<{ category: string; sportCategory: string; searchTerm: string }>
  ) => {
    const newFilters = {
      category: updated.category ?? category,
      sportCategory: updated.sportCategory ?? sportCategory,
      searchTerm: updated.searchTerm ?? searchTerm,
    };

    setCategory(newFilters.category);
    setSportCategory(newFilters.sportCategory);
    setSearchTerm(newFilters.searchTerm);

    onFilterChange(newFilters);
  };


  return (
    <div className="news-filter-panel p-4 bg-white rounded shadow-md flex flex-wrap gap-4 items-center">
      {/* Category dropdown */}
      <select
        value={category}
        onChange={(e) => handleFilterChange({ category: e.target.value })}
        className="border rounded px-3 py-2"
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
        className="border rounded px-3 py-2"
      >
        <option value="">All Sports</option>
        {sportCategories.map((sport) => (
          <option key={sport} value={sport}>
            {sport}
          </option>
        ))}
      </select>

      {/* Search input */}
      <div className="flex items-center max-w-md w-full rounded-lg overflow-hidden border border-gray-300 focus-within:ring-2 focus-within:ring-blue-500">
        <input
          type="search"
          placeholder="Search news..."
          value={searchTerm}
          onChange={(e) => handleFilterChange({ searchTerm: e.target.value })}
          className="flex-grow px-4 py-2 outline-none"
        />
        <button
          className="bg-blue-600 text-white px-4 py-2 hover:bg-blue-700 transition-colors duration-300"
        >
          Search
        </button>
      </div>

    </div>
  );
}
