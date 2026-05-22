import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { floorballTeamService } from "../../../api/floorball/floorballTeamService";
import {
    FloorballPosition,
    type FloorballActiveLineupPlayer,
    type FloorballMatchDto,
    type FloorballTeamPlayer,
} from "../../../types/floorball/floorballTypes";
import './MatchLineups.scss';

type RosterLookup = Map<string, FloorballTeamPlayer>;

interface ActiveRosterEntry {
    playerId: string;
    playerName: string;
    jerseyNumber?: number;
    position: FloorballPosition;
}

// Display order for the per-match active lineup. Goalkeeper first, then field roles
// (defender / center / forward) in fixed order so the two team columns line up.
const POSITION_DISPLAY_ORDER: FloorballPosition[] = [
    FloorballPosition.Goalkeeper,
    FloorballPosition.Defender,
    FloorballPosition.Center,
    FloorballPosition.Forward,
];

const sortByJerseyThenName = (a: ActiveRosterEntry, b: ActiveRosterEntry): number => {
    const numA: number = a.jerseyNumber ?? Number.MAX_SAFE_INTEGER;
    const numB: number = b.jerseyNumber ?? Number.MAX_SAFE_INTEGER;
    if (numA !== numB) return numA - numB;
    return a.playerName.localeCompare(b.playerName, undefined, { sensitivity: 'base' });
};

const buildRosterLookup = (roster: FloorballTeamPlayer[]): RosterLookup => {
    const byId: RosterLookup = new Map<string, FloorballTeamPlayer>();
    for (const player of roster) {
        byId.set(player.playerId, player);
    }
    return byId;
};

// Builds the displayable per-match lineup by joining the match's active player
// references with the team's full roster (for names and jersey numbers). Active
// goalie is appended explicitly because it is tracked outside the active player
// list on the match DTO.
const buildActiveRoster = (
    activePlayers: readonly FloorballActiveLineupPlayer[],
    activeGoalieId: string | undefined,
    lookup: RosterLookup
): ActiveRosterEntry[] => {
    const entries: ActiveRosterEntry[] = [];

    if (activeGoalieId) {
        const goalie: FloorballTeamPlayer | undefined = lookup.get(activeGoalieId);
        if (goalie) {
            entries.push({
                playerId: goalie.playerId,
                playerName: goalie.playerName,
                jerseyNumber: goalie.jerseyNumber,
                position: FloorballPosition.Goalkeeper,
            });
        }
    }

    for (const entry of activePlayers) {
        const player: FloorballTeamPlayer | undefined = lookup.get(entry.playerId);
        if (!player) continue;
        entries.push({
            playerId: player.playerId,
            playerName: player.playerName,
            jerseyNumber: player.jerseyNumber,
            position: entry.position,
        });
    }

    return entries;
};

export default function MatchLineups({ match }: { match: FloorballMatchDto }) {
    const navigate = useNavigate();
    const { t } = useTranslation();
    const [homeLookup, setHomeLookup] = useState<RosterLookup>(new Map());
    const [awayLookup, setAwayLookup] = useState<RosterLookup>(new Map());

    useEffect(() => {
        let cancelled: boolean = false;
        async function fetchRosters(): Promise<void> {
            try {
                const [homeResponse, awayResponse] = await Promise.all([
                    floorballTeamService.getById(match.homeTeamId),
                    floorballTeamService.getById(match.awayTeamId),
                ]);
                if (cancelled) return;
                setHomeLookup(buildRosterLookup(homeResponse.roster));
                setAwayLookup(buildRosterLookup(awayResponse.roster));
            } catch (error) {
                console.error('Failed to load team rosters for match lineup', error);
            }
        }
        fetchRosters();
        return () => {
            cancelled = true;
        };
    }, [match.homeTeamId, match.awayTeamId]);

    const homeActiveRoster: ActiveRosterEntry[] = useMemo(
        () => buildActiveRoster(match.homeActivePlayers ?? [], match.homeActiveGoalieId, homeLookup),
        [match.homeActivePlayers, match.homeActiveGoalieId, homeLookup]
    );

    const awayActiveRoster: ActiveRosterEntry[] = useMemo(
        () => buildActiveRoster(match.awayActivePlayers ?? [], match.awayActiveGoalieId, awayLookup),
        [match.awayActivePlayers, match.awayActiveGoalieId, awayLookup]
    );

    const handlePlayerClick = (playerId: string): void => {
        navigate(`/floorballplayer/${playerId}`);
    };

    const renderTeamRoster = (roster: ActiveRosterEntry[], teamName: string) => {
        if (roster.length === 0) {
            return (
                <div className="lineup-team-block">
                    <div className="lineup-team-title">{teamName}</div>
                    <div className="lineup-empty">
                        {t(
                            'matchPage.lineups.notSet',
                            'Ottelun kokoonpanoa ei ole vielä asetettu.'
                        )}
                    </div>
                </div>
            );
        }

        const presentPositions: FloorballPosition[] = POSITION_DISPLAY_ORDER.filter((pos) =>
            roster.some((entry) => entry.position === pos)
        );

        return (
            <div className="lineup-team-block">
                <div className="lineup-team-title">{teamName}</div>
                {presentPositions.map((pos) => {
                    const entries: ActiveRosterEntry[] = roster
                        .filter((entry) => entry.position === pos)
                        .sort(sortByJerseyThenName);
                    return (
                        <div key={pos} className="lineup-position-group">
                            <div className="lineup-position-label">
                                {t(`roster.positions.${pos}`)}
                            </div>
                            <div className="lineup-table-wrap">
                                <div className="lineup-row lineup-row-header">
                                    <div className="lineup-col-number">#</div>
                                    <div className="lineup-col-name">{t('roster.name')}</div>
                                    <div className="lineup-col-pos">{t('roster.position')}</div>
                                </div>
                                {entries.map((entry) => (
                                    <div
                                        key={entry.playerId}
                                        className="lineup-row lineup-row-player"
                                        onClick={() => handlePlayerClick(entry.playerId)}
                                    >
                                        <div className="lineup-col-number">
                                            <span className="jersey-badge">{entry.jerseyNumber}</span>
                                        </div>
                                        <div className="lineup-col-name player-link">
                                            {entry.playerName}
                                        </div>
                                        <div className="lineup-col-pos">
                                            {t(`roster.positions.${entry.position}`)}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    );
                })}
            </div>
        );
    };

    return (
        <div className="match-lineups-container">
            <div className="match-lineups-grid">
                {renderTeamRoster(homeActiveRoster, match.homeTeamName)}
                {renderTeamRoster(awayActiveRoster, match.awayTeamName)}
            </div>
        </div>
    );
}
