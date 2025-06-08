interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

const API_URL = import.meta.env.VITE_API_URL || '/api';

export async function handleImageUpload(file: File){

    const formData = new FormData();
    formData.append("image", file);
  
    try {
        const response = await fetch(`${API_URL}/news`, { 
          method: "POST",
          body: formData,
        });
    
        if (!response.ok) {
          throw new Error("Image upload failed");
        }
    
        const data = await response.json();
        return data.imageUrl
      } catch (error) {
        console.error("Upload error:", error);
        throw error;
      }
  };