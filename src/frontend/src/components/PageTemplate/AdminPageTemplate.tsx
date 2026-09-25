import type { ReactNode } from 'react';
import { useEffect, useState, useCallback } from 'react';
import { useLocation } from 'react-router-dom';
import AdminNavBar from '../Navigation/AdminNavBar';
import AdminBackButton from '../AdminBackButton/AdminBackButton';
import { rememberAdminLocation } from '../../utils/adminReturnTo';
import { InProgressMatchesProvider } from '../../hooks/InProgressMatchesProvider';
import { InProgressFootballMatchesProvider } from '../../hooks/InProgressFootballMatchesProvider';
import { InProgressHockeyMatchesProvider } from '../../hooks/InProgressHockeyMatchesProvider';
import type { SportKind } from '../../utils/sportRoutes';
import './AdminPageTemplate.scss';

function contentSport(pathname: string): SportKind | null {
  if (pathname.startsWith('/admin/floorball')) {
    return 'floorball';
  }
  if (pathname.startsWith('/admin/football')) {
    return 'football';
  }
  if (pathname.startsWith('/admin/hockey')) {
    return 'hockey';
  }
  return null;
}

const SIDEBAR_COLLAPSED_KEY = 'admin-sidebar-collapsed';

interface AdminPageTemplateProps {
  title: string;
  children?: ReactNode;
}

function AdminPageTemplate({ title, children }: AdminPageTemplateProps) {
  const location = useLocation();
  const sport = contentSport(location.pathname);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(() => {
    try {
      return localStorage.getItem(SIDEBAR_COLLAPSED_KEY) === 'true';
    } catch {
      return false;
    }
  });

  const handleToggleSidebar = useCallback(() => {
    setSidebarCollapsed(prev => {
      const next = !prev;
      try { localStorage.setItem(SIDEBAR_COLLAPSED_KEY, String(next)); } catch { /* noop */ }
      return next;
    });
  }, []);

  useEffect(() => {
    document.title = `${title} - MAHL Admin`;
    return () => {
      document.title = 'MAHL';
    };
  }, [title]);

  useEffect(() => {
    rememberAdminLocation(`${location.pathname}${location.search}`);
  }, [location.pathname, location.search]);

  return (
    <InProgressMatchesProvider>
      <InProgressFootballMatchesProvider>
        <InProgressHockeyMatchesProvider>
          <div className={`admin-page-container ${sidebarCollapsed ? 'admin-page-container--collapsed' : ''}`}>
            <AdminNavBar collapsed={sidebarCollapsed} onToggleCollapse={handleToggleSidebar} />
            <div className="admin-page-content">
              <div className="admin-page-body" data-sport={sport ?? undefined}>
                <AdminBackButton />
                {children || (
                  <p className="placeholder-text">This admin page is under construction.</p>
                )}
              </div>
            </div>
          </div>
        </InProgressHockeyMatchesProvider>
      </InProgressFootballMatchesProvider>
    </InProgressMatchesProvider>
  );
}

export default AdminPageTemplate;

