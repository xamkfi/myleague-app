import type { ReactNode } from 'react';
import { useEffect } from 'react';
import AdminNavBar from '../Navigation/AdminNavBar';
import './AdminPageTemplate.scss';

interface AdminPageTemplateProps {
  title: string;
  children?: ReactNode;
}

function AdminPageTemplate({ title, children }: AdminPageTemplateProps) {
  useEffect(() => {
    document.title = `${title} - MAHL Admin`;
    return () => {
      document.title = 'MAHL';
    };
  }, [title]);

  return (
    <div className="admin-page-container">
      <AdminNavBar />
      <div className="admin-page-content">
        <div className="admin-page-body">
          {children || (
            <p className="placeholder-text">This admin page is under construction.</p>
          )}
        </div>
      </div>
    </div>
  );
}

export default AdminPageTemplate;

