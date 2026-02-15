import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { floorballTeamService } from "../../../api/floorball/floorballTeamService";
import type { FloorballMatchDto, FloorballTeamPlayer } from "../../../types/floorball/floorballTypes";
import './MatchLineups.scss';

export default function MatchLineups({match}: {match: FloorballMatchDto}) {
    const navigate = useNavigate();
    const { t } = useTranslation();
    const [homeRoster, setHomeRoster] = useState<FloorballTeamPlayer[]>([]);
    const [awayRoster, setAwayRoster] = useState<FloorballTeamPlayer[]>([]);

    useEffect(() => {
        async function fetchLineups() {
            const [homeResponse, awayResponse] = await Promise.all([
                floorballTeamService.getById(match.homeTeamId),
                floorballTeamService.getById(match.awayTeamId)
            ]);
            setHomeRoster(homeResponse.roster);
            setAwayRoster(awayResponse.roster);
        }
        fetchLineups();
    }, [match.homeTeamId, match.awayTeamId]);

    const positionOrder = ['Goalkeeper', 'Defender', 'Center', 'Forward'];

    const sortByPosition = (roster: FloorballTeamPlayer[]) => {
        return [...roster].sort((a, b) => {
            const posA = positionOrder.indexOf(a.position);
            const posB = positionOrder.indexOf(b.position);
            if (posA !== posB) return posA - posB;
            return (a.jerseyNumber || 0) - (b.jerseyNumber || 0);
        });
    };

    const handlePlayerClick = (playerId: string) => {
        navigate(`/floorballplayer/${playerId}`);
    };

    const renderTeamRoster = (roster: FloorballTeamPlayer[], teamName: string) => {
        const sortedRoster = sortByPosition(roster);
        const positions = [...new Set(sortedRoster.map(p => p.position))]
            .sort((a, b) => positionOrder.indexOf(a) - positionOrder.indexOf(b));

        return (
            <div className="lineup-team-block">
                <div className="lineup-team-title">{teamName}</div>
                {positions.map(pos => (
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
                            {sortedRoster
                                .filter(p => p.position === pos)
                                .map(player => (
                                    <div
                                        key={player.playerId}
                                        className="lineup-row lineup-row-player"
                                        onClick={() => handlePlayerClick(player.playerId)}
                                    >
                                        <div className="lineup-col-number">
                                            <span className="jersey-badge">{player.jerseyNumber}</span>
                                        </div>
                                        <div className="lineup-col-name player-link">
                                            {player.playerName}
                                        </div>
                                        <div className="lineup-col-pos">
                                            {t(`roster.positions.${player.position}`)}
                                        </div>
                                    </div>
                                ))}
                        </div>
                    </div>
                ))}
            </div>
        );
    };

    return (
        <div className="match-lineups-container">
            <div className="match-lineups-grid">
                {renderTeamRoster(homeRoster, match.homeTeamName)}
                {renderTeamRoster(awayRoster, match.awayTeamName)}
            </div>
        </div>
    );
}
