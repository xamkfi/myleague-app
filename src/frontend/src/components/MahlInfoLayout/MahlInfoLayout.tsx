import { Link, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
    CHILDREN_INFO_PAGES,
    MAHL_INFO_PAGES,
} from "../../constants/mahlInfoPages";
import "./MahlInfoLayout.scss";

export type MahlInfoNavGroup = "mahl" | "children";

interface MahlInfoLayoutProps {
    children: React.ReactNode;
    pageTitle?: string;
    intro?: string;
    navGroup?: MahlInfoNavGroup;
}

const mahlNavLinks = [
    ...MAHL_INFO_PAGES.map((page) => ({
        labelKey: page.labelKey,
        defaultLabel: page.defaultLabel,
        path: page.path,
    })),
    {
        labelKey: "rules.mahlNav.rules",
        defaultLabel: "Säännöt",
        path: "/saannot",
    },
];

const childrenNavLinks = CHILDREN_INFO_PAGES.map((page) => ({
    labelKey: page.labelKey,
    defaultLabel: page.defaultLabel,
    path: page.path,
}));

export default function MahlInfoLayout({
    children,
    pageTitle,
    intro,
    navGroup = "mahl",
}: MahlInfoLayoutProps) {
    const { t } = useTranslation();
    const location = useLocation();
    const navLinks =
        navGroup === "children" ? childrenNavLinks : mahlNavLinks;

    return (
        <main className="mahl-info-layout">
            <section className="mahl-info-layout__hero" aria-label={pageTitle}>
                <div className="mahl-info-layout__hero-overlay" aria-hidden="true" />

                <div className="mahl-info-layout__hero-inner">
                    <div className="mahl-info-layout__hero-content">
                        <h1 className="mahl-info-layout__title">
                            {pageTitle || t("nav.mahl", "MAHL")}
                        </h1>

                        <nav
                            className="mahl-info-layout__nav"
                            aria-label={t(
                                "rules.mahlNavigation",
                                "MAHL navigaatio",
                            )}
                        >
                            {navLinks.map((link) => {
                                const isActive =
                                    location.pathname === link.path;

                                return (
                                    <Link
                                        key={link.path}
                                        to={link.path}
                                        className={
                                            isActive
                                                ? "mahl-info-layout__nav-link mahl-info-layout__nav-link--active"
                                                : "mahl-info-layout__nav-link"
                                        }
                                    >
                                        {t(link.labelKey, link.defaultLabel)}
                                    </Link>
                                );
                            })}
                        </nav>
                    </div>
                </div>
            </section>

            <div className="mahl-info-layout__content-shell">
                {intro && (
                    <p className="mahl-info-layout__intro">{intro}</p>
                )}

                <div className="mahl-info-layout__content">{children}</div>
            </div>
        </main>
    );
}
