import { Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import type { AuthUserRole } from '../../types/auth/authTypes';

interface ProtectedRouteProps {
  children: React.ReactNode;
  /**
   * Roles allowed to view the route. Defaults to admin roles so all existing
   * admin routes stay admin-only. Team leader routes must pass roles explicitly.
   */
  allowedRoles?: AuthUserRole[];
  /** Login page to redirect unauthenticated users to. */
  loginPath?: string;
}

const DEFAULT_ALLOWED_ROLES: AuthUserRole[] = ['ClubAdmin', 'SystemAdmin'];

function ProtectedRoute({
  children,
  allowedRoles = DEFAULT_ALLOWED_ROLES,
  loginPath = '/admin/login',
}: ProtectedRouteProps) {
  const { isAuthenticated, isLoading, user } = useAuth();

  if (isLoading) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '100vh',
        fontFamily: 'Inter, system-ui, sans-serif',
        color: '#6E6D75',
      }}>
        Loading...
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to={loginPath} replace />;
  }

  if (user && !allowedRoles.includes(user.role)) {
    // Authenticated but not allowed here: send the user to their own home area.
    const home = user.role === 'TeamLeader' ? '/team-leader' : '/admin';
    return <Navigate to={home} replace />;
  }

  return <>{children}</>;
}

export default ProtectedRoute;
