import { useMemo, type ReactNode } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import AdminPageTemplate from '../../components/PageTemplate/AdminPageTemplate';
import SportIcon from '../../components/SportIcon/SportIcon';
import type { SportKind } from '../../utils/sportRoutes';
import './AdminPage.scss';
import PersonIcon from '../../assets/adminIcons/Persons.svg';
import NewsIcon from '../../assets/adminIcons/News.svg';
import LeaguesIcon from '../../assets/adminIcons/Leagues.svg';
import ClubsIcon from '../../assets/adminIcons/Clubs.svg';
import RulesIcon from '../../assets/adminIcons/Rules.svg';
import TeamsIcon from '../../assets/adminIcons/Teams.svg';
import SeasonsIcon from '../../assets/adminIcons/Seasons.svg';
import MatchesIcon from '../../assets/adminIcons/Matches.svg';
import RefereesIcon from '../../assets/adminIcons/Referees.svg';

interface AdminHomeCard {
  to: string;
  titleKey: string;
  titleFallback: string;
  subtitleKey: string;
  subtitleFallback: string;
  iconSrc: string;
}

interface AdminHomeSection {
  titleKey: string;
  titleFallback: string;
  cards: AdminHomeCard[];
}

interface SportHomeGroup {
  sport: SportKind;
  titleKey: string;
  titleFallback: string;
  cards: AdminHomeCard[];
}

const ADMIN_SPORTS: SportKind[] = ['floorball', 'football', 'hockey'];

function isSportKind(value: string | null): value is SportKind {
  return value === 'floorball' || value === 'football' || value === 'hockey';
}

const leagueCards: AdminHomeCard[] = [
  {
    to: '/admin/clubs',
    titleKey: 'admin.actions.clubs',
    titleFallback: 'Clubs',
    subtitleKey: 'admin.actions.manageClubs',
    subtitleFallback: 'Manage clubs',
    iconSrc: ClubsIcon,
  },
  {
    to: '/admin/divisions',
    titleKey: 'admin.actions.divisions',
    titleFallback: 'Divisions',
    subtitleKey: 'admin.actions.manageDivisions',
    subtitleFallback: 'Manage sport divisions',
    iconSrc: LeaguesIcon,
  },
  {
    to: '/admin/persons',
    titleKey: 'admin.actions.persons',
    titleFallback: 'Persons',
    subtitleKey: 'admin.actions.managePerson',
    subtitleFallback: 'Manage persons',
    iconSrc: PersonIcon,
  },
];

const siteContentCards: AdminHomeCard[] = [
  {
    to: '/admin/news',
    titleKey: 'admin.actions.news',
    titleFallback: 'News',
    subtitleKey: 'admin.actions.manageNews',
    subtitleFallback: 'Manage news',
    iconSrc: NewsIcon,
  },
  {
    to: '/admin/site-content/info-pages',
    titleKey: 'admin.siteContent.infoPages.nav',
    titleFallback: 'MAHL info pages',
    subtitleKey: 'admin.actions.manageInfoPages',
    subtitleFallback: 'Manage info pages',
    iconSrc: RulesIcon,
  },
  {
    to: '/admin/site-content/rules',
    titleKey: 'admin.siteContent.rules',
    titleFallback: 'Rules',
    subtitleKey: 'admin.actions.manageRules',
    subtitleFallback: 'Manage rules',
    iconSrc: RulesIcon,
  },
  {
    to: '/admin/site-content/footer-contacts',
    titleKey: 'admin.siteContent.footerContacts.nav',
    titleFallback: 'Footer content',
    subtitleKey: 'admin.actions.manageFooterContacts',
    subtitleFallback: 'Manage footer content',
    iconSrc: LeaguesIcon,
  },
];

const administrationCards: AdminHomeCard[] = [
  {
    to: '/admin/users',
    titleKey: 'admin.actions.users',
    titleFallback: 'System Users',
    subtitleKey: 'admin.actions.manageUsers',
    subtitleFallback: 'Manage system users',
    iconSrc: PersonIcon,
  },
  {
    to: '/admin/settings',
    titleKey: 'admin.settings.nav',
    titleFallback: 'Settings',
    subtitleKey: 'admin.actions.manageSettings',
    subtitleFallback: 'Manage site settings',
    iconSrc: LeaguesIcon,
  },
];

function sportCards(sport: SportKind): AdminHomeCard[] {
  const labelNamespace = `${sport}.management.actions`;
  const basePath = `/admin/${sport}`;
  const officialsSlug = sport === 'hockey' ? 'officials' : 'referees';
  const officialsSubtitleKey = sport === 'hockey' ? 'manageOfficials' : 'manageReferees';

  return [
    {
      to: `${basePath}/teams`,
      titleKey: 'admin.home.teams',
      titleFallback: 'Teams',
      subtitleKey: `${labelNamespace}.manageTeams`,
      subtitleFallback: 'Manage teams',
      iconSrc: TeamsIcon,
    },
    {
      to: `${basePath}/players`,
      titleKey: 'admin.home.players',
      titleFallback: 'Players',
      subtitleKey: `${labelNamespace}.managePlayers`,
      subtitleFallback: 'Manage players',
      iconSrc: PersonIcon,
    },
    {
      to: `${basePath}/seasons`,
      titleKey: 'admin.home.seasons',
      titleFallback: 'Seasons',
      subtitleKey: `${labelNamespace}.manageSeasons`,
      subtitleFallback: 'Manage seasons',
      iconSrc: SeasonsIcon,
    },
    {
      to: `${basePath}/tournaments`,
      titleKey: 'admin.home.tournaments',
      titleFallback: 'Tournaments',
      subtitleKey: `${labelNamespace}.manageTournaments`,
      subtitleFallback: 'Manage tournaments',
      iconSrc: SeasonsIcon,
    },
    {
      to: `${basePath}/matches`,
      titleKey: 'admin.home.matches',
      titleFallback: 'Matches',
      subtitleKey: `${labelNamespace}.manageMatches`,
      subtitleFallback: 'Manage matches',
      iconSrc: MatchesIcon,
    },
    {
      to: `${basePath}/${officialsSlug}`,
      titleKey: 'admin.home.referees',
      titleFallback: 'Referees',
      subtitleKey: `${labelNamespace}.${officialsSubtitleKey}`,
      subtitleFallback: sport === 'hockey' ? 'Manage officials' : 'Manage referees',
      iconSrc: RefereesIcon,
    },
  ];
}

const sportGroups: SportHomeGroup[] = [
  {
    sport: 'floorball',
    titleKey: 'admin.actions.floorball',
    titleFallback: 'Floorball',
    cards: sportCards('floorball'),
  },
  {
    sport: 'football',
    titleKey: 'admin.actions.football',
    titleFallback: 'Football',
    cards: sportCards('football'),
  },
  {
    sport: 'hockey',
    titleKey: 'admin.actions.hockey',
    titleFallback: 'Ice hockey',
    cards: sportCards('hockey'),
  },
];

const leagueSection: AdminHomeSection = {
  titleKey: 'admin.nav.league',
  titleFallback: 'League data',
  cards: leagueCards,
};

const siteContentSection: AdminHomeSection = {
  titleKey: 'admin.siteContent.title',
  titleFallback: 'Site content',
  cards: siteContentCards,
};

const administrationSection: AdminHomeSection = {
  titleKey: 'admin.nav.administration',
  titleFallback: 'Administration',
  cards: administrationCards,
};

interface AdminHomeCardLinkProps {
  card: AdminHomeCard;
  title: string;
  subtitle: string;
}

function AdminHomeCardLink({ card, title, subtitle }: AdminHomeCardLinkProps) {
  return (
    <Link to={card.to} className="admin-action-button">
      <div className="button-text">
        <span className="button-title">{title}</span>
        <span className="button-subtitle">{subtitle}</span>
      </div>
      <img src={card.iconSrc} alt="" className="button-icon" />
    </Link>
  );
}

function AdminPage() {
  const { t } = useTranslation();
  const [searchParams, setSearchParams] = useSearchParams();

  const activeSport: SportKind = useMemo(() => {
    const requested = searchParams.get('sport');
    if (isSportKind(requested) && ADMIN_SPORTS.includes(requested)) {
      return requested;
    }
    return ADMIN_SPORTS[0];
  }, [searchParams]);

  const activeGroup = sportGroups.find((group) => group.sport === activeSport) ?? sportGroups[0];

  const handleSportChange = (sport: SportKind): void => {
    setSearchParams({ sport });
  };

  const renderCards = (cards: AdminHomeCard[]): ReactNode => (
    <div className="admin-actions">
      {cards.map((card) => (
        <AdminHomeCardLink
          key={card.to}
          card={card}
          title={t(card.titleKey, card.titleFallback)}
          subtitle={t(card.subtitleKey, card.subtitleFallback)}
        />
      ))}
    </div>
  );

  return (
    <AdminPageTemplate title={t('admin.title', 'Admin Dashboard')}>
      <div className="admin-container">
        <section className="admin-section">
          <h2 className="admin-section-title">{t(leagueSection.titleKey, leagueSection.titleFallback)}</h2>
          {renderCards(leagueSection.cards)}
        </section>

        <section className="admin-section">
          <h2 className="admin-section-title">{t('admin.sportsTitle', 'Sports')}</h2>
          <div className="admin-sport-tabs" role="tablist" aria-label={t('admin.sportsTitle', 'Sports')}>
            <div className="admin-sport-tabs__group">
              {sportGroups.map((group) => (
                <button
                  key={group.sport}
                  type="button"
                  role="tab"
                  aria-selected={group.sport === activeSport}
                  className={`tab-button${group.sport === activeSport ? ' active' : ''}`}
                  onClick={() => handleSportChange(group.sport)}
                >
                  <SportIcon
                    sport={group.sport}
                    size="sm"
                    inverted={group.sport === activeSport}
                    decorative
                  />
                  <span>{t(group.titleKey, group.titleFallback)}</span>
                </button>
              ))}
            </div>
          </div>
          {renderCards(activeGroup.cards)}
        </section>

        <section className="admin-section">
          <h2 className="admin-section-title">{t(siteContentSection.titleKey, siteContentSection.titleFallback)}</h2>
          {renderCards(siteContentSection.cards)}
        </section>

        <section className="admin-section">
          <h2 className="admin-section-title">{t(administrationSection.titleKey, administrationSection.titleFallback)}</h2>
          {renderCards(administrationSection.cards)}
        </section>
      </div>
    </AdminPageTemplate>
  );
}

export default AdminPage;
