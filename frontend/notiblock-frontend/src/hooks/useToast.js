import { useState, useCallback } from 'react';

/**
 * Custom hook for managing toast notifications
 * @returns {Object} Toast utilities
 */
export const useToast = () => {
    const [toast, setToast] = useState({
        show: false,
        message: '',
        type: 'info', // 'success', 'error', 'warning', 'info'
    });

    const showToast = useCallback((message, type = 'info') => {
        setToast({ show: true, message, type });

        // Auto-hide after 5 seconds
        setTimeout(() => {
            setToast({ show: false, message: '', type: 'info' });
        }, 5000);
    }, []);

    const hideToast = useCallback(() => {
        setToast({ show: false, message: '', type: 'info' });
    }, []);

    const success = useCallback((message) => showToast(message, 'success'), [showToast]);
    const error = useCallback((message) => showToast(message, 'error'), [showToast]);
    const warning = useCallback((message) => showToast(message, 'warning'), [showToast]);
    const info = useCallback((message) => showToast(message, 'info'), [showToast]);

    return {
        toast,
        showToast,
        hideToast,
        success,
        error,
        warning,
        info,
    };
};
