import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { PublicAdvertisement } from '../../types/admin/advertisementTypes';
import './AdSlot.scss';

const ROTATE_INTERVAL_MS = 8000;

interface AdSlotProps {
  /** Ads for one slot, already in display order. */
  ads: PublicAdvertisement[];
  className?: string;
}

/** Shows one ad at a time and rotates through the slot's ads when there are several. */
function AdSlot({ ads, className }: AdSlotProps) {
  const { t } = useTranslation();
  const [index, setIndex] = useState(0);

  useEffect(() => {
    setIndex(0);

    if (ads.length < 2) {
      return undefined;
    }

    const interval = window.setInterval(() => {
      setIndex((current) => (current + 1) % ads.length);
    }, ROTATE_INTERVAL_MS);

    return () => window.clearInterval(interval);
  }, [ads]);

  const ad = ads[index] ?? ads[0];

  if (!ad) {
    return null;
  }

  const image = (
    <img
      className="ad-slot__image"
      src={ad.imageUrl}
      alt={ad.altText ?? ad.name}
      loading="lazy"
    />
  );

  return (
    <aside
      className={className ? `ad-slot ${className}` : 'ad-slot'}
      aria-label={t('homePage.ads.label', 'Mainos')}
    >
      <span className="ad-slot__label">{t('homePage.ads.label', 'Mainos')}</span>
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
    </aside>
  );
}

export default AdSlot;
