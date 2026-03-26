import { Navigate, Outlet } from 'react-router-dom';
import type { UserRole } from '../types';
import { useAuth } from '../hooks/useAuth';

interface ProtectedRouteProps {
  roles?: UserRole[];
  permissions?: string[];
}

export function ProtectedRoute({ roles, permissions }: ProtectedRouteProps) {
  const { isAuthenticated, hasAnyRole, hasAnyPermission } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (roles && roles.length > 0 && !hasAnyRole(roles)) {
    return <Navigate to="/" replace />;
  }

  if (permissions && permissions.length > 0 && !hasAnyPermission(permissions)) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
