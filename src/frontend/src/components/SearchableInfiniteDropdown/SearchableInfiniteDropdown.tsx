import React, { useState, useEffect, useRef, useCallback } from 'react';
import './SearchableInfiniteDropdown.scss';

interface DropdownOption {
  id: string;
  name: string;
  [key: string]: any; // Allow additional properties
}

interface SearchableInfiniteDropdownProps {
  placeholder?: string;
  value?: string;
  onChange: (value: string) => void;
  onSearch: (query: string, page: number) => Promise<{
    data: DropdownOption[];
    pagination: {
      hasNextPage: boolean;
      totalCount: number;
    };
  }>;
  disabled?: boolean;
  required?: boolean;
  className?: string;
  emptyMessage?: string;
  searchPlaceholder?: string;
}

const SearchableInfiniteDropdown: React.FC<SearchableInfiniteDropdownProps> = ({
  placeholder = "Select an option",
  value,
  onChange,
  onSearch,
  disabled = false,
  required = false,
  className = "",
  emptyMessage = "No options found",
  searchPlaceholder = "Search..."
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [options, setOptions] = useState<DropdownOption[]>([]);
  const [loading, setLoading] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [page, setPage] = useState(1);
  const [error, setError] = useState<string | null>(null);

  const dropdownRef = useRef<HTMLDivElement>(null);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLDivElement>(null);
  const loadMoreTriggerRef = useRef<HTMLDivElement>(null);
  const searchTimeoutRef = useRef<number>();

  const selectedOption = options.find(option => option.id === value);

  // Load initial data
  const loadInitialData = useCallback(async (query: string = '') => {
    try {
      setLoading(true);
      setError(null);
      const result = await onSearch(query, 1);
      setOptions(result.data);
      setHasMore(result.pagination.hasNextPage);
      setPage(1);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load options');
      setOptions([]);
    } finally {
      setLoading(false);
    }
  }, [onSearch]);

  // Load more data for infinite scroll
  const loadMoreData = useCallback(async () => {
    if (loadingMore || !hasMore) return;

    try {
      setLoadingMore(true);
      setError(null);
      const result = await onSearch(searchQuery, page + 1);
      setOptions(prev => [...prev, ...result.data]);
      setHasMore(result.pagination.hasNextPage);
      setPage(prev => prev + 1);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load more options');
    } finally {
      setLoadingMore(false);
    }
  }, [onSearch, searchQuery, page, loadingMore, hasMore]);

  // Handle search with debouncing
  const handleSearch = useCallback((query: string) => {
    setSearchQuery(query);
    
    // Clear existing timeout
    if (searchTimeoutRef.current) {
      window.clearTimeout(searchTimeoutRef.current);
    }

    // Debounce search
    searchTimeoutRef.current = window.setTimeout(() => {
      loadInitialData(query);
    }, 300);
  }, [loadInitialData]);

  // Intersection Observer for infinite scroll
  useEffect(() => {
    const trigger = loadMoreTriggerRef.current;
    if (!trigger) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasMore && !loadingMore) {
          loadMoreData();
        }
      },
      { threshold: 0.1 }
    );

    observer.observe(trigger);

    return () => {
      observer.disconnect();
    };
  }, [hasMore, loadingMore, loadMoreData]);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // Load initial data when component mounts
  useEffect(() => {
    if (isOpen && options.length === 0 && !loading) {
      loadInitialData();
    }
  }, [isOpen, options.length, loading, loadInitialData]);

  // Focus search input when dropdown opens
  useEffect(() => {
    if (isOpen && searchInputRef.current) {
      searchInputRef.current.focus();
    }
  }, [isOpen]);

  const handleToggle = () => {
    if (disabled) return;
    setIsOpen(!isOpen);
  };

  const handleOptionSelect = (option: DropdownOption) => {
    onChange(option.id);
    setIsOpen(false);
    setSearchQuery('');
  };

  const handleKeyDown = (event: React.KeyboardEvent) => {
    if (event.key === 'Escape') {
      setIsOpen(false);
    }
  };

  return (
    <div
      ref={dropdownRef}
      className={`searchable-infinite-dropdown ${className} ${disabled ? 'disabled' : ''} ${isOpen ? 'open' : ''}`}
      onKeyDown={handleKeyDown}
    >
      <div
        className="dropdown-trigger"
        onClick={handleToggle}
        role="button"
        tabIndex={disabled ? -1 : 0}
        aria-expanded={isOpen}
        aria-haspopup="listbox"
        aria-required={required}
      >
        <span className="dropdown-value">
          {selectedOption ? selectedOption.name : placeholder}
        </span>
        <span className={`dropdown-arrow ${isOpen ? 'up' : 'down'}`}>
          ▼
        </span>
      </div>

      {isOpen && (
        <div className="dropdown-content">
          <div className="dropdown-search">
            <input
              ref={searchInputRef}
              type="text"
              placeholder={searchPlaceholder}
              value={searchQuery}
              onChange={(e) => handleSearch(e.target.value)}
              className="search-input"
            />
          </div>

          <div ref={listRef} className="dropdown-list">
            {loading ? (
              <div className="dropdown-loading">
                <div className="spinner"></div>
                <span>Loading options...</span>
              </div>
            ) : error ? (
              <div className="dropdown-error">
                <span>{error}</span>
                <button onClick={() => loadInitialData(searchQuery)} className="retry-button">
                  Retry
                </button>
              </div>
            ) : options.length === 0 ? (
              <div className="dropdown-empty">
                {emptyMessage}
              </div>
            ) : (
              <>
                {options.map((option) => (
                  <div
                    key={option.id}
                    className={`dropdown-option ${option.id === value ? 'selected' : ''}`}
                    onClick={() => handleOptionSelect(option)}
                    role="option"
                    aria-selected={option.id === value}
                  >
                    {option.name}
                  </div>
                ))}

                {hasMore && (
                  <div
                    ref={loadMoreTriggerRef}
                    className="dropdown-load-more"
                  >
                    {loadingMore ? (
                      <div className="loading-more">
                        <div className="spinner small"></div>
                        <span>Loading more...</span>
                      </div>
                    ) : (
                      <div className="load-more-trigger">
                        Scroll for more...
                      </div>
                    )}
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default SearchableInfiniteDropdown; 