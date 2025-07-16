import React from 'react';
import { useSelector } from 'react-redux';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import type { RootState } from '../app/store';

interface ProtectedRouteProps {
  // Specifies the roles allowed to access the route.
  allowedRoles?: string[];
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ allowedRoles }) => {
  const { isAuthenticated, user, loading } = useSelector((state: RootState) => state.auth);
  const location = useLocation();

  // Display a loading message while authentication state is being determined.
  if (loading) {
    return <div className="text-center mt-20 text-xl text-gray-700">Loading authentication...</div>;
  }

  // If the user is not authenticated, redirect them to the login page.
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // If specific roles are required for the route and the user is authenticated,
  // check if the user has any of the allowed roles.
  if (allowedRoles && user) {
    const userRoles = user.roles || [];
    const hasRequiredRole = allowedRoles.some(role => userRoles.includes(role));

    // If the user does not have the required role, redirect them to the home page.
    if (!hasRequiredRole) {
      return <Navigate to="/" replace />;
    }
  }

  // If authenticated and authorized (or no roles specified), render the nested routes.
  return <Outlet />;
};

export default ProtectedRoute;