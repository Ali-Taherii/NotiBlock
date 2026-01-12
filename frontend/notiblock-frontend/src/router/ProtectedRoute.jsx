import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { useEffect, useState } from 'react';

/**
 * Protected Route Component
 * Prevents unauthorized access to protected pages
 * 
 * @param {Object} props
 * @param {React.ReactNode} props.children - Content to render if authorized
 * @param {string} [props.role] - Required user role (optional - if not provided, any authenticated user can access)
 * @returns {React.ReactNode}
 */
export const ProtectedRoute = ({ children, role }) => {
  const { user, loading } = useAuth();
  const location = useLocation();
  const [authChecked, setAuthChecked] = useState(false);

  // Re-check auth whenever user state or location changes
  useEffect(() => {
    console.log('ProtectedRoute check:', { 
      user: user?.email, 
      userRole: user?.role, 
      requiredRole: role, 
      loading,
      path: location.pathname 
    });
    setAuthChecked(true);
  }, [user, loading, role, location.pathname]);

  // Show loading state while checking authentication
  if (loading || !authChecked) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mb-4"></div>
          <p className="text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  // Redirect to login if not authenticated
  if (!user) {
    console.log('ProtectedRoute: No user, redirecting to /');
    // Save the attempted location for redirect after login
    return <Navigate to="/" state={{ from: location }} replace />;
  }

  // Check role-based access if role is specified
  if (role && user.role !== role) {
    console.log('ProtectedRoute: Role mismatch', { expected: role, actual: user.role });
    // User is authenticated but doesn't have required role
    return <Navigate to="/not-found" replace />;
  }

  console.log('ProtectedRoute: Access granted');
  // User is authenticated and has required role (or no role required)
  return children;
};

export default ProtectedRoute;
