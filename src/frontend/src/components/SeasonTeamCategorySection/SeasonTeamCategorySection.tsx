import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import TeamCategoryPicker from '../TeamCategoryPicker/TeamCategoryPicker';
import ConfirmationDialog from '../ConfirmationDialog/ConfirmationDialog';
import { getAudienceByTeamCategory } from '../../audience/audienceRegistry';
import { changeSeasonTeamCategory } from '../../api/common/seasonTeamCategoryService';
import type { SportKind } from '../../utils/sportRoutes';
import type { TeamCategory } from '../../types/floorball/floorballTypes';

interface SeasonTeamCategorySectionProps {
  sport: SportKind;
  seasonId: string;
  /** Current category from the loaded season. */
  category: string;
  /** Called after the change is saved, so the page can update its season state. */
  onChanged: (category: TeamCategory) => void;
}

/**
 * Lets an admin move a season to another audience group. Saves on its own after a confirmation,
 * so it also works for completed seasons whose other details are locked.
 */
function SeasonTeamCategorySection({ sport, seasonId, category, onChanged }: SeasonTeamCategorySectionProps) {
  const { t } = useTranslation();
  const [pending, setPending] = useState<TeamCategory | null>(null);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [saved, setSaved] = useState<boolean>(false);

  const groupName = (value: string): string => {
    const audience = getAudienceByTeamCategory(value);
    return audience ? t(audience.i18nKey) : value;
  };

  const confirm = async (): Promise<void> => {
    if (!pending) {
      return;
    }
    setSaving(true);
    setError(null);
    try {
      await changeSeasonTeamCategory(sport, seasonId, pending);
      onChanged(pending);
      setSaved(true);
      window.setTimeout(() => setSaved(false), 2500);
    } catch (err) {
      setError(err instanceof Error ? err.message : t('seasonGroup.changeFailed'));
    } finally {
      setSaving(false);
      setPending(null);
    }
  };

  return (
    <div className="form-section season-team-category">
      <TeamCategoryPicker
        name={`season-group-${seasonId}`}
        value={category}
        onChange={(next) => {
          if (next !== category) {
            setSaved(false);
            setPending(next);
          }
        }}
        hint={t('seasonGroup.editHint')}
        disabled={saving}
      />
      {saved && (
        <p className="season-team-category__saved" role="status">
          {t('seasonGroup.changed', { group: groupName(category) })}
        </p>
      )}
      {error && <p className="season-team-category__error" role="alert">{error}</p>}
      <ConfirmationDialog
        isOpen={pending !== null}
        icon="🔀"
        title={t('seasonGroup.confirmTitle')}
        message={t('seasonGroup.confirmMessage', { from: groupName(category), to: groupName(pending ?? category) })}
        confirmText={t('seasonGroup.confirm')}
        cancelText={t('common.cancel')}
        isLoading={saving}
        onConfirm={() => void confirm()}
        onCancel={() => setPending(null)}
      />
    </div>
  );
}

export default SeasonTeamCategorySection;
