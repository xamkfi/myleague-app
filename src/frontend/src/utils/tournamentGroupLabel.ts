/**
 * Builds a user-facing group label without duplicating the localized group word
 * (e.g. "Lohko" + "Lohko A" → "Lohko A", not "Lohko Lohko A").
 */
export function formatTournamentGroupLabel(rawName: string, groupWord: string): string {
  const name = rawName.trim();
  const word = groupWord.trim();
  if (!name) {
    return word || name;
  }
  if (!word) {
    return name;
  }

  const nameLower = name.toLowerCase();
  const wordLower = word.toLowerCase();

  if (nameLower.startsWith(wordLower)) {
    return name;
  }

  if (/\blohko\b/i.test(name) || /\bgroup\b/i.test(name)) {
    return name;
  }

  if (/^[A-Za-z0-9]{1,2}$/.test(name)) {
    return `${word} ${name}`;
  }

  return name;
}

/**
 * Short label for group switcher tabs (e.g. "A", "B") when the stored name is a single letter.
 */
export function formatTournamentGroupTabLabel(rawName: string, groupWord: string): string {
  const name = rawName.trim();
  if (/^[A-Za-z0-9]{1,2}$/.test(name)) {
    return name.toUpperCase();
  }

  const full = formatTournamentGroupLabel(name, groupWord);
  const escapedWord = groupWord.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const shortMatch = full.match(new RegExp(`^${escapedWord}\\s+([A-Za-z0-9]{1,2})$`, 'i'));
  if (shortMatch) {
    return shortMatch[1].toUpperCase();
  }

  return full;
}
