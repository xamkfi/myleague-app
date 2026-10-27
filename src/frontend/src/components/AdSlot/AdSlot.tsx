import { useTranslation } from 'react-i18next';
import type { PublicAdvertisement } from '../../types/admin/advertisementTypes';
import './AdSlot.scss';

interface AdSlotProps {
  /** Ads for one slot, already in display order. */
  ads: PublicAdvertisement[];
  className?: string;
}

/** Shows every ad of one slot stacked vertically, in the slot's display order. */
function AdSlot({ ads, className }: AdSlotProps) {
  const { t } = useTranslation();

  if (ads.length === 0) {
    return null;
  }

  return (
    <aside
      className={className ? `ad-slot ${className}` : 'ad-slot'}
      aria-label={t('homePage.ads.label', 'Mainos')}
    >
      <span className="ad-slot__label">{t('homePage.ads.label', 'Mainos')}</span>
      {ads.map((ad) => {
        const image = (
          <img
            className="ad-slot__image"
            src={ad.imageUrl}
            alt={ad.altText ?? ad.name}
            loading="lazy"
          />
        );

        return (
          <div className="ad-slot__item" key={ad.id}>
            {ad.linkUrl ? (
              <a
                className="ad-slot__link"
                href={ad.linkUrl}
                target="_blank"
                rel="noopener noreferrer sponsored"
              >
                {image}
              </a>
            ) : (
              image
            )}
          </div>
        );
      })}
    </aside>
  );
}

export default AdSlot;
