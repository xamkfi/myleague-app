import { useTranslation } from 'react-i18next';

export interface FloorballDecisionRow {
  gamesPlayed: number;
  regulationWins?: number;
  overtimeWins?: number;
  shootoutWins?: number;
  regulationLosses?: number;
  overtimeLosses?: number;
  shootoutLosses?: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  points: number;
}

export function FloorballDecisionColGroup() {
  return (
    <>
      <col className="stats-col" />
      <col className="stats-col" />
      <col className="stats-col" />
      <col className="stats-col" />
      <col className="stats-col" />
      <col className="stats-col" />
      <col className="stats-col" />
      <col className="stats-col" />
      <col className="points-col" />
    </>
  );
}

export function FloorballDecisionHeaderCells() {
  const { t } = useTranslation();
  const key = 'leaguePage.standings.floorballColumns';
  return (
    <>
      <th className="stats-col" title={t(`${key}.gpTitle`)}>{t(`${key}.gp`)}</th>
      <th className="stats-col" title={t(`${key}.wTitle`)}>{t(`${key}.w`)}</th>
      <th className="stats-col" title={t(`${key}.otwTitle`)}>{t(`${key}.otw`)}</th>
      <th className="stats-col" title={t(`${key}.otlTitle`)}>{t(`${key}.otl`)}</th>
      <th className="stats-col" title={t(`${key}.lTitle`)}>{t(`${key}.l`)}</th>
      <th className="stats-col" title={t(`${key}.gfTitle`)}>{t(`${key}.gf`)}</th>
      <th className="stats-col" title={t(`${key}.gaTitle`)}>{t(`${key}.ga`)}</th>
      <th className="stats-col" title={t(`${key}.gdTitle`)}>{t(`${key}.gd`)}</th>
      <th className="points-col" title={t(`${key}.ptsTitle`)}>{t(`${key}.pts`)}</th>
    </>
  );
}

interface FloorballDecisionCellsProps {
  row: FloorballDecisionRow;
}

export function FloorballDecisionCells({ row }: FloorballDecisionCellsProps) {
  const decidedWins: number = (row.overtimeWins ?? 0) + (row.shootoutWins ?? 0);
  const decidedLosses: number = (row.overtimeLosses ?? 0) + (row.shootoutLosses ?? 0);
  return (
    <>
      <td className="stats-col">{row.gamesPlayed}</td>
      <td className="stats-col">{row.regulationWins ?? 0}</td>
      <td className="stats-col">{decidedWins}</td>
      <td className="stats-col">{decidedLosses}</td>
      <td className="stats-col">{row.regulationLosses ?? 0}</td>
      <td className="stats-col">{row.goalsFor}</td>
      <td className="stats-col">{row.goalsAgainst}</td>
      <td className="stats-col">{row.goalDifference}</td>
      <td className="points-col">{row.points}</td>
    </>
  );
}
