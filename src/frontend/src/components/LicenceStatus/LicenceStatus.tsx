import { useTranslation } from 'react-i18next';
import './LicenceStatus.scss';

interface LicenceStatusDotProps {
  /** Whether the player's licence is paid for this team. */
  active: boolean;
}

/**
 * Small dot on the corner of a jersey number. Place it inside an element with
 * `licence-anchor` so it sits on that element's top-right corner.
 */
export function LicenceStatusDot({ active }: LicenceStatusDotProps) {
  const { t } = useTranslation();
  const label: string = active ? t('playerLicence.statusOk') : t('playerLicence.statusMissing');
  return (
    <span
      className={`licence-dot licence-dot--${active ? 'ok' : 'missing'}`}
      role="img"
      aria-label={label}
      title={label}
    />
  );
}

/** Explains the dot colours. Render once above a roster or lineup. */
export function LicenceLegend() {
  const { t } = useTranslation();
  return (
    <div className="licence-legend" aria-hidden="true">
      <span className="licence-legend__item">
        <span className="licence-legend__dot licence-dot--ok" />
        {t('playerLicence.statusOk')}
      </span>
      <span className="licence-legend__item">
        <span className="licence-legend__dot licence-dot--missing" />
        {t('playerLicence.statusMissing')}
      </span>
    </div>
  );
}
