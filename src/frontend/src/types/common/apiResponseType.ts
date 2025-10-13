export interface ApiResponse<T> {
   success: boolean;
   data: T;
   message: string;
   errors: null | string[];
}