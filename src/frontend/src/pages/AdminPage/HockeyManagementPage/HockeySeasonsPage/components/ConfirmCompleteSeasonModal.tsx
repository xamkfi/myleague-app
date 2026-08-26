import { useTranslation } from 'react-i18next';
import type { HockeySeasonDto } from '../../../../../types/hockey/hockeyTypes';
import './ConfirmCompleteSeasonModal.scss';

interface ConfirmCompleteSeasonModalProps {
  season: HockeySeasonDto;
  loading?: boolean;
  onConfirm: () => void | Promise<void>;
  onCancel: () => void;
}

export function ConfirmCompleteSeasonModal({
  season,
  loading = false,
  onConfirm,
  onCancel,
}: ConfirmCompleteSeasonModalProps) {
  const { t } = useTranslation();
  const divisionCount = season.divisions?.length ?? 0;
  const teamCount = season.teams?.length ?? 0;

  return (
    <div
      className="confirm-complete-modal__backdrop"
      role="presentation"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget && !loading) {
          onCancel();
        }
      }}
    >
      <div
        className="confirm-complete-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirm-complete-hockey-season-title"
      >
        <div className="confirm-complete-modal__header">
          <div className="confirm-complete-modal__icon" aria-hidden="true">
            <i className="fas fa-flag-checkered"></i>
          </div>
          <div>
            <h2 id="confirm-complete-hockey-season-title">
              {t('hockey.seasons.completeConfirm.title', 'Complete Season?')}
            </h2>
            <p>
              {t('hockey.seasons.completeConfirm.subtitle', 'Are you sure you want to mark this season as completed?')}
            </p>
          </div>
        </div>
        <div className="confirm-complete-modal__body">
          <div className="confirm-complete-modal__season">
            <span className="confirm-complete-modal__label">{t('hockey.seasons.fields.name', 'Name')}</span>
            <strong>{season.name}</strong>
          </div>
          <div className="confirm-complete-modal__details">
            <div>
              <span>{t('hockey.seasons.fields.division', 'Division')}</span>
              <strong>
                {t('hockey.seasons.completeConfirm.divisionCount', '{{count}} division(s)', { count: divisionCount })}
              </strong>
            </div>
            <div>
              <span>{t('hockey.seasons.fields.teams', 'Teams')}</span>
              <strong>
                {t('hockey.seasons.completeConfirm.teamCount', '{{count}} team(s)', { count: teamCount })}
              </strong>
            </div>
          </div>
          <div className="confirm-complete-modal__warning">
            <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
            <p>
              {t(
                'hockey.seasons.completeConfirm.warning',
                'After this action the season is marked Completed. Make sure all matches and season data are correct before continuing.',
              )}
            </p>
          </div>
        </div>
        <div className="confirm-complete-modal__actions">
          <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={loading}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button type="button" className="btn btn-primary" onClick={() => void onConfirm()} disabled={loading}>
            {loading ? (
              <>
                <i className="fas fa-spinner fa-spin"></i> {t('common.saving', 'Saving...')}
              </>
            ) : (
              <>
                <i className="fas fa-check"></i> {t('hockey.seasons.completeConfirm.confirmButton', 'Complete Season')}
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
