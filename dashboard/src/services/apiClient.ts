import axios from 'axios';

// Create a configured axios instance
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
});

// Request interceptor (useful for auth tokens later)
apiClient.interceptors.request.use(
  (config) => {
    // Retrieve token from storage if needed
    const token = localStorage.getItem('token');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor (centralized error handling)
apiClient.interceptors.response.use(
  (response) => {
    // If the response is text/html (like Vite SPA fallback), reject it as an error
    const contentType = String(response.headers['content-type'] || '');
    if (
      contentType.includes('text/html') ||
      (typeof response.data === 'string' && response.data.trim().toLowerCase().startsWith('<!doctype html>'))
    ) {
      const customError = {
        message: 'Invalid response format (HTML received instead of JSON)',
        status: 406,
        originalError: new Error('HTML received instead of JSON'),
      };
      return Promise.reject(customError);
    }
    return response;
  },
  (error) => {
    const customError = {
      message: error.response?.data?.message || 'Something went wrong',
      status: error.response?.status,
      originalError: error,
    };
    return Promise.reject(customError);
  }
);

export default apiClient;
