import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { TeamLink, type NamedTeam } from '../../../components/SportLinks';
import type { SportKind } from '../../../utils/sportRoutes';

export interface ClubTeamSeason {
  id: string;
  name: string;
}

interface ClubTeamSeasonListProps {
  sport: SportKind;
  teamId: string;
  teamName: string;
  teams: NamedTeam[];
  seasons: ClubTeamSeason[];
}

export function ClubTeamSeasonList({
  sport,
  teamId,
  teamName,
  teams,
  seasons,
}: ClubTeamSeasonListProps) {
  const { t } = useTranslation();
  const [historyOpen, setHistoryOpen] = useState(false);
  const latest = seasons[0];
  const history = seasons.slice(1);

  if (!latest) {
    return null;
  }

  return (
    <div className="club-page-team__seasons">
      <TeamLink
        sport={sport}
        teamId={teamId}
        teamName={teamName}
        teams={teams}
        seasonId={latest.id}
        className="club-page-team__season club-page-team__season--latest"
      >
        {latest.name}
      </TeamLink>

      {history.length > 0 && (
        <>
          <button
            type="button"
            className="club-page-team__history-toggle"
            aria-expanded={historyOpen}
            onClick={() => setHistoryOpen((open) => !open)}
          >
            {historyOpen
              ? t('clubPage.hideEarlierSeasons')
              : t('clubPage.showEarlierSeasons', { count: history.length })}
          </button>
          {historyOpen && (
            <div className="club-page-team__history">
              {history.map((season) => (
                <TeamLink
                  key={season.id}
                  sport={sport}
                  teamId={teamId}
                  teamName={teamName}
                  teams={teams}
                  seasonId={season.id}
                  className="club-page-team__season"
                >
                  {season.name}
                </TeamLink>
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
}
