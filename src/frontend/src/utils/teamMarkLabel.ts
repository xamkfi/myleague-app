export function teamMarkLabel(name: string): string {
  const trimmed = name.trim();
  if (trimmed.length === 0) {
    return '';
  }

  const parts = trimmed.split(/[\s-]+/).filter((part) => part.length > 0);
  if (parts.length >= 2) {
    return `${parts[0].charAt(0)}${parts[1].charAt(0)}`.toLocaleUpperCase('fi-FI');
  }

  return trimmed.slice(0, 2).toLocaleUpperCase('fi-FI');
}
