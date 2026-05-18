import type { ReactNode } from 'react';
import { useEffect, useState, useCallback } from 'react';
import AdminNavBar from '../Navigation/AdminNavBar';
import { InProgressMatchesProvider } from '../../hooks/InProgressMatchesProvider';
import './AdminPageTemplate.scss';

const SIDEBAR_COLLAPSED_KEY = 'admin-sidebar-collapsed';

interface AdminPageTemplateProps {
  title: string;
  children?: ReactNode;
}

function AdminPageTemplate({ title, children }: AdminPageTemplateProps) {
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

  return (
    <InProgressMatchesProvider>
      <div className={`admin-page-container ${sidebarCollapsed ? 'admin-page-container--collapsed' : ''}`}>
        <AdminNavBar collapsed={sidebarCollapsed} onToggleCollapse={handleToggleSidebar} />
        <div className="admin-page-content">
          <div className="admin-page-body">
            {children || (
              <p className="placeholder-text">This admin page is under construction.</p>
            )}
          </div>
        </div>
      </div>
    </InProgressMatchesProvider>
  );
}

export default AdminPageTemplate;

