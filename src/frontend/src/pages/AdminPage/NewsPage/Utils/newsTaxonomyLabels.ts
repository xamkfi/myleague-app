const SPORT_I18N_KEYS: Record<string, string> = {
  Floorball: 'newsPage.sportCategory.floorball',
  Icehockey: 'newsPage.sportCategory.hockey',
  Football: 'newsPage.sportCategory.football',
};

export function newsCategoryLabel(
  t: (key: string, fallback?: string) => string,
  value: string,
): string {
  return t(`newsPage.categoryValues.${value}`, value);
}

export function newsSportLabel(
  t: (key: string, fallback?: string) => string,
  value: string,
): string {
  const key = SPORT_I18N_KEYS[value];
  return key ? t(key, value) : value;
}
