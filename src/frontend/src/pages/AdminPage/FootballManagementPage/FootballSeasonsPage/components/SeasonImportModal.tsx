import { SeasonJsonImportModal } from '../../../../../components/admin/SeasonJsonImportModal/SeasonJsonImportModal';
import {
  getDryRunCounts,
  importSeason,
  inferTeamCategory,
  revertImport,
  validatePayload,
} from '../../../../../api/football/seasonImportService';
import type { FootballSeasonImportPayload } from '../../../../../types/football/seasonImportTypes';
import type { SeasonTeamCategory } from '../../../../../types/common/seasonImportTypes';
import { FOOTBALL_SEASON_IMPORT_AI_PROMPT, buildFootballSeasonPromptFileName } from './seasonImportPrompt';

const SAMPLE_HREF = new URL('../../../../../types/football/seasonImport.sample.json', import.meta.url).href;

interface SeasonImportModalProps {
  onClose: () => void;
  onImported: () => void;
}

export function SeasonImportModal({ onClose, onImported }: SeasonImportModalProps) {
  return (
    <SeasonJsonImportModal<FootballSeasonImportPayload>
      onClose={onClose}
      onImported={onImported}
      i18nPrefix="football.seasons.import"
      prompt={FOOTBALL_SEASON_IMPORT_AI_PROMPT}
      buildPromptFileName={buildFootballSeasonPromptFileName}
      sampleHref={SAMPLE_HREF}
      sampleDownloadName="football-season-import-sample.json"
      editPath={(seasonId) => `/admin/football/seasons/${seasonId}/edit`}
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
  payload: FootballSeasonImportPayload,
  name: string,
  venue: string,
  teamCategory: SeasonTeamCategory,
): FootballSeasonImportPayload {
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
