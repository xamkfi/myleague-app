import type { ReactNode } from 'react';
import { useEffect } from 'react';
import Navbar from '../Navigation/Navbar';
import './PageTemplate.scss';

interface PageTemplateProps {
  title: string;
  children?: ReactNode;
}

function PageTemplate({ title, children }: PageTemplateProps) {
  // Set the document title (browser tab title)
  useEffect(() => {
    document.title = `${title} - MAHL`;
    return () => {
      document.title = 'MAHL'; // Reset to default on unmount
    };
  }, [title]);

  return (
    <div className="page-container">
      <Navbar />
      <div className="page-content">
        <div className="page-body">
          {children || (
            <p className="placeholder-text">This page is under construction.</p>
          )}
        </div>
      </div>
    </div>
  );
}

export default PageTemplate; 