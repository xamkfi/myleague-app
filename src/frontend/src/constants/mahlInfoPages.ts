export type MahlInfoPageDefinition = {
    slug: string;
    path: string;
    labelKey: string;
    defaultLabel: string;
    defaultTitle: string;
};

export const MAHL_INFO_PAGES: MahlInfoPageDefinition[] = [
    {
        slug: "mahl-summary",
        path: "/mahl",
        labelKey: "rules.mahlNav.summary",
        defaultLabel: "Summary",
        defaultTitle: "Summary",
    },
    {
        slug: "mahl-finance",
        path: "/mahl/seuran-talous",
        labelKey: "rules.mahlNav.finance",
        defaultLabel: "Seuran talous",
        defaultTitle: "Seuran talous",
    },
    {
        slug: "mahl-partners",
        path: "/mahl/kumppanuudet",
        labelKey: "rules.mahlNav.partners",
        defaultLabel: "Kumppanuudet",
        defaultTitle: "Kumppanuudet",
    },
    {
        slug: "mahl-responsibility",
        path: "/mahl/vastuullisuus",
        labelKey: "rules.mahlNav.responsibility",
        defaultLabel: "Vastuullisuus",
        defaultTitle: "Vastuullisuus",
    },
];

export function getMahlInfoPageBySlug(
    slug: string,
): MahlInfoPageDefinition | undefined {
    return MAHL_INFO_PAGES.find((page) => page.slug === slug);
}

export function getMahlInfoPageByPath(
    path: string,
): MahlInfoPageDefinition | undefined {
    return MAHL_INFO_PAGES.find((page) => page.path === path);
}
