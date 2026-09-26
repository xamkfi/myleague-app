import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

export interface AdminTeamListFilters {
  seasonId: string | null;
  divisionId: string;
  clubId: string;
  categories: string[];
  searchTerm: string;
  debouncedSearch: string;
  page: number;
  pageSize: number;
  setSeasonId: (seasonId: string) => void;
  setDivisionId: (divisionId: string) => void;
  setClubId: (clubId: string) => void;
  setCategories: (categories: string[]) => void;
  setSearchTerm: (value: string) => void;
  setPage: (page: number) => void;
  setPageSize: (pageSize: number) => void;
}

function readPositiveInt(value: string | null, fallback: number): number {
  const parsed = Number(value);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback;
}

export function useAdminTeamListFilters(): AdminTeamListFilters {
  const [searchParams, setSearchParams] = useSearchParams();
  const queryFromUrl = searchParams.get('q') ?? '';
  const writtenQuery = useRef(queryFromUrl);
  const [searchTerm, setSearchTerm] = useState(queryFromUrl);

  useEffect(() => {
    if (queryFromUrl !== writtenQuery.current) {
      writtenQuery.current = queryFromUrl;
      setSearchTerm(queryFromUrl);
    }
  }, [queryFromUrl]);

  useEffect(() => {
    const trimmed = searchTerm.trim();
    const timeoutId = window.setTimeout(() => {
      setSearchParams((current) => {
        const existing = current.get('q') ?? '';
        if (existing === trimmed) {
          return current;
        }
        const next = new URLSearchParams(current);
        if (trimmed) {
          next.set('q', trimmed);
        } else {
          next.delete('q');
        }
        next.delete('page');
        writtenQuery.current = trimmed;
        return next;
      }, { replace: true });
    }, 300);
    return () => window.clearTimeout(timeoutId);
  }, [searchTerm, setSearchParams]);

  const replaceParams = useCallback((mutate: (next: URLSearchParams) => void, resetPage = true): void => {
    setSearchParams((current) => {
      const next = new URLSearchParams(current);
      mutate(next);
      if (resetPage) {
        next.delete('page');
      }
      return next;
    }, { replace: true });
  }, [setSearchParams]);

  const categoryKey = searchParams.getAll('category').join('|');
  const categories = useMemo(
    () => (categoryKey ? categoryKey.split('|') : []),
    [categoryKey],
  );

  const setSeasonId = useCallback((seasonId: string) => {
    replaceParams((next) => {
      next.set('season', seasonId);
      next.delete('division');
    });
  }, [replaceParams]);

  const setDivisionId = useCallback((divisionId: string) => {
    replaceParams((next) => {
      if (divisionId) {
        next.set('division', divisionId);
      } else {
        next.delete('division');
      }
    });
  }, [replaceParams]);

  const setClubId = useCallback((clubId: string) => {
    replaceParams((next) => {
      if (clubId) {
        next.set('club', clubId);
      } else {
        next.delete('club');
      }
    });
  }, [replaceParams]);

  const setCategories = useCallback((nextCategories: string[]) => {
    replaceParams((next) => {
      next.delete('category');
      nextCategories.forEach((category) => next.append('category', category));
    });
  }, [replaceParams]);

  const setPage = useCallback((page: number) => {
    replaceParams((next) => {
      if (page <= 1) {
        next.delete('page');
      } else {
        next.set('page', String(page));
      }
    }, false);
  }, [replaceParams]);

  const setPageSize = useCallback((pageSize: number) => {
    replaceParams((next) => {
      next.set('pageSize', String(pageSize));
    });
  }, [replaceParams]);

  return {
    seasonId: searchParams.get('season'),
    divisionId: searchParams.get('division') ?? '',
    clubId: searchParams.get('club') ?? '',
    categories,
    searchTerm,
    debouncedSearch: queryFromUrl,
    page: readPositiveInt(searchParams.get('page'), 1),
    pageSize: readPositiveInt(searchParams.get('pageSize'), 50),
    setSeasonId,
    setDivisionId,
    setClubId,
    setCategories,
    setSearchTerm,
    setPage,
    setPageSize,
  };
}
