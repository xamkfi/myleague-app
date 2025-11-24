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

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

import { VITE_API_URL } from '../../constants/config';

const API_URL = VITE_API_URL;

export async function singleNewsService(id: string){

    try {
        const response = await fetch(`${API_URL}/News/${id}`, { 
          method: "GET"
        });
    
        if (!response.ok) {
            const errorText = await response.text();
            console.log("Upload error response:", errorText);
            throw new Error("Failed to fetch news.");
        }
        const data: ApiResponse<NewsArticleDto> = await response.json();
        return data.data;
      } catch (error) {
        console.error("Upload error:", error);
        throw error;
      }
  };