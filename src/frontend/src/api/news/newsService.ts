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
  tags: string[];
  isArchived: boolean;
}

export interface NewsParameters{
  category: string,
  sportCategory: string,
  searchTerm: string,
}

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

const API_URL = import.meta.env.VITE_API_URL || '/api';

export async function newsService(params?: Partial<NewsParameters>) {
  try {
    const queryParams = new URLSearchParams();

    if (params?.category) queryParams.append("category", params.category);
    if (params?.sportCategory) queryParams.append("sportCategory", params.sportCategory);
    if (params?.searchTerm) queryParams.append("search", params.searchTerm);

    const queryString = queryParams.toString();
    const response = await fetch(`${API_URL}/News${queryString ? `?${queryString}` : ''}`, {
      method: "GET"
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.log("Upload error response:", errorText);
      throw new Error("Failed to fetch news.");
    }

    const data: ApiResponse<NewsArticleDto[]> = await response.json();
    return data.data;

  } catch (error) {
    console.error("Upload error:", error);
    throw error;
  }
}