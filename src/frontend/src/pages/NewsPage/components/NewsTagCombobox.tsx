import { useEffect, useMemo, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { formatNewsTagLabel } from '../newsListFilters';

const TAG_ROWS = 2;

function tagsPerPageForWidth(width: number): number {
  if (width <= 480) {
    return 3 * TAG_ROWS;
  }
  if (width <= 768) {
    return 5 * TAG_ROWS;
  }
  return 10 * TAG_ROWS;
}

type NewsTagComboboxProps = {
  tags: string[];
  selectedTag: string;
  onChange: (tag: string) => void;
};

export default function NewsTagCombobox({ tags, selectedTag, onChange }: NewsTagComboboxProps) {
  const { t } = useTranslation();
  const rootRef = useRef<HTMLDivElement>(null);
  const searchRef = useRef<HTMLInputElement>(null);
  const [isOpen, setIsOpen] = useState(false);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [tagsPerPage, setTagsPerPage] = useState(() => tagsPerPageForWidth(window.innerWidth));

  const matchingTags = useMemo(() => {
    const needle = search.trim().toLocaleLowerCase().replace(/^#+/, '');
    if (!needle) {
      return tags;
    }
    return tags.filter((tag) => tag.toLocaleLowerCase().replace(/^#+/, '').includes(needle));
  }, [tags, search]);

  const totalPages = Math.max(1, Math.ceil(matchingTags.length / tagsPerPage));
  const currentPage = Math.min(page, totalPages);
  const pageStart = (currentPage - 1) * tagsPerPage;
  const pageTags = matchingTags.slice(pageStart, pageStart + tagsPerPage);

  useEffect(() => {
    const syncPageSize = (): void => {
      setTagsPerPage(tagsPerPageForWidth(window.innerWidth));
    };

    syncPageSize();
    window.addEventListener('resize', syncPageSize);
    return () => window.removeEventListener('resize', syncPageSize);
  }, []);

  useEffect(() => {
    setPage(1);
  }, [search, tagsPerPage]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    const handlePointerDown = (event: MouseEvent): void => {
      if (rootRef.current && !rootRef.current.contains(event.target as Node)) {
        setIsOpen(false);
        setSearch('');
        setPage(1);
      }
    };

    const handleKeyDown = (event: KeyboardEvent): void => {
      if (event.key === 'Escape') {
        setIsOpen(false);
        setSearch('');
        setPage(1);
      }
    };

    document.addEventListener('mousedown', handlePointerDown);
    document.addEventListener('keydown', handleKeyDown);
    window.setTimeout(() => searchRef.current?.focus(), 0);

    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen]);

  const selectTag = (tag: string): void => {
    onChange(tag);
    setIsOpen(false);
    setSearch('');
    setPage(1);
  };

  const label = selectedTag ? formatNewsTagLabel(selectedTag) : t('newsPage.filters.allTags');

  return (
    <div className={`news-tag-combobox${selectedTag ? ' is-active' : ''}`} ref={rootRef}>
      <button
        type="button"
        className={`category-dropdown${selectedTag ? ' is-active' : ''}`}
        aria-label={t('newsPage.filters.tag')}
        aria-expanded={isOpen}
        aria-haspopup="listbox"
        onClick={() => {
          setIsOpen((open) => {
            if (open) {
              setSearch('');
              setPage(1);
            }
            return !open;
          });
        }}
      >
        <span className="news-tag-combobox__label">{label}</span>
      </button>

      {isOpen && (
        <div className="news-tag-combobox__panel">
          <input
            ref={searchRef}
            type="search"
            className="news-tag-combobox__search"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            onKeyDown={(event) => {
              if (event.key === 'Enter') {
                event.preventDefault();
              }
            }}
            placeholder={t('newsPage.filters.tagSearch')}
            aria-label={t('newsPage.filters.tagSearch')}
          />

          <button
            type="button"
            className={`news-tag-combobox__all${!selectedTag ? ' is-selected' : ''}`}
            onClick={() => selectTag('')}
          >
            {t('newsPage.filters.allTags')}
          </button>

          <ul className="news-tag-combobox__list" role="listbox">
            {pageTags.map((tag) => (
              <li key={tag}>
                <button
                  type="button"
                  className={`news-tag-combobox__option${selectedTag === tag ? ' is-selected' : ''}`}
                  onClick={() => selectTag(tag)}
                  title={formatNewsTagLabel(tag)}
                >
                  {formatNewsTagLabel(tag)}
                </button>
              </li>
            ))}
            {matchingTags.length === 0 && (
              <li className="news-tag-combobox__empty">{t('newsPage.filters.noTags')}</li>
            )}
          </ul>

          {matchingTags.length > tagsPerPage && (
            <div className="news-tag-combobox__pager">
              <button
                type="button"
                disabled={currentPage <= 1}
                onClick={() => setPage((current) => Math.max(1, current - 1))}
              >
                {t('newsPage.filters.previous')}
              </button>
              <span>
                {t('newsPage.filters.tagPage', { page: currentPage, total: totalPages })}
              </span>
              <button
                type="button"
                disabled={currentPage >= totalPages}
                onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
              >
                {t('newsPage.filters.next')}
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
