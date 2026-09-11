import { useTranslation } from 'react-i18next';
import ConfirmationDialog from '../../../../components/ConfirmationDialog/ConfirmationDialog';
import { HOCKEY_MATCH_TYPES, type HockeyMatchType } from '../../../../types/hockey/hockeyTypes';
import '../../../../components/AdminMatchForm/AdminMatchForm.scss';

interface HockeyMatchFormOption {
  id: string;
  name: string;
}

export interface HockeyMatchFormValues {
  competitionId: string;
  homeTeamId: string;
  awayTeamId: string;
  date: string;
  hours: string;
  minutes: string;
  venue: string;
  matchType: HockeyMatchType;
}

interface HockeyMatchFormProps {
  mode: 'create' | 'edit';
  competitionKind: 'season' | 'tournament';
  values: HockeyMatchFormValues;
  competitions: HockeyMatchFormOption[];
  teams: HockeyMatchFormOption[];
  loading?: boolean;
  matchStatus?: string;
  showCancelConfirm?: boolean;
  showReactivateConfirm?: boolean;
  onChange: (values: HockeyMatchFormValues) => void;
  onSubmit: () => void;
  onCancel: () => void;
  onCancelMatch?: () => void;
  onReactivateMatch?: () => void;
  onCloseCancelConfirm?: () => void;
  onCloseReactivateConfirm?: () => void;
  onOpenCancelConfirm?: () => void;
  onOpenReactivateConfirm?: () => void;
}

function HockeyMatchForm({
  mode,
  competitionKind,
  values,
  competitions,
  teams,
  loading = false,
  matchStatus,
  showCancelConfirm = false,
  showReactivateConfirm = false,
  onChange,
  onSubmit,
  onCancel,
  onCancelMatch,
  onReactivateMatch,
  onCloseCancelConfirm,
  onCloseReactivateConfirm,
  onOpenCancelConfirm,
  onOpenReactivateConfirm,
}: HockeyMatchFormProps) {
  const { t } = useTranslation();
  const isTournament = competitionKind === 'tournament';

  const setField = <K extends keyof HockeyMatchFormValues>(field: K, value: HockeyMatchFormValues[K]): void => {
    onChange({ ...values, [field]: value });
  };

  const handleHoursChange = (value: string): void => {
    const numeric = Number(value);
    if (value === '' || (numeric >= 0 && numeric <= 23 && value.length <= 2)) {
      setField('hours', value);
    }
  };

  const handleMinutesChange = (value: string): void => {
    const numeric = Number(value);
    if (value === '' || (numeric >= 0 && numeric <= 59 && value.length <= 2)) {
      setField('minutes', value);
    }
  };

  return (
    <>
      <form
        className="modal-form"
        onSubmit={(event) => {
          event.preventDefault();
          onSubmit();
        }}
      >
        <div className="form-group create-match-form-row">
          <label htmlFor="hockey-competition">
            {isTournament
              ? t('hockey.matches.tournament', 'Tournament')
              : t('hockey.matches.season', 'Season')} *
          </label>
          <div className="input-wrapper">
            <select
              id="hockey-competition"
              value={values.competitionId}
              onChange={(event) => setField('competitionId', event.target.value)}
              required={mode === 'create'}
              disabled={mode === 'edit'}
            >
              <option value="">
                {isTournament
                  ? t('hockey.matches.selectTournament', 'Select tournament')
                  : t('hockey.matches.selectSeason', 'Select season')}
              </option>
              {competitions.map((item) => (
                <option key={item.id} value={item.id}>{item.name}</option>
              ))}
            </select>
          </div>
        </div>

        <div className="form-group create-match-form-row">
          <label htmlFor="hockey-home">{t('hockey.matches.homeTeam', 'Home team')}</label>
          <div className="input-wrapper">
            <select
              id="hockey-home"
              value={values.homeTeamId}
              onChange={(event) => setField('homeTeamId', event.target.value)}
            >
              <option value="">{t('hockey.matches.homeTeamPlaceholder', 'Select home team (optional)')}</option>
              {teams.map((team) => (
                <option key={team.id} value={team.id}>{team.name}</option>
              ))}
            </select>
          </div>
        </div>

        <div className="form-group create-match-form-row">
          <label htmlFor="hockey-away">{t('hockey.matches.awayTeam', 'Away team')}</label>
          <div className="input-wrapper">
            <select
              id="hockey-away"
              value={values.awayTeamId}
              onChange={(event) => setField('awayTeamId', event.target.value)}
            >
              <option value="">{t('hockey.matches.awayTeamPlaceholder', 'Select away team (optional)')}</option>
              {teams.map((team) => (
                <option key={team.id} value={team.id}>{team.name}</option>
              ))}
            </select>
          </div>
        </div>

        <div className="form-help-text">
          {t('hockey.matches.teamsOptionalHint', 'You can create a match without teams and assign them later.')}
        </div>

        <div className="form-group create-match-form-row">
          <label>{t('hockey.matches.dateTime', 'Date & Time')} *</label>
          <div className="input-wrapper">
            <div className="datetime-input-group">
              <div className="date-input">
                <input
                  type="date"
                  value={values.date}
                  onChange={(event) => setField('date', event.target.value)}
                  required
                />
              </div>
              <div className="time-input-group">
                <input
                  type="number"
                  placeholder="HH"
                  value={values.hours}
                  onChange={(event) => handleHoursChange(event.target.value)}
                  min={0}
                  max={23}
                  className="time-input hours"
                  required
                />
                <span className="time-separator">:</span>
                <input
                  type="number"
                  placeholder="MM"
                  value={values.minutes}
                  onChange={(event) => handleMinutesChange(event.target.value)}
                  min={0}
                  max={59}
                  className="time-input minutes"
                  required
                />
              </div>
            </div>
          </div>
        </div>

        {mode === 'create' && (
          <div className="form-group create-match-form-row">
            <label htmlFor="hockey-type">{t('hockey.matches.type', 'Match type')}</label>
            <div className="input-wrapper">
              <select
                id="hockey-type"
                value={values.matchType}
                onChange={(event) => setField('matchType', event.target.value as HockeyMatchType)}
              >
                {HOCKEY_MATCH_TYPES.map((item) => (
                  <option key={item} value={item}>{item}</option>
                ))}
              </select>
            </div>
          </div>
        )}

        <div className="form-group create-match-form-row">
          <label htmlFor="hockey-venue">{t('hockey.matches.venue', 'Venue')}</label>
          <div className="input-wrapper">
            <input
              id="hockey-venue"
              type="text"
              value={values.venue}
              onChange={(event) => setField('venue', event.target.value)}
            />
          </div>
        </div>

        <div className="form-actions">
          <button type="button" className="cancel-button" onClick={onCancel} disabled={loading}>
            {t('common.cancel', 'Cancel')}
          </button>
          {mode === 'edit' && matchStatus === 'Cancelled' && onOpenReactivateConfirm && (
            <button type="button" className="reactivate-match-button" onClick={onOpenReactivateConfirm} disabled={loading}>
              {t('hockey.matches.reactivate', 'Reactivate')}
            </button>
          )}
          {mode === 'edit' && matchStatus && matchStatus !== 'Cancelled' && matchStatus !== 'Finished' && matchStatus !== 'Forfeit' && onOpenCancelConfirm && (
            <button type="button" className="cancel-match-button" onClick={onOpenCancelConfirm} disabled={loading}>
              {t('hockey.matches.cancel', 'Cancel Match')}
            </button>
          )}
          <button type="submit" className="submit-button" disabled={loading}>
            {loading
              ? t('common.saving', 'Saving...')
              : mode === 'create'
                ? t('common.create', 'Create')
                : t('common.save', 'Save')}
          </button>
        </div>
      </form>

      <ConfirmationDialog
        isOpen={showCancelConfirm}
        icon="⚠️"
        title={t('hockey.matches.confirmCancelTitle', 'Cancel Match')}
        message={t('hockey.matches.confirmCancel', 'Cancel this match?')}
        confirmText={t('hockey.matches.confirmCancelButton', 'Yes, Cancel Match')}
        cancelText={t('common.cancel', 'Cancel')}
        isLoading={loading}
        onConfirm={() => onCancelMatch?.()}
        onCancel={() => onCloseCancelConfirm?.()}
      />
      <ConfirmationDialog
        isOpen={showReactivateConfirm}
        icon="♻️"
        title={t('hockey.matches.reactivate', 'Reactivate')}
        message={t('hockey.matches.confirmReactivate', 'Reactivate this match?')}
        confirmText={t('hockey.matches.reactivate', 'Reactivate')}
        cancelText={t('common.cancel', 'Cancel')}
        isLoading={loading}
        onConfirm={() => onReactivateMatch?.()}
        onCancel={() => onCloseReactivateConfirm?.()}
      />
    </>
  );
}

export default HockeyMatchForm;
