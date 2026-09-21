import { SeasonJsonImportModal } from '../../../../../components/admin/SeasonJsonImportModal/SeasonJsonImportModal';
import {
  getDryRunCounts,
  importSeason,
  inferTeamCategory,
  revertImport,
  validatePayload,
} from '../../../../../api/floorball/seasonImportService';
import type { FloorballSeasonImportPayload } from '../../../../../types/floorball/seasonImportTypes';
import type { SeasonTeamCategory } from '../../../../../types/common/seasonImportTypes';
import { FLOORBALL_SEASON_IMPORT_AI_PROMPT, buildFloorballSeasonPromptFileName } from './seasonImportPrompt';

const SAMPLE_HREF = new URL('../../../../../types/floorball/seasonImport.sample.json', import.meta.url).href;

interface SeasonImportModalProps {
  onClose: () => void;
  onImported: () => void;
}

export function SeasonImportModal({ onClose, onImported }: SeasonImportModalProps) {
  return (
    <SeasonJsonImportModal<FloorballSeasonImportPayload>
      onClose={onClose}
      onImported={onImported}
      i18nPrefix="floorball.seasons.import"
      prompt={FLOORBALL_SEASON_IMPORT_AI_PROMPT}
      buildPromptFileName={buildFloorballSeasonPromptFileName}
      sampleHref={SAMPLE_HREF}
      sampleDownloadName="floorball-season-import-sample.json"
      editPath={(seasonId) => `/admin/floorball/seasons/${seasonId}/edit`}
      validatePayload={validatePayload}
      getDryRunCounts={getDryRunCounts}
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
  payload: FloorballSeasonImportPayload,
  name: string,
  venue: string,
  teamCategory: SeasonTeamCategory,
): FloorballSeasonImportPayload {
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
