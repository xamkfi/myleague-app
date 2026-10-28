import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { NewsMatchOption } from '../../../../api/news/newsMatchService';
import { usableTeamLogo } from '../../../../components/RichTextEditor/matchResultRender';

interface MatchPickerRowProps {
  match: NewsMatchOption;
  selected: boolean;
  /** Show the season or tournament name, useful when the list spans several. */
  showCompetition: boolean;
  onToggle: (match: NewsMatchOption) => void;
}

function TeamLogo({ url }: { url: string | null }) {
  const [failed, setFailed] = useState(false);
  const src: string | undefined = usableTeamLogo(url);
  if (!src || failed) {
    return <span className="match-picker-row__logo match-picker-row__logo--empty" aria-hidden="true" />;
  }
  return (
    <img className="match-picker-row__logo" src={src} alt="" loading="lazy" onError={() => setFailed(true)} />
  );
}

/** One selectable match in the news match picker. The whole row toggles the checkbox. */
export default function MatchPickerRow({ match, selected, showCompetition, onToggle }: MatchPickerRowProps) {
  const { t, i18n } = useTranslation();
  const date: Date = new Date(match.scheduledDateTime);
  const validDate: boolean = !Number.isNaN(date.getTime());
  const day: string = validDate
    ? date.toLocaleDateString(i18n.language, { day: 'numeric', month: 'numeric', year: 'numeric' })
    : '';
  const time: string = validDate
    ? date.toLocaleTimeString(i18n.language, { hour: '2-digit', minute: '2-digit' })
    : '';
  const hasScore: boolean = match.status === 'completed' || match.status === 'in_progress';
  const tbd: string = t('admin.news.matches.tbd');

  return (
    <li>
      <label className={`match-picker-row${selected ? ' match-picker-row--selected' : ''}`}>
        <input
          type="checkbox"
          className="match-picker-row__check"
          checked={selected}
          onChange={() => onToggle(match)}
        />
        <span className="match-picker-row__when">
          <span className="match-picker-row__day">{day}</span>
          <span className="match-picker-row__time">{time}</span>
        </span>
        <span className="match-picker-row__teams">
          <span className="match-picker-row__team match-picker-row__team--home">
            <span className="match-picker-row__team-name">{match.homeTeamName ?? tbd}</span>
            <TeamLogo url={match.homeTeamLogo} />
          </span>
          <span className={`match-picker-row__score${hasScore ? '' : ' match-picker-row__score--pending'}`}>
            {hasScore ? `${match.homeScore}–${match.awayScore}` : '–'}
          </span>
          <span className="match-picker-row__team match-picker-row__team--away">
            <TeamLogo url={match.awayTeamLogo} />
            <span className="match-picker-row__team-name">{match.awayTeamName ?? tbd}</span>
          </span>
        </span>
        <span className="match-picker-row__meta">
          <span className={`match-picker-row__status match-picker-row__status--${match.status}`}>
            {t(`newsPage.matchStatus.${match.status}`)}
          </span>
          {showCompetition && match.competitionName && (
            <span className="match-picker-row__competition">{match.competitionName}</span>
          )}
        </span>
      </label>
    </li>
  );
}
