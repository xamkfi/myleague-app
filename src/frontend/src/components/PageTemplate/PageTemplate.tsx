import type { ReactNode } from 'react';
import { useEffect } from 'react';
import Navbar from '../Navigation/Navbar';
import Footer from '../Footer/Footer';
import ScrollToTop from '../ScrollToTop/ScrollToTop';
import './PageTemplate.scss';

interface PageTemplateProps {
  title: string;
  children?: ReactNode;
  fullBleed?: boolean;
}

function PageTemplate({ title, children, fullBleed = false }: PageTemplateProps) {
  useEffect(() => {
    document.title = `${title} - MAHL`;
    return () => {
      document.title = 'MAHL';
    };
  }, [title]);

  return (
    <div
      className={
        fullBleed
          ? "page-container page-container--full-bleed"
          : "page-container"
      }
    >
      <ScrollToTop />
      <Navbar />
      <div
        className={
          fullBleed
            ? "page-content page-content--full-bleed"
            : "page-content"
        }
      >
        <div
          className={
            fullBleed ? "page-body page-body--full-bleed" : "page-body"
          }
        >
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