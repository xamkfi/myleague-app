import { SeasonJsonImportModal } from '../../../../../components/admin/SeasonJsonImportModal/SeasonJsonImportModal';
import {
  getDryRunCounts,
  importSeason,
  inferTeamCategory,
  revertImport,
  validatePayload,
} from '../../../../../api/hockey/seasonImportService';
import type { HockeySeasonImportPayload } from '../../../../../types/hockey/seasonImportTypes';
import type { SeasonTeamCategory } from '../../../../../types/common/seasonImportTypes';
import { buildSeasonImportPreview } from '../../../../../api/common/seasonImportShared';
import { HOCKEY_SEASON_IMPORT_AI_PROMPT, buildHockeySeasonPromptFileName } from './seasonImportPrompt';

const SAMPLE_HREF = new URL('../../../../../types/hockey/seasonImport.sample.json', import.meta.url).href;

interface SeasonImportModalProps {
  onClose: () => void;
  onImported: () => void;
}

export function SeasonImportModal({ onClose, onImported }: SeasonImportModalProps) {
  return (
    <SeasonJsonImportModal<HockeySeasonImportPayload>
      onClose={onClose}
      onImported={onImported}
      i18nPrefix="hockey.seasons.import"
      prompt={HOCKEY_SEASON_IMPORT_AI_PROMPT}
      buildPromptFileName={buildHockeySeasonPromptFileName}
      sampleHref={SAMPLE_HREF}
      sampleDownloadName="hockey-season-import-sample.json"
      editPath={(seasonId) => `/admin/hockey/seasons/${seasonId}/edit`}
      validatePayload={validatePayload}
      getDryRunCounts={getDryRunCounts}
      getPreview={buildSeasonImportPreview}
      inferTeamCategory={inferTeamCategory}
      getSeasonName={(payload) => payload.season.name}
      getDefaultVenue={(payload) => payload.season.defaultVenue?.trim() ?? ''}
      applyOverrides={(payload, overrides) =>
        applySeasonOverrides(payload, overrides.name, overrides.venue, overrides.teamCategory)
      }
      importSeason={importSeason}
      revertImport={revertImport}
    />
  );
}

function applySeasonOverrides(
  payload: HockeySeasonImportPayload,
  name: string,
  venue: string,
  teamCategory: SeasonTeamCategory,
): HockeySeasonImportPayload {
  return {
    ...payload,
    season: {
      ...payload.season,
      name,
      defaultVenue: venue.length > 0 ? venue : null,
      teamCategory,
    },
  };
}

export default SeasonImportModal;
