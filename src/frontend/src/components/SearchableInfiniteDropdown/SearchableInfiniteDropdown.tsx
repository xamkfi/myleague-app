import React, { useState, useEffect, useRef, useCallback } from 'react';
import './SearchableInfiniteDropdown.scss';

interface DropdownOption {
  id: string;
  name: string;
  [key: string]: unknown; // Allow additional properties
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
  onEnterSelect?: () => void; // Callback for moving to next field
}

const SearchableInfiniteDropdown = ({
  placeholder = "Select an option",
  value,
  onChange,
  onSearch,
  disabled = false,
  required = false,
  className = "",
  emptyMessage = "No options found",
  searchPlaceholder = "Search...",
  onEnterSelect
}: SearchableInfiniteDropdownProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [options, setOptions] = useState<DropdownOption[]>([]);
  const [loading, setLoading] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [page, setPage] = useState(1);
  const [error, setError] = useState<string | null>(null);
  const [highlightedIndex, setHighlightedIndex] = useState(-1);

  const dropdownRef = useRef<HTMLDivElement>(null);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLDivElement>(null);
  const loadMoreTriggerRef = useRef<HTMLDivElement>(null);
  const searchTimeoutRef = useRef<number | undefined>(undefined);
  const optionRefs = useRef<(HTMLDivElement | null)[]>([]);

  const selectedOption = options.find(option => option.id === value);

  // Auto-highlight first option when options change (but not during loading more)
  useEffect(() => {
    // Only auto-highlight if we're not loading more data (which appends to existing options)
    if (!loadingMore && options.length > 0) {
      setHighlightedIndex(0);
    } else if (options.length === 0) {
      setHighlightedIndex(-1);
    }
  }, [options, loadingMore]);

  // Scroll highlighted option into view
  useEffect(() => {
    if (highlightedIndex >= 0 && highlightedIndex < optionRefs.current.length) {
      const highlightedElement = optionRefs.current[highlightedIndex];
      if (highlightedElement && listRef.current) {
        const listRect = listRef.current.getBoundingClientRect();
        const optionRect = highlightedElement.getBoundingClientRect();
        
        if (optionRect.bottom > listRect.bottom) {
          highlightedElement.scrollIntoView({ block: 'end', behavior: 'smooth' });
        } else if (optionRect.top < listRect.top) {
          highlightedElement.scrollIntoView({ block: 'start', behavior: 'smooth' });
        }
      }
    }
  }, [highlightedIndex]);

  // Load initial data
  const loadInitialData = useCallback(async (query: string = '') => {
    try {
      setLoading(true);
      setError(null);
      const result = await onSearch(query, 1);
      setOptions(result.data);
      setHasMore(result.pagination.hasNextPage);
      setPage(1);
      // Highlight first option if there are results
      setHighlightedIndex(result.data.length > 0 ? 0 : -1);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load options');
      setOptions([]);
      setHighlightedIndex(-1);
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

  // Handle keyboard navigation for dropdown container
  const handleDropdownKeyDown = useCallback((event: React.KeyboardEvent) => {
    if (!isOpen) {
      if (event.key === 'Enter' || event.key === ' ') {
        event.preventDefault();
        setIsOpen(true);
      }
      return;
    }

    // Only handle navigation keys here, not input keys
    switch (event.key) {
      case 'Escape':
        event.preventDefault();
        setIsOpen(false);
        setHighlightedIndex(-1);
        break;

      case 'Tab':
        if (highlightedIndex >= 0 && highlightedIndex < options.length) {
          event.preventDefault();
          const selectedOption = options[highlightedIndex];
          onChange(selectedOption.id);
          setIsOpen(false);
          setSearchQuery('');
          setHighlightedIndex(-1);
          onEnterSelect?.();
        } else {
          // Allow tab to close dropdown and move to next field
          setIsOpen(false);
          setSearchQuery('');
          setHighlightedIndex(-1);
        }
        break;

      default:
        break;
    }
  }, [isOpen, highlightedIndex, options, onChange, onEnterSelect]);

  // Handle keyboard navigation for search input
  const handleSearchKeyDown = useCallback((event: React.KeyboardEvent) => {
    if (!isOpen) return;

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        setHighlightedIndex(prev => {
          if (prev === -1) {
            // If nothing is highlighted, go to first option
            return options.length > 0 ? 0 : -1;
          }
          // Move down the list (increase index), wrap to beginning when reaching end
          return prev + 1 >= options.length ? 0 : prev + 1;
        });
        break;

      case 'ArrowUp':
        event.preventDefault();
        setHighlightedIndex(prev => {
          if (prev === -1) {
            // If nothing highlighted, go to last option
            return options.length > 0 ? options.length - 1 : -1;
          }
          if (prev === 0) {
            // If at first option, wrap to last option
            return options.length - 1;
          }
          // Move up the list (decrease index)
          return prev - 1;
        });
        break;

      case 'Enter':
        event.preventDefault();
        if (highlightedIndex >= 0 && highlightedIndex < options.length) {
          const selectedOption = options[highlightedIndex];
          onChange(selectedOption.id);
          setIsOpen(false);
          setSearchQuery('');
          setHighlightedIndex(-1);
          onEnterSelect?.();
        }
        break;

      case 'Escape':
        event.preventDefault();
        setIsOpen(false);
        setHighlightedIndex(-1);
        break;

      default:
        break;
    }
  }, [isOpen, highlightedIndex, options, onChange, onEnterSelect]);

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
        setHighlightedIndex(-1);
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
    if (!isOpen) {
      setHighlightedIndex(-1);
    }
  };

  const handleOptionSelect = (option: DropdownOption) => {
    onChange(option.id);
    setIsOpen(false);
    setSearchQuery('');
    setHighlightedIndex(-1);
    onEnterSelect?.();
  };

  const handleMouseEnter = (index: number) => {
    setHighlightedIndex(index);
  };

  return (
    <div
      ref={dropdownRef}
      className={`searchable-infinite-dropdown ${className} ${disabled ? 'disabled' : ''} ${isOpen ? 'open' : ''}`}
      onKeyDown={handleDropdownKeyDown}
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
              onKeyDown={handleSearchKeyDown}
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
                {options.map((option, index) => (
                  <div
                    key={option.id}
                    ref={(el) => { optionRefs.current[index] = el; }}
                    className={`dropdown-option ${option.id === value ? 'selected' : ''} ${index === highlightedIndex ? 'highlighted' : ''}`}
                    onClick={() => handleOptionSelect(option)}
                    onMouseEnter={() => handleMouseEnter(index)}
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