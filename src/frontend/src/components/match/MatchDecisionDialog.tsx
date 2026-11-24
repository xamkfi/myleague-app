import { useRef, useState, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { useDialogBehavior } from '../../hooks/useDialogBehavior';
import type { MatchDecision } from '../../types/common/matchDecision';
import SegmentedControl, { type SegmentedOption } from '../SegmentedControl/SegmentedControl';
import './BulkSaveDialog.scss';

interface MatchDecisionDialogProps {
  current: MatchDecision;
  /** False when the match rules have no overtime / extra time. */
  allowOvertime: boolean;
  /** False when the match rules have no shootout. */
  allowShootout: boolean;
  /** Rejects with the backend's error when the decision could not be changed. */
  onSubmit: (decision: MatchDecision) => Promise<void>;
  onClose: () => void;
}

/** Translates the backend's refusals, which are in English. */
function decisionErrorKey(error: unknown): string {
  const message: string = error instanceof Error ? error.message : '';
  if (message.includes('must be level')) return 'matchManage.decision.errors.notLevel';
  if (message.includes('later periods')) return 'matchManage.decision.errors.laterEvents';
  if (message.includes('not allowed by the match rules')) return 'matchManage.decision.errors.notInRules';
  return 'matchManage.decision.errors.failed';
}

/**
 * Corrects the phase of play that decided the match, for example when the shootout was never started
 * on the desk. Shared by the floorball, football and hockey live desks.
 */
function MatchDecisionDialog({
  current,
  allowOvertime,
  allowShootout,
  onSubmit,
  onClose,
}: MatchDecisionDialogProps): ReactElement {
  const { t } = useTranslation();
  const dialogRef = useRef<HTMLDivElement | null>(null);
  const [decision, setDecision] = useState<MatchDecision>(current);
  const [saving, setSaving] = useState<boolean>(false);
  const [errorKey, setErrorKey] = useState<string | null>(null);

  useDialogBehavior(true, dialogRef, onClose);

  const options: SegmentedOption<MatchDecision>[] = [
    { value: 'Regulation', label: t('matchManage.decision.regulation') },
  ];
  if (allowOvertime || current === 'Overtime') {
    options.push({ value: 'Overtime', label: t('matchManage.decision.overtime') });
  }
  if (allowShootout || current === 'Shootout') {
    options.push({ value: 'Shootout', label: t('matchManage.decision.shootout') });
  }

  const handleSubmit = async (): Promise<void> => {
    setSaving(true);
    setErrorKey(null);
    try {
      await onSubmit(decision);
      onClose();
    } catch (error) {
      setErrorKey(decisionErrorKey(error));
      setSaving(false);
    }
  };

  return (
    <div className="bulk-save-modal-overlay" onClick={onClose} role="presentation">
      <div
        ref={dialogRef}
        tabIndex={-1}
        className="bulk-save-modal"
        onClick={(event) => event.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="match-decision-title"
      >
        <div className="bulk-save-modal__header">
          <div>
            <h3 id="match-decision-title">{t('matchManage.decision.title')}</h3>
            <p className="bulk-save-modal__subtitle">{t('matchManage.decision.hint')}</p>
          </div>
          <button
            className="bulk-save-modal__close"
            onClick={onClose}
            disabled={saving}
            type="button"
            aria-label={t('common.close')}
          >
            ×
          </button>
        </div>

        <div className="bulk-save-modal__body">
          <SegmentedControl
            options={options}
            value={decision}
            onChange={setDecision}
            ariaLabel={t('matchManage.decision.title')}
            layout="stretch"
          />
          {errorKey && (
            <div className="field-error" role="alert">
              {t(errorKey)}
            </div>
          )}
        </div>

        <div className="bulk-save-modal__footer">
          <button onClick={onClose} className="cancel-btn" type="button" disabled={saving}>
            {t('common.cancel')}
          </button>
          <button
            onClick={() => void handleSubmit()}
            className="submit-btn"
            type="button"
            disabled={saving || decision === current}
          >
            {saving ? t('common.saving') : t('common.save')}
          </button>
        </div>
      </div>
    </div>
  );
}

export default MatchDecisionDialog;
