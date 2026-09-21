import { authFetch } from '../../utils/authFetch';
import { parseErrorResponse } from '../../utils/ParseErrorResponse';
import { API_URL } from '../../../constants/config';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

interface News{
  title: string,
  mainImage: string | null,
  contentHtml: string,
  summary: string | null,
  author: string | null,
  category: string | null,
  sportCategory: string | null,
  tags: string[] |null
}

export async function CreateNewsService(news: News){
    try {
        const response = await authFetch(`${API_URL}/News`, { 
          method: "POST",
          headers: {
            "Content-Type": "application/json" // serialize object to JSON
          },
          body: JSON.stringify(news)
        });
    
        if (!response.ok) {
            throw new Error(await parseErrorResponse(response, 'Failed to publish news article.'));
        }
        const data: ApiResponse<string> = await response.json();
        return data.data;
      } catch (error) {
        console.error("Upload error:", error);
        throw error;
      }
};