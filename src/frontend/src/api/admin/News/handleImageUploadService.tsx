import { authFetch } from '../../utils/authFetch';
import { parseErrorResponse } from '../../utils/ParseErrorResponse';
import { API_URL } from '../../../constants/config';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

export async function handleImageUploadService(file: File): Promise<string> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await authFetch(`${API_URL}/News/upload-image`, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    throw new Error(await parseErrorResponse(response, 'Image upload failed'));
  }

  const data: ApiResponse<string> = await response.json();
  if (!data.data) {
    throw new Error('Image upload failed');
  }

  return data.data;
}
