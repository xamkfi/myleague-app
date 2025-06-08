
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}
const API_URL = import.meta.env.VITE_API_URL || '/api';

export async function handleImageDelete(imageUrl: string) {

  try {
    const response = await fetch(`${API_URL}/news`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ imageUrl }),
    });

    if (!response.ok) {
      throw new Error("Image deletion failed");
    }

    const result = await response.json();
    return result;
  } catch (error) {
    console.error("Delete error:", error);
    throw error;
  }
}
