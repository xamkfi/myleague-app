import { useTranslation } from 'react-i18next';
import type { FloorballSeasonDto } from '../../../../../api/floorball/floorballSeasonService';
import './ConfirmCompleteSeasonModal.scss';

interface ConfirmCompleteSeasonModalProps {
  /**
   * Kausi, jota ollaan merkitsemässä päättyneeksi.
   */
  season: FloorballSeasonDto;

  /**
   * Näytetään lataustila vahvistuspainikkeessa,
   * kun kauden päättämisoperaatio on käynnissä.
   */
  loading?: boolean;

  /**
   * Suorittaa varsinaisen complete-toiminnon.
   * Tämä kutsutaan vasta, kun käyttäjä vahvistaa modaalissa.
   */
  onConfirm: () => void | Promise<void>;

  /**
   * Sulkee modaalin ilman muutoksia.
   */
  onCancel: () => void;
}

export const ConfirmCompleteSeasonModal = ({
  season,
  loading = false,
  onConfirm,
  onCancel,
}: ConfirmCompleteSeasonModalProps) => {
  const { t } = useTranslation();

  const divisionCount = season.seasonDivisions?.length ?? 0;
  const teamCount = season.teams?.length ?? 0;

  return (
    <div
      className="confirm-complete-modal__backdrop"
      role="presentation"
      onMouseDown={(e) => {
        /**
         * Suljetaan modaali taustaa klikkaamalla vain silloin,
         * kun käyttäjä ei klikkaa itse modal-laatikkoa.
         * Latauksen aikana sulkeminen estetään.
         */
        if (e.target === e.currentTarget && !loading) {
          onCancel();
        }
      }}
    >
      <div
        className="confirm-complete-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirm-complete-season-title"
      >
        <div className="confirm-complete-modal__header">
          <div className="confirm-complete-modal__icon" aria-hidden="true">
            <i className="fas fa-flag-checkered"></i>
          </div>

          <div>
            <h2 id="confirm-complete-season-title">
              {t(
                'floorball.seasons.completeConfirm.title',
                'Päätä Kausi?'
              )}
            </h2>

            <p>
              {t(
                'floorball.seasons.completeConfirm.subtitle',
                'Oletko varma että haluat merkata kauden päättyneeksi.'
              )}
            </p>
          </div>
        </div>

        <div className="confirm-complete-modal__body">
          <div className="confirm-complete-modal__season">
            <span className="confirm-complete-modal__label">
              {t('floorball.seasons.fields.name', 'Name')}
            </span>
            <strong>{season.name}</strong>
          </div>

          <div className="confirm-complete-modal__details">
            <div>
              <span>{t('floorball.seasons.fields.division', 'Division')}</span>
              <strong>
                {t(
                  'floorball.seasons.completeConfirm.divisionCount',
                  '{{count}} division(s)',
                  { count: divisionCount }
                )}
              </strong>
            </div>

            <div>
              <span>{t('floorball.seasons.fields.teams', 'Teams')}</span>
              <strong>
                {t(
                  'floorball.seasons.completeConfirm.teamCount',
                  '{{count}} team(s)',
                  { count: teamCount }
                )}
              </strong>
            </div>
          </div>

          <div className="confirm-complete-modal__warning">
            <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
            <p>
              {t(
                'floorball.seasons.completeConfirm.warning',
                'Tämän toiminnon jälkeen kausi merkitään Päättyneeksi. Varmista, että kaikki ottelut ja kauden tiedot ovat oikein ennen jatkamista.'
              )}
            </p>
          </div>
        </div>

        <div className="confirm-complete-modal__actions">
          <button
            type="button"
            className="btn btn-secondary"
            onClick={onCancel}
            disabled={loading}
          >
            {t('common.cancel', 'Peruuta')}
          </button>

          <button
            type="button"
            className="btn btn-primary"
            onClick={onConfirm}
            disabled={loading}
          >
            {loading ? (
              <>
                <i className="fas fa-spinner fa-spin"></i>{' '}
                {t('common.saving', 'Saving...')}
              </>
            ) : (
              <>
                <i className="fas fa-check"></i>{' '}
                {t(
                  'floorball.seasons.completeConfirm.confirmButton',
                  'Päätä Kausi'
                )}
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
};