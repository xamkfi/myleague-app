import type { ReactNode } from 'react';
import { useEffect } from 'react';
import Navbar from '../Navigation/Navbar';
import Footer from '../Footer/Footer';
import './PageTemplate.scss';

interface PageTemplateProps {
  title: string;
  children?: ReactNode;
}

function PageTemplate({ title, children }: PageTemplateProps) {
  useEffect(() => {
    document.title = `${title} - MAHL`;
    return () => {
      document.title = 'MAHL';
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
      <Footer />
    </div>
  );
}

export default PageTemplate; 