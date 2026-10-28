import { useTranslation } from 'react-i18next';
import SegmentedControl, { type SegmentedOption } from '../../../../components/SegmentedControl/SegmentedControl';
import SportIcon from '../../../../components/SportIcon/SportIcon';
import { TeamCategory } from '../../../../types/floorball/floorballTypes';
import type { SportKind } from '../../../../utils/sportRoutes';
import { NEWS_CATEGORY_OPTIONS } from '../Utils/NewsFilterContstants';
import { newsAudienceLabel, newsCategoryLabel, newsSportLabel } from '../Utils/newsTaxonomyLabels';
import { sportCategoryForKind, sportKindForCategory } from './newsForm';

type SportChoice = SportKind | 'none';

const SPORT_CHOICES: SportChoice[] = ['floorball', 'football', 'hockey', 'none'];

interface NewsClassificationFieldsProps {
  sportCategory: string;
  category: string;
  teamCategory: string;
  onSportCategoryChange: (value: string) => void;
  onCategoryChange: (value: string) => void;
  onTeamCategoryChange: (value: string) => void;
}

/** Sport, category and audience of an article. The sport also opens the match picker on that sport. */
export default function NewsClassificationFields({
  sportCategory,
  category,
  teamCategory,
  onSportCategoryChange,
  onCategoryChange,
  onTeamCategoryChange,
}: NewsClassificationFieldsProps) {
  const { t } = useTranslation();
  const selectedSport: SportChoice = sportKindForCategory(sportCategory) ?? 'none';

  const sportOptions: SegmentedOption<SportChoice>[] = SPORT_CHOICES.map((choice) =>
    choice === 'none'
      ? { value: choice, label: t('admin.news.no_sport') }
      : {
          value: choice,
          label: newsSportLabel(t, sportCategoryForKind(choice)),
          icon: (active: boolean) => <SportIcon sport={choice} size="sm" inverted={active} decorative />,
        },
  );

  return (
    <div className="news-field-group">
      <div className="news-field">
        <span className="news-field__label" id="news-sport-label">{t('admin.news.sport_category')}</span>
        <SegmentedControl
          options={sportOptions}
          value={selectedSport}
          onChange={(choice) => onSportCategoryChange(choice === 'none' ? '' : sportCategoryForKind(choice))}
          ariaLabel={t('admin.news.sport_category')}
          tone="sport"
          layout="grid"
        />
      </div>

      <div className="news-field">
        <label htmlFor="news-category" className="news-field__label">
          {t('admin.news.category')}
        </label>
        <select
          id="news-category"
          className="news-field__input news-field__select"
          value={category}
          onChange={(event) => onCategoryChange(event.target.value)}
        >
          <option value="">{t('admin.news.select_category')}</option>
          {NEWS_CATEGORY_OPTIONS.map((option) => (
            <option key={option} value={option}>{newsCategoryLabel(t, option)}</option>
          ))}
        </select>
      </div>

      <div className="news-field">
        <label htmlFor="news-audience" className="news-field__label">
          {t('admin.news.team_category')}
        </label>
        <select
          id="news-audience"
          className="news-field__input news-field__select"
          value={teamCategory}
          onChange={(event) => onTeamCategoryChange(event.target.value)}
        >
          <option value="">{t('admin.news.all_audiences')}</option>
          {Object.values(TeamCategory).map((audience) => (
            <option key={audience} value={audience}>{newsAudienceLabel(t, audience)}</option>
          ))}
        </select>
      </div>
    </div>
  );
}
