import { useTranslation } from 'react-i18next';
import type { LicenceFilterValue } from './licenceFilterUtils';

interface PlayerLicenceFilterProps {
  id: string;
  value: LicenceFilterValue;
  onChange: (value: LicenceFilterValue) => void;
}

export default function PlayerLicenceFilter({ id, value, onChange }: PlayerLicenceFilterProps) {
  const { t } = useTranslation();

  return (
    <div className="players-licence-filter">
      <label htmlFor={id}>{t('playerLicence.filterLabel')}</label>
      <select
        id={id}
        value={value}
        onChange={(event) => onChange(event.target.value as LicenceFilterValue)}
      >
        <option value="all">{t('playerLicence.filterAll')}</option>
        <option value="active">{t('playerLicence.filterActive')}</option>
        <option value="inactive">{t('playerLicence.filterInactive')}</option>
      </select>
    </div>
  );
}
