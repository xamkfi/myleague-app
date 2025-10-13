
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

const API_URL = import.meta.env.VITE_API_URL || '/api';

export async function CreateNewsService(news: News){
    try {
        const response = await fetch(`${API_URL}/News`, { 
          method: "POST",
          headers: {
            "Content-Type": "application/json" // serialize object to JSON
          },
          body: JSON.stringify(news)
        });
    
        if (!response.ok) {
            const errorText = await response.text();
            console.log("Upload error response:", errorText);
            throw new Error("Image upload failed");
        }
        const data: ApiResponse<string> = await response.json();
        return data.data;
      } catch (error) {
        console.error("Upload error:", error);
        throw error;
      }
};