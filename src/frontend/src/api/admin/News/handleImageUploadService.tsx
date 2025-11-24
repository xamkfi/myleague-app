interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

import { VITE_API_URL } from '../../../constants/config';

const API_URL = VITE_API_URL;

export async function handleImageUploadService(file: File){

    const formData = new FormData();
    formData.append("file", file);
  
    try {
        const response = await fetch(`${API_URL}/News/upload-image`, { 
          method: "POST",
          body: formData,
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