import { useState, type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { newsListUrl, formatNewsTagLabel } from '../NewsPage/newsListFilters';
import type { RelatedNewsTeam } from './extractRelatedNewsTeams';
import { SportsCategory } from '../../types/common/sports';
import SportIcon from '../../components/SportIcon/SportIcon';

type NewsTaxonomyBarProps = {
  sportCategory?: string;
  category?: string;
  tags?: string[];
  teams?: RelatedNewsTeam[];
  clickable?: boolean;
};

function sportLabelKey(sport: string): string {
  if (sport === SportsCategory.Floorball) return 'newsPage.sportCategory.floorball';
  if (sport === SportsCategory.Icehockey) return 'newsPage.sportCategory.hockey';
  if (sport === SportsCategory.Football) return 'newsPage.sportCategory.football';
  return sport;
}

function NewsTeamLogo({ team }: { team: RelatedNewsTeam }) {
  const [failed, setFailed] = useState(false);
  const hasLogo = Boolean(team.logoUrl?.trim()) && !failed;

  return (
    <span className="news-taxonomy-bar__team" title={team.name}>
      {hasLogo ? (
        <img
          src={team.logoUrl}
          alt={team.name}
          className="news-taxonomy-bar__team-logo"
          onError={() => setFailed(true)}
        />
      ) : (
        <span className="news-taxonomy-bar__team-placeholder" aria-hidden="true" />
      )}
      <span className="news-taxonomy-bar__team-name">{team.name}</span>
    </span>
  );
}

export default function NewsTaxonomyBar({
  sportCategory,
  category,
  tags = [],
  teams = [],
  clickable = false,
}: NewsTaxonomyBarProps) {
  const { t } = useTranslation();
  const hasTeams = teams.length > 0;
  const hasSport = Boolean(sportCategory);
  const hasCategory = Boolean(category);
  const hasTags = tags.length > 0;

  if (!hasTeams && !hasSport && !hasCategory && !hasTags) {
    return null;
  }

  const renderChip = (label: string, className: string, to?: string, icon?: ReactNode) => {
    const content = (
      <>
        {icon}
        {label}
      </>
    );

    if (clickable && to) {
      return (
        <Link to={to} className={className}>
          {content}
        </Link>
      );
    }

    return <span className={className}>{content}</span>;
  };

  return (
    <div className="news-taxonomy-bar">
      {hasTeams && (
        <div className="news-taxonomy-bar__teams" aria-label={t('newsPage.relatedTeams')}>
          {teams.map((team) => (
            <NewsTeamLogo key={team.name} team={team} />
          ))}
        </div>
      )}
      <div className="news-taxonomy-bar__chips">
        {sportCategory &&
          renderChip(
            t(sportLabelKey(sportCategory), sportCategory),
            'news-taxonomy-bar__chip news-taxonomy-bar__chip--sport',
            newsListUrl({ sportCategory }),
            <SportIcon sport={sportCategory} size="sm" decorative />
          )}
        {category &&
          renderChip(
            t(`newsPage.categoryValues.${category}`, category),
            'news-taxonomy-bar__chip news-taxonomy-bar__chip--category',
            newsListUrl({ category })
          )}
        {tags.map((tag) =>
          renderChip(
            formatNewsTagLabel(tag),
            'news-taxonomy-bar__chip news-taxonomy-bar__chip--tag',
            newsListUrl({ tag })
          )
        )}
      </div>
    </div>
  );
}
