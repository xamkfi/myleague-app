import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';

export interface NewsArticleDto {
  id: string;
  title: string;
  mainImage?: string; 
  contentHtml: string;
  summary?: string;
  author?: string;
  createdAt: string; 
  updatedAt?: string;
  category?: string;
  sportCategory?: string;
  teamCategory?: string;
  tags: string[];
  isArchived: boolean;
}

export interface NewsParameters{
  category: string;
  sportCategory: string;
  teamCategory?: string;
  /** Multi-select category filter (admin views); each value is sent as its own teamCategory param. */
  teamCategories?: string[];
  searchTerm: string;
  includeArchived?: boolean;
  page?: number;
  pageSize?: number;
}

export interface PaginationInfo {
  currentPage: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  startItem: number;
  endItem: number;
}

export interface PaginatedNewsResponse {
  data: NewsArticleDto[];
  pagination: PaginationInfo;
}

export async function newsService(params?: Partial<NewsParameters>): Promise<PaginatedNewsResponse | NewsArticleDto[]> {
  try {
    const queryParams = new URLSearchParams();

    if (params?.category) queryParams.append("category", params.category);
    if (params?.sportCategory) queryParams.append("sportCategory", params.sportCategory);
    if (params?.teamCategory) queryParams.append("teamCategory", params.teamCategory);
    params?.teamCategories?.forEach(category => queryParams.append("teamCategory", category));
    if (params?.searchTerm) queryParams.append("search", params.searchTerm);
    if (params?.includeArchived !== undefined) queryParams.append("includeArchived", params.includeArchived.toString());
    if (params?.page) queryParams.append("page", params.page.toString());
    if (params?.pageSize) queryParams.append("pageSize", params.pageSize.toString());

    const queryString = queryParams.toString();
    const response = await authFetch(`${API_URL}/News${queryString ? `?${queryString}` : ''}`, {
      method: "GET"
    });

    if (!response.ok) {
      throw new Error(await parseErrorResponse(response, 'Failed to fetch news.'));
    }

    // The backend returns the data directly, not wrapped in ApiResponse
    const data: PaginatedNewsResponse | NewsArticleDto[] = await response.json();
    return data;

  } catch (error) {
    console.error("Upload error:", error);
    throw error;
  }
}

export async function archiveNewsService(id: string) {
  try {
    const response = await authFetch(`${API_URL}/News/${id}/archive`, {
      method: "POST",
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(await parseErrorResponse(response, 'Failed to archive news article.'));
    }

    // The backend returns the data directly, not wrapped in ApiResponse
    const data: NewsArticleDto = await response.json();
    return data;

  } catch (error) {
    console.error("Archive error:", error);
    throw error;
  }
}

export async function restoreNewsService(id: string) {
  try {
    const response = await authFetch(`${API_URL}/News/${id}/restore`, {
      method: "POST",
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(await parseErrorResponse(response, 'Failed to restore news article.'));
    }

    // The backend returns the data directly, not wrapped in ApiResponse
    const data: NewsArticleDto = await response.json();
    return data;

  } catch (error) {
    console.error("Restore error:", error);
    throw error;
  }
}

export async function deleteNewsService(id: string) {
  try {
    const response = await authFetch(`${API_URL}/News/${id}`, {
      method: "DELETE",
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(await parseErrorResponse(response, 'Failed to delete news article.'));
    }

    // The backend returns a boolean wrapped in ApiResponse
    const apiResponse = await response.json();
    return apiResponse.data ?? true;

  } catch (error) {
    console.error("Delete error:", error);
    throw error;
  }
}

export async function getMainNewsArticle() {
  try {
    const response = await authFetch(`${API_URL}/News/main-news`, { method: 'GET' });
    if (!response.ok) {
      return null;
    }
    const data = await response.json();
    return data.data;
  } catch (error) {
    console.error('Failed to fetch main news:', error);
    return null;
  }
}

