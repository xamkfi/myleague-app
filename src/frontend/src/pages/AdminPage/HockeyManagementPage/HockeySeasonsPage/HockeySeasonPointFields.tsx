import { useTranslation } from 'react-i18next';
import type { HockeySeasonPointSettings } from '../../../../types/hockey/hockeyTypes';

interface PointField {
  key: keyof HockeySeasonPointSettings;
  labelKey: string;
  fallback: string;
}

const POINT_FIELDS: PointField[] = [
  { key: 'regulationWinPoints', labelKey: 'hockey.seasons.fields.regulationWinPoints', fallback: 'Regulation win' },
  { key: 'tiePoints', labelKey: 'hockey.seasons.fields.tiePoints', fallback: 'Draw' },
  { key: 'overtimeWinPoints', labelKey: 'hockey.seasons.fields.overtimeWinPoints', fallback: 'Overtime win' },
  { key: 'overtimeLossPoints', labelKey: 'hockey.seasons.fields.overtimeLossPoints', fallback: 'Overtime loss' },
  { key: 'shootoutWinPoints', labelKey: 'hockey.seasons.fields.shootoutWinPoints', fallback: 'Shootout win' },
  { key: 'shootoutLossPoints', labelKey: 'hockey.seasons.fields.shootoutLossPoints', fallback: 'Shootout loss' },
];

interface HockeySeasonPointFieldsProps {
  idPrefix: string;
  values: HockeySeasonPointSettings;
  disabled: boolean;
  onChange: (values: HockeySeasonPointSettings) => void;
}

function HockeySeasonPointFields({ idPrefix, values, disabled, onChange }: HockeySeasonPointFieldsProps) {
  const { t } = useTranslation();

  return (
    <div className="form-section">
      <h3 className="form-section__title">{t('hockey.seasons.sections.points', 'Points')}</h3>
      <div className="form-row">
        {POINT_FIELDS.map((field) => (
          <div className="form-group" key={field.key}>
            <label htmlFor={`${idPrefix}-${field.key}`}>{t(field.labelKey, field.fallback)}</label>
            <input
              type="number"
              id={`${idPrefix}-${field.key}`}
              min={0}
              disabled={disabled}
              value={values[field.key]}
              onChange={(event) => {
                const parsed = Number(event.target.value);
                onChange({
                  ...values,
                  [field.key]: Number.isFinite(parsed) && parsed >= 0 ? parsed : 0,
                });
              }}
            />
          </div>
        ))}
      </div>
    </div>
  );
}

export default HockeySeasonPointFields;
