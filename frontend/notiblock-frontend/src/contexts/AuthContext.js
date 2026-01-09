import { createContext } from 'react';

/**
 * Authentication Context
 * Provides user state and auth methods to the entire app
 */
export const AuthContext = createContext({
    user: null,
    loading: true,
    error: null,
    login: async () => { },
    register: async () => { },
    logout: async () => { },
    refreshUser: async () => { },
    clearError: () => { },
});

export default AuthContext;
