import { useEffect, useState, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';

export type LifecycleModalVariant = 'default' | 'destructive';

export interface LifecyclePrerequisite {
  label: string;
  met: boolean;
}

interface TournamentLifecycleConfirmModalProps {
  isOpen: boolean;
  variant: LifecycleModalVariant;
  title: string;
  description: string;
  prerequisites: ReadonlyArray<LifecyclePrerequisite>;
  confirmLabel: string;
  destructiveAcknowledgeLabel?: string;
  loading: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export const TournamentLifecycleConfirmModal = ({
  isOpen,
  variant,
  title,
  description,
  prerequisites,
  confirmLabel,
  destructiveAcknowledgeLabel,
  loading,
  onConfirm,
  onCancel,
}: TournamentLifecycleConfirmModalProps): ReactElement | null => {
  const { t } = useTranslation();
  const [acknowledged, setAcknowledged] = useState<boolean>(false);

  useEffect(() => {
    if (!isOpen) {
      setAcknowledged(false);
    }
  }, [isOpen]);

  if (!isOpen) {
    return null;
  }

  const isDestructive: boolean = variant === 'destructive';
  const confirmDisabled: boolean =
    loading || (isDestructive && !acknowledged);

  return (
    <div
      className="tlb-modal-overlay"
      role="dialog"
      aria-modal="true"
      aria-labelledby="tlb-modal-title"
    >
      <div
        className={`tlb-modal-content${
          isDestructive ? ' tlb-modal-content--destructive' : ''
        }`}
      >
        <div className="tlb-modal-header">
          <h3 id="tlb-modal-title">{title}</h3>
          <button
            type="button"
            className="tlb-modal-close"
            onClick={onCancel}
            aria-label={t('common.close', 'Sulje')}
            disabled={loading}
          >
            ×
          </button>
        </div>

        <div className="tlb-modal-body">
          <div className="tlb-modal-icon">
            <i
              className={
                isDestructive
                  ? 'fas fa-exclamation-triangle'
                  : 'fas fa-info-circle'
              }
              aria-hidden="true"
            ></i>
          </div>

          <p className="tlb-modal-description">{description}</p>

          {prerequisites.length > 0 && (
            <ul className="tlb-modal-prereqs">
              {prerequisites.map((prereq, idx) => (
                <li
                  key={`${idx}-${prereq.label}`}
                  className={`tlb-modal-prereq${
                    prereq.met
                      ? ' tlb-modal-prereq--met'
                      : ' tlb-modal-prereq--unmet'
                  }`}
                >
                  <i
                    className={
                      prereq.met
                        ? 'fas fa-check-circle'
                        : 'fas fa-times-circle'
                    }
                    aria-hidden="true"
                  ></i>
                  <span>{prereq.label}</span>
                </li>
              ))}
            </ul>
          )}

          {isDestructive && destructiveAcknowledgeLabel && (
            <label className="tlb-modal-ack">
              <input
                type="checkbox"
                checked={acknowledged}
                onChange={(e): void => setAcknowledged(e.target.checked)}
                disabled={loading}
              />
              <span>{destructiveAcknowledgeLabel}</span>
            </label>
          )}
        </div>

        <div className="tlb-modal-footer">
          <button
            type="button"
            className="tlb-modal-btn tlb-modal-btn--secondary"
            onClick={onCancel}
            disabled={loading}
          >
            {t('common.cancel', 'Peruuta')}
          </button>
          <button
            type="button"
            className={`tlb-modal-btn ${
              isDestructive
                ? 'tlb-modal-btn--danger'
                : 'tlb-modal-btn--primary'
            }`}
            onClick={onConfirm}
            disabled={confirmDisabled}
          >
            {loading ? (
              <>
                <i className="fas fa-spinner fa-spin" aria-hidden="true"></i>{' '}
                {t('common.processing', 'Käsitellään...')}
              </>
            ) : (
              confirmLabel
            )}
          </button>
        </div>
      </div>
    </div>
  );
};

export default TournamentLifecycleConfirmModal;
