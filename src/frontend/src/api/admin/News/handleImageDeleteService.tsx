
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}
const API_URL = import.meta.env.VITE_API_URL || '/api';

export async function handleImageDeleteService(imageUrl: string) {

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
