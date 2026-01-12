import axios from 'axios';

const API_URL = 'https://localhost:7179/api';

// Event emitter for auth state changes (for cross-component communication)
const authEventListeners = new Set();

export const authEvents = {
    emit: (event) => {
        authEventListeners.forEach(listener => listener(event));
    },
    subscribe: (listener) => {
        authEventListeners.add(listener);
        return () => authEventListeners.delete(listener);
    },
};

// Create axios instance with default config
const apiClient = axios.create({
    baseURL: API_URL,
    withCredentials: true, // Important: sends HTTP-only cookies
    timeout: 30000, // 30 second timeout
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request interceptor (for future enhancements like CSRF tokens)
apiClient.interceptors.request.use(
    (config) => {
        // Future: Add CSRF token from meta tag or cookie here
        // const csrfToken = document.querySelector('meta[name="csrf-token"]')?.content;
        // if (csrfToken) {
        //     config.headers['X-CSRF-Token'] = csrfToken;
        // }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Response interceptor for error handling and auth state management
apiClient.interceptors.response.use(
    (response) => response.data,
    (error) => {
        const status = error.response?.status;

        // Handle authentication errors
        if (status === 401) {
            // User is not authenticated - emit event for AuthContext to handle
            authEvents.emit({ type: 'UNAUTHORIZED' });

            // Don't redirect here - let the auth context handle it
            // This prevents redirect loops during login attempts
            const err = new Error(error.response?.data?.message || 'Unauthorized');
            err.response = error.response;
            return Promise.reject(err);
        }

        // Handle forbidden access
        if (status === 403) {
            authEvents.emit({ type: 'FORBIDDEN' });
            const err = new Error(error.response?.data?.message || 'Access forbidden');
            err.response = error.response;
            return Promise.reject(err);
        }

        // Handle not found
        if (status === 404) {
            const err = new Error(error.response?.data?.message || 'Resource not found');
            err.response = error.response;
            return Promise.reject(err);
        }

        // Handle server errors
        if (status === 500) {
            const err = new Error(error.response?.data?.message || 'Internal server error');
            err.response = error.response;
            return Promise.reject(err);
        }

        // Handle network errors
        if (!error.response) {
            const err = new Error('Network error - please check your connection');
            return Promise.reject(err);
        }

        // Generic error
        return Promise.reject(error);
    }
);

// Legacy apiFetch for backward compatibility
export const apiFetch = async (endpoint, options = {}) => {
    const { method = 'GET', body, headers, ...restOptions } = options;

    return await apiClient({
        url: endpoint,
        method,
        data: body ? JSON.parse(body) : undefined,
        headers,
        ...restOptions,
    });
};

export default apiClient;