export type NewsListFilters = {
  category: string;
  sportCategory: string;
  tag: string;
  searchTerm: string;
};

export const EMPTY_NEWS_LIST_FILTERS: NewsListFilters = {
  category: '',
  sportCategory: '',
  tag: '',
  searchTerm: '',
};

export function newsListUrl(filters: Partial<NewsListFilters>): string {
  const params = new URLSearchParams();
  if (filters.category) params.set('category', filters.category);
  if (filters.sportCategory) params.set('sportCategory', filters.sportCategory);
  if (filters.tag) params.set('tag', filters.tag);
  if (filters.searchTerm) params.set('search', filters.searchTerm);
  const query = params.toString();
  return query ? `/uutiset?${query}` : '/uutiset';
}

export function newsListFiltersFromSearchParams(searchParams: URLSearchParams): NewsListFilters {
  return {
    category: searchParams.get('category') ?? '',
    sportCategory: searchParams.get('sportCategory') ?? '',
    tag: searchParams.get('tag') ?? '',
    searchTerm: searchParams.get('search') ?? '',
  };
}

export function formatNewsTagLabel(tag: string): string {
  const trimmed = tag.trim();
  if (!trimmed) {
    return trimmed;
  }
  return trimmed.startsWith('#') ? trimmed : `#${trimmed}`;
}

export function newsListFiltersToSearchParams(filters: NewsListFilters): URLSearchParams {
  const params = new URLSearchParams();
  if (filters.category) params.set('category', filters.category);
  if (filters.sportCategory) params.set('sportCategory', filters.sportCategory);
  if (filters.tag) params.set('tag', filters.tag);
  if (filters.searchTerm) params.set('search', filters.searchTerm);
  return params;
}
