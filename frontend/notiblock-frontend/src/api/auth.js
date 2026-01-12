/**
 * Authentication Service
 * Handles all authentication-related API calls
 * Backend uses HTTP-only cookies for token storage (secure, not accessible via JS)
 */

import apiClient from './api';

// ============================================================================
// REGISTRATION ENDPOINTS
// ============================================================================

export const authService = {
    /**
     * Register a new consumer
     * @param {Object} data - { email, password, name?, phoneNumber?, walletAddress? }
     * @returns {Promise<Object>} User data from /auth/me endpoint
     */
    async registerConsumer(data) {
        await apiClient.post('/auth/consumer/register', data);
        // After registration, fetch user data from /auth/me
        return this.getCurrentUser();
    },

    /**
     * Register a new manufacturer
     * @param {Object} data - { email, password, name?, phoneNumber?, walletAddress? }
     * @returns {Promise<Object>} User data from /auth/me endpoint
     */
    async registerManufacturer(data) {
        await apiClient.post('/auth/manufacturer/register', data);
        return this.getCurrentUser();
    },

    /**
     * Register a new reseller
     * @param {Object} data - { email, password, name?, phoneNumber?, walletAddress? }
     * @returns {Promise<Object>} User data from /auth/me endpoint
     */
    async registerReseller(data) {
        await apiClient.post('/auth/reseller/register', data);
        return this.getCurrentUser();
    },

    /**
     * Register a new regulator
     * @param {Object} data - { email, password, name?, phoneNumber?, walletAddress? }
     * @returns {Promise<Object>} User data from /auth/me endpoint
     */
    async registerRegulator(data) {
        await apiClient.post('/auth/regulator/register', data);
        return this.getCurrentUser();
    },

    // ============================================================================
    // LOGIN ENDPOINTS
    // ============================================================================

    /**
     * Login as consumer
     * @param {Object} data - { email, password }
     * @returns {Promise<Object>} User data from /auth/me endpoint
     */
    async loginConsumer(data) {
        await apiClient.post('/auth/consumer/login', data);
        // Wait a small moment to ensure cookie is set
        await new Promise(resolve => setTimeout(resolve, 100));
        return this.getCurrentUser();
    },

    /**
     * Login as manufacturer
     * @param {Object} data - { email, password }
     * @returns {Promise<Object>} User data from /auth/me endpoint
     */
    async loginManufacturer(data) {
        await apiClient.post('/auth/manufacturer/login', data);
        await new Promise(resolve => setTimeout(resolve, 100));
        return this.getCurrentUser();
    },

    /**
     * Login as reseller
     * @param {Object} data - { email, password }
     * @returns {Promise<Object>} User data from /auth/me endpoint
     */
    async loginReseller(data) {
        await apiClient.post('/auth/reseller/login', data);
        await new Promise(resolve => setTimeout(resolve, 100));
        return this.getCurrentUser();
    },

    /**
     * Login as regulator
     * @param {Object} data - { email, password }
     * @returns {Promise<Object>} User data from /auth/me endpoint
     */
    async loginRegulator(data) {
        await apiClient.post('/auth/regulator/login', data);
        await new Promise(resolve => setTimeout(resolve, 100));
        return this.getCurrentUser();
    },

    // ============================================================================
    // USER PROFILE ENDPOINTS
    // ============================================================================

    /**
     * Get current authenticated user
     * @returns {Promise<Object>} { userId, email, role }
     */
    async getCurrentUser() {
        const response = await apiClient.get('/auth/me');
        return response?.data || response;
    },

    /**
     * Update user profile
     * @param {Object} data - Profile data to update
     * @returns {Promise<Object>} Updated user data
     */
    async updateProfile(data) {
        const response = await apiClient.put('/auth/profile', data);
        return response?.data || response;
    },

    // ============================================================================
    // PASSWORD & ACCOUNT MANAGEMENT
    // ============================================================================

    /**
     * Change user password
     * @param {Object} data - { oldPassword, newPassword }
     * @returns {Promise<Object>} Success response
     */
    async changePassword(data) {
        return await apiClient.post('/auth/change-password', data);
    },

    /**
     * Delete user account (soft delete)
     * @param {string} password - Current password for confirmation
     * @returns {Promise<Object>} Success response
     */
    async deleteAccount(password) {
        return await apiClient.delete('/auth/account', { data: { password } });
    },

    // ============================================================================
    // UTILITY ENDPOINTS
    // ============================================================================

    /**
     * Check if email is available for registration
     * @param {string} email - Email to check
     * @param {string} userType - User type (consumer/manufacturer/reseller/regulator)
     * @returns {Promise<boolean>} True if email is available
     */
    async checkEmailAvailability(email, userType) {
        const response = await apiClient.get(
            `/auth/check-email?email=${encodeURIComponent(email)}&userType=${encodeURIComponent(userType)}`
        );
        return response?.data?.available || false;
    },

    /**
     * Get user statistics for dashboard
     * @returns {Promise<Object>} User statistics
     */
    async getUserStats() {
        const response = await apiClient.get('/auth/stats');
        return response?.data || response;
    },

    // ============================================================================
    // LOGOUT
    // ============================================================================

    /**
     * Logout current user (clears HTTP-only cookie on backend)
     * @returns {Promise<Object>} Success response
     */
    async logout() {
        return await apiClient.post('/auth/logout');
    },

    // ============================================================================
    // UNIFIED REGISTRATION/LOGIN BY ROLE
    // ============================================================================

    /**
     * Register user by role (convenience method)
     * @param {string} role - User role (consumer/manufacturer/reseller/regulator)
     * @param {Object} data - Registration data
     * @returns {Promise<Object>} User data
     */
    async register(role, data) {
        const normalizedRole = role.toLowerCase();
        switch (normalizedRole) {
            case 'consumer':
                return this.registerConsumer(data);
            case 'manufacturer':
                return this.registerManufacturer(data);
            case 'reseller':
                return this.registerReseller(data);
            case 'regulator':
                return this.registerRegulator(data);
            default:
                throw new Error(`Invalid role: ${role}`);
        }
    },

    /**
     * Login user by role (convenience method)
     * @param {string} role - User role (consumer/manufacturer/reseller/regulator)
     * @param {Object} data - Login data { email, password }
     * @returns {Promise<Object>} User data
     */
    async login(role, data) {
        const normalizedRole = role.toLowerCase();
        switch (normalizedRole) {
            case 'consumer':
                return this.loginConsumer(data);
            case 'manufacturer':
                return this.loginManufacturer(data);
            case 'reseller':
                return this.loginReseller(data);
            case 'regulator':
                return this.loginRegulator(data);
            default:
                throw new Error(`Invalid role: ${role}`);
        }
    },
};

export default authService;
