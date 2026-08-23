const DELETION_REASON_KEYS: Record<string, string> = {
  'Cannot delete this person because they have match or statistics records.':
    'deletion.personHasMatchRecords',
  'Cannot delete this person because they have a user account.':
    'deletion.personHasUserAccount',
  'Cannot delete this person because they are a club administrator.':
    'deletion.personIsClubManager',
  'Cannot delete this person because they are assigned as an official on a match.':
    'deletion.personIsAssignedOfficial',
  'Cannot delete this player because they have played in a match or have statistics.':
    'deletion.playerHasHistory',
  'Cannot delete this team because it is used in matches.':
    'deletion.teamUsedInMatches',
  'Cannot delete this club because it still has teams.':
    'deletion.clubHasTeams',
  'Cannot delete this division because teams still use it. Deactivate it instead.':
    'deletion.divisionHasTeams',
  'Cannot delete this referee because they are assigned to a match.':
    'deletion.refereeAssignedToMatch',
  'Cannot delete the last system administrator.':
    'deletion.lastSystemAdmin',
  'You cannot delete your own user account.':
    'deletion.cannotDeleteOwnAccount',
};

type Translate = (key: string, defaultValue?: string) => string;

function extractApiErrorMessage(error: unknown): string {
  if (!(error instanceof Error) || !error.message) {
    return '';
  }

  const raw = error.message.replace(/^Error:\s*/, '');
  try {
    const parsed = JSON.parse(raw) as { title?: string; message?: string };
    if (parsed && typeof parsed.title === 'string' && parsed.title.length > 0) {
      return parsed.title;
    }
    if (parsed && typeof parsed.message === 'string' && parsed.message.length > 0) {
      return parsed.message;
    }
  } catch {
    // The API client sometimes throws a plain sentence instead of JSON.
  }

  return raw;
}

export function mapDeletionError(error: unknown, t: Translate): string | null {
  const message = extractApiErrorMessage(error);
  if (!message) {
    return null;
  }

  const key = DELETION_REASON_KEYS[message];
  if (key) {
    return t(key, message);
  }

  return message;
}
