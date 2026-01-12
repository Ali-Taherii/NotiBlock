import { useState, useEffect, useCallback, useRef } from 'react';
import { AuthContext } from './AuthContext';
import authService from '../api/auth';

/**
 * Authentication Provider Component
 * Manages authentication state and provides auth methods to children
 */
export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Prevent multiple simultaneous user fetches
  const fetchingRef = useRef(false);
  const fetchPromiseRef = useRef(null);
  const initializedRef = useRef(false);

  /**
   * Clear error state
   */
  const clearError = useCallback(() => {
    setError(null);
  }, []);

  /**
   * Fetch current user from backend
   * Uses ref to prevent race conditions from multiple simultaneous calls
   * Returns the same promise if called multiple times simultaneously
   */
  const refreshUser = useCallback(async () => {
    // If already fetching, return the existing promise
    if (fetchingRef.current && fetchPromiseRef.current) {
      return fetchPromiseRef.current;
    }

    fetchingRef.current = true;
    setLoading(true);
    setError(null);

    // Create and store the fetch promise
    const fetchPromise = (async () => {
      try {
        const userData = await authService.getCurrentUser();
        setUser(userData);
        return userData;
      } catch (err) {
        // 401/403 means not authenticated - this is normal, not an error
        if (err?.response?.status === 401 || err?.response?.status === 403) {
          setUser(null);
          return null;
        }
        
        // Other errors are actual problems
        console.error('Error fetching user:', err);
        setError(err?.message || 'Failed to fetch user data');
        setUser(null);
        return null;
      } finally {
        setLoading(false);
        fetchingRef.current = false;
        fetchPromiseRef.current = null;
      }
    })();

    fetchPromiseRef.current = fetchPromise;
    return fetchPromise;
  }, []);

  /**
   * Login user
   * @param {string} role - User role (consumer/manufacturer/reseller/regulator)
   * @param {Object} credentials - { email, password }
   * @returns {Promise<Object>} User data
   */
  const login = useCallback(async (role, credentials) => {
    setLoading(true);
    setError(null);
    
    // Clear any existing user state before login
    setUser(null);

    try {
      // Call login endpoint (sets HTTP-only cookie)
      const userData = await authService.login(role, credentials);
      
      // Verify the returned user has the correct role
      if (userData.role !== role) {
        console.error('Role mismatch after login:', { expected: role, received: userData.role });
        throw new Error(`Login failed: expected role '${role}' but got '${userData.role}'`);
      }
      
      console.log('Login successful:', userData);
      
      // Update state with returned user data
      setUser(userData);
      return userData;
    } catch (err) {
      const errorMessage = err?.response?.data?.message || err?.message || 'Login failed';
      setError(errorMessage);
      setUser(null); // Ensure user is null on error
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Register new user
   * @param {string} role - User role (consumer/manufacturer/reseller/regulator)
   * @param {Object} data - { email, password, name?, phoneNumber?, walletAddress? }
   * @returns {Promise<Object>} User data
   */
  const register = useCallback(async (role, data) => {
    setLoading(true);
    setError(null);

    try {
      // Call registration endpoint (sets HTTP-only cookie)
      const userData = await authService.register(role, data);
      
      // Update state with returned user data
      setUser(userData);
      return userData;
    } catch (err) {
      const errorMessage = err?.response?.data?.message || err?.message || 'Registration failed';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Logout user
   * Clears backend cookie and local state
   */
  const logout = useCallback(async () => {
    setLoading(true);
    setError(null);
    
    console.log('Logging out...');

    try {
      // Call logout endpoint to clear HTTP-only cookie
      await authService.logout();
      console.log('Logout API call successful');
    } catch (err) {
      // Log error but continue with local cleanup
      console.error('Logout API error (continuing with local cleanup):', err);
    } finally {
      // Always clear local state regardless of API success
      setUser(null);
      setLoading(false);
      console.log('User state cleared');
    }
  }, []);

  /**
   * Initialize auth state on mount
   * Fetches current user if session exists
   */
  useEffect(() => {
    if (!initializedRef.current) {
      initializedRef.current = true;
      refreshUser();
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // Empty deps - only run once on mount

  const value = {
    user,
    loading,
    error,
    login,
    register,
    logout,
    refreshUser,
    clearError,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export default AuthProvider;
