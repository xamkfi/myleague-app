import type { NewsArticleDto } from '../../news/newsService';

interface UpdateNewsData {
  title: string;
  contentHtml: string;
  mainImage?: string | null;
  summary?: string | null;
  imageUrls: (string | null)[];
  author?: string | null;
  category?: string | null;
  sportCategory?: string | null;
  tags: (string | null)[];
}

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

const API_URL = import.meta.env.VITE_API_URL || '/api';

export async function UpdateNewsService(id: string, newsData: UpdateNewsData): Promise<NewsArticleDto> {
  try {
    const response = await fetch(`${API_URL}/News/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(newsData),
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.log("Update error response:", errorText);
      throw new Error("Failed to update news article.");
    }

    const data: ApiResponse<NewsArticleDto> = await response.json();
    return data.data;

  } catch (error) {
    console.error("Update error:", error);
    throw error;
  }
} 