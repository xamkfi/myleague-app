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

type Translate = (key: string) => string;

function firstErrorString(errors: unknown): string | null {
  if (Array.isArray(errors)) {
    const first = errors.find((item): item is string => typeof item === 'string' && item.trim().length > 0);
    return first ?? null;
  }

  if (errors && typeof errors === 'object') {
    const values = Object.values(errors as Record<string, unknown>).flat();
    const first = values.find((item): item is string => typeof item === 'string' && item.trim().length > 0);
    return first ?? null;
  }

  return null;
}

function messageFromParsedBody(parsed: {
  title?: unknown;
  message?: unknown;
  errors?: unknown;
}): string | null {
  if (typeof parsed.title === 'string' && parsed.title.trim().length > 0) {
    return parsed.title;
  }
  if (typeof parsed.message === 'string' && parsed.message.trim().length > 0) {
    return parsed.message;
  }
  return firstErrorString(parsed.errors);
}

function extractApiErrorMessage(error: unknown): string {
  if (!(error instanceof Error) || !error.message) {
    return '';
  }

  const raw = error.message.replace(/^Error:\s*/, '').replace(/^HTTP\s+\d+:\s*/, '');
  try {
    const parsed = JSON.parse(raw) as { title?: unknown; message?: unknown; errors?: unknown };
    return messageFromParsedBody(parsed) ?? raw;
  } catch {
    const start = raw.indexOf('{');
    const end = raw.lastIndexOf('}');
    if (start !== -1 && end > start) {
      try {
        const parsed = JSON.parse(raw.slice(start, end + 1)) as {
          title?: unknown;
          message?: unknown;
          errors?: unknown;
        };
        return messageFromParsedBody(parsed) ?? raw;
      } catch {
        // Fall through to the raw sentence or substring match.
      }
    }
  }

  return raw;
}

function matchKnownDeletionReason(text: string): string | null {
  const exact = DELETION_REASON_KEYS[text];
  if (exact) {
    return exact;
  }

  for (const [reason, key] of Object.entries(DELETION_REASON_KEYS)) {
    if (text.includes(reason)) {
      return key;
    }
  }

  return null;
}

export function mapDeletionError(error: unknown, t: Translate): string | null {
  const message = extractApiErrorMessage(error);
  if (!message) {
    return null;
  }

  const key = matchKnownDeletionReason(message);
  if (key) {
    return t(key);
  }

  const raw = error instanceof Error ? error.message : '';
  const nestedKey = raw ? matchKnownDeletionReason(raw) : null;
  if (nestedKey) {
    return t(nestedKey);
  }

  return message;
}
