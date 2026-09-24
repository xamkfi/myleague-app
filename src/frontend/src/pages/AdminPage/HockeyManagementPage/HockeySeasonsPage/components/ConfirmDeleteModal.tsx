import { useTranslation } from 'react-i18next';
import type { HockeySeasonDto } from '../../../../../types/hockey/hockeyTypes';
import './ConfirmCompleteSeasonModal.scss';

interface ConfirmDeleteModalProps {
  season: HockeySeasonDto;
  loading?: boolean;
  onConfirm: () => void | Promise<void>;
  onCancel: () => void;
}

export function ConfirmDeleteModal({
  season,
  loading = false,
  onConfirm,
  onCancel,
}: ConfirmDeleteModalProps) {
  const { t } = useTranslation();
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
        aria-labelledby="confirm-delete-hockey-season-title"
      >
        <div className="confirm-complete-modal__header">
          <div className="confirm-complete-modal__icon" aria-hidden="true">
            <i className="fas fa-exclamation-triangle"></i>
          </div>
          <div>
            <h2 id="confirm-delete-hockey-season-title">
              {t('hockey.seasons.deleteConfirm.title', 'Delete season?')}
            </h2>
            <p>
              {t(
                'hockey.seasons.deleteConfirm.message',
                'Unplayed matches are removed with the season. Club teams stay.',
              )}
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
              <span>{t('hockey.seasons.fields.teams', 'Teams')}</span>
              <strong>
                {t('hockey.seasons.completeConfirm.teamCount', '{{count}} team(s)', { count: teamCount })}
              </strong>
            </div>
          </div>
          <div className="confirm-complete-modal__warning">
            <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
            <p>{t('hockey.seasons.deleteConfirm.warning', 'This action cannot be undone.')}</p>
          </div>
        </div>
        <div className="confirm-complete-modal__actions">
          <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={loading}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button type="button" className="btn btn-danger" onClick={() => void onConfirm()} disabled={loading}>
            {loading ? (
              <>
                <i className="fas fa-spinner fa-spin"></i> {t('common.saving', 'Saving...')}
              </>
            ) : (
              t('common.delete', 'Delete')
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
