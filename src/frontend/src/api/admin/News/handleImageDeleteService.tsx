
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}
import { VITE_API_URL } from '../../../constants/config';

const API_URL = VITE_API_URL;

export async function handleImageDeleteService(imageUrl: string) {

  console.log("Deleting image:", imageUrl);
  try {
    const response = await fetch(`${API_URL}/News/delete-image?url=${encodeURIComponent(imageUrl)}`, {
      method: "DELETE",
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.log("Delete error response:", errorText);
      throw new Error("Image deletion failed");
    }

    const result: ApiResponse<string> = await response.json();
    return result;
  } catch (error) {
    console.error("Delete error:", error);
    throw error;
  }
}
