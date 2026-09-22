import type { ReactNode } from 'react';
import bannerImage from '../../assets/floorball-banner.png';
import './CatalogPage.scss';

interface CatalogPageProps {
  title: string;
  description: string;
  children: ReactNode;
  bannerExtra?: ReactNode;
}

function CatalogPage({ title, description, children, bannerExtra }: CatalogPageProps) {
  return (
    <div className="catalog-page">
      <header className="catalog-page__banner">
        <img className="catalog-page__banner-image" src={bannerImage} alt="" aria-hidden="true" />
        <div className="catalog-page__banner-content">
          <h1 className="catalog-page__title">{title}</h1>
          <p className="catalog-page__description">{description}</p>
          {bannerExtra}
        </div>
      </header>
      <div className="catalog-page__content">{children}</div>
    </div>
  );
}

export default CatalogPage;
