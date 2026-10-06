import {
  hockeyLiveStateChanged,
  isHockeyMatchFinished,
  type HockeyLiveMatchDto,
  type HockeyMatchDto,
} from '../types/hockey/hockeyTypes';

export interface HockeyLiveMergeResult {
  matches: HockeyMatchDto[];
  changed: boolean;
  /** A match finished since the last load, so period scores and standings need a full reload. */
  needsFullReload: boolean;
}

/** Applies live score and status rows onto a loaded match list without touching other fields. */
export function mergeHockeyLiveMatches(
  matches: HockeyMatchDto[],
  liveRows: HockeyLiveMatchDto[],
): HockeyLiveMergeResult {
  const liveById = new Map(liveRows.map((row) => [row.id, row]));
  let changed = false;
  let needsFullReload = false;

  const merged = matches.map((match) => {
    const live = liveById.get(match.id);
    if (!live || !hockeyLiveStateChanged(match, live)) {
      return match;
    }
    changed = true;
    if (isHockeyMatchFinished(live.status) && !isHockeyMatchFinished(match.status)) {
      needsFullReload = true;
    }
    return { ...match, ...live };
  });

  return { matches: changed ? merged : matches, changed, needsFullReload };
}
