import React from 'react';
import type { ReactNode } from 'react';
import Navbar from '../Navigation/Navbar';
import './PageTemplate.scss';

interface PageTemplateProps {
  title: string;
  children?: ReactNode;
}

function PageTemplate({ title, children }: PageTemplateProps) {
  return (
    <div className="page-container">
      <Navbar />
      <div className="page-content">
        <h1 className="page-title">{title}</h1>
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