import type { ReactNode } from 'react';
import { useEffect } from 'react';
import Navbar from '../Navigation/Navbar';
import Footer from '../Footer/Footer';
import ScrollToTop from '../ScrollToTop/ScrollToTop';
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
      <ScrollToTop />
      <Navbar />
      <div className="page-content" style={{ paddingLeft: '0px', paddingRight: '0px' }}>
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