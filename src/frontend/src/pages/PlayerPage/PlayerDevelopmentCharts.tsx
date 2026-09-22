import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import {
  aggregateGoalieSeasons,
  aggregateSkaterSeasons,
  type GoalieSeasonInput,
  type GoalieSeasonPoint,
  type SkaterSeasonInput,
  type SkaterSeasonPoint,
} from './playerSeasonSeries';

interface PlayerDevelopmentChartsProps {
  skaterSeasons: SkaterSeasonInput[];
  goalieSeasons: GoalieSeasonInput[];
}

const CHART_WIDTH = 640;
const CHART_HEIGHT = 280;
const PAD = { top: 16, right: 64, bottom: 118, left: 56 };
const TICK_COUNT = 4;

interface PlotPoint {
  x: number;
  y: number;
}

function axisCeiling(value: number): number {
  if (value <= 0) {
    return 1;
  }
  const magnitude = 10 ** Math.floor(Math.log10(value));
  const normalized = value / magnitude;
  const nice = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
  return nice * magnitude;
}

function ticks(max: number): number[] {
  return Array.from({ length: TICK_COUNT + 1 }, (_, index) => (max / TICK_COUNT) * index);
}

function formatTick(value: number): string {
  const rounded = Math.round(value * 100) / 100;
  return String(rounded);
}

function shortLabel(label: string): string {
  const yearMatch = label.match(/(\d{4})(?:-(\d{4}))?/);
  const start = yearMatch?.[1]?.slice(2) ?? '';
  const end = yearMatch?.[2]?.slice(2);
  const yearText = end ? `${start}-${end}` : start;
  const distinctive = label
    .replace(/salibandy|jalkapallo|jääkiekko|hockey|mahl/gi, '')
    .replace(/\d{4}(?:-\d{4})?/g, '')
    .replace(/\|/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();
  const text = [yearText, distinctive].filter((part) => part.length > 0).join(' ');
  return text.length > 18 ? `${text.slice(0, 16)}…` : text;
}

function linePath(points: PlotPoint[]): string {
  return points
    .map((point, index) => `${index === 0 ? 'M' : 'L'} ${point.x.toFixed(1)} ${point.y.toFixed(1)}`)
    .join(' ');
}

function plotY(value: number, max: number): number {
  const safeMax = max > 0 ? max : 1;
  return PAD.top + (CHART_HEIGHT - PAD.top - PAD.bottom) * (1 - value / safeMax);
}

interface ChartFrameProps {
  title: string;
  ariaLabel: string;
  leftMax: number;
  rightMax: number;
  labels: string[];
  children: ReactNode;
}

function ChartFrame({ title, ariaLabel, leftMax, rightMax, labels, children }: ChartFrameProps) {
  const plotBottom = CHART_HEIGHT - PAD.bottom;
  const plotWidth = CHART_WIDTH - PAD.left - PAD.right;
  const slot = labels.length > 0 ? plotWidth / labels.length : plotWidth;

  return (
    <figure className="player-chart">
      <figcaption className="player-chart__title">{title}</figcaption>
      <svg
        className="player-chart__svg"
        viewBox={`0 0 ${CHART_WIDTH} ${CHART_HEIGHT}`}
        role="img"
        aria-label={ariaLabel}
      >
        {ticks(leftMax).map((tick) => {
          const y = plotY(tick, leftMax);
          return (
            <g key={`left-${tick}`}>
              <line className="player-chart__grid" x1={PAD.left} x2={CHART_WIDTH - PAD.right} y1={y} y2={y} />
              <text className="player-chart__tick" x={PAD.left - 8} y={y + 4} textAnchor="end">
                {formatTick(tick)}
              </text>
            </g>
          );
        })}
        {ticks(rightMax).map((tick) => (
          <text
            key={`right-${tick}`}
            className="player-chart__tick player-chart__tick--right"
            x={CHART_WIDTH - PAD.right + 8}
            y={plotY(tick, rightMax) + 4}
            textAnchor="start"
          >
            {formatTick(tick)}
          </text>
        ))}
        <line
          className="player-chart__axis"
          x1={PAD.left}
          x2={CHART_WIDTH - PAD.right}
          y1={plotBottom}
          y2={plotBottom}
        />
        {labels.map((label, index) => {
          const x = PAD.left + slot * index + slot / 2;
          const y = plotBottom + 14;
          return (
            <text
              key={`${label}-${index}`}
              className="player-chart__label"
              x={x}
              y={y}
              textAnchor="end"
              transform={`rotate(-55 ${x} ${y})`}
            >
              <title>{label}</title>
              {shortLabel(label)}
            </text>
          );
        })}
        {children}
      </svg>
    </figure>
  );
}

interface SkaterChartProps {
  points: SkaterSeasonPoint[];
}

function SkaterChart({ points }: SkaterChartProps) {
  const { t } = useTranslation();
  const plotWidth = CHART_WIDTH - PAD.left - PAD.right;
  const plotHeight = CHART_HEIGHT - PAD.top - PAD.bottom;
  const plotBottom = PAD.top + plotHeight;
  const slot = plotWidth / points.length;
  const barWidth = Math.min(28, slot * 0.42);
  const stackMax = axisCeiling(Math.max(...points.map((point) => point.goals + point.assists), 1));
  const rateMax = axisCeiling(Math.max(...points.map((point) => point.pointsPerGame), 0.5));
  const linePoints: PlotPoint[] = points.map((point, index) => ({
    x: PAD.left + slot * index + slot / 2,
    y: plotY(point.pointsPerGame, rateMax),
  }));

  return (
    <div className="player-chart-block">
      <ChartFrame
        title={t('playerPage.charts.skaterTitle')}
        ariaLabel={t('playerPage.charts.skaterTitle')}
        leftMax={stackMax}
        rightMax={rateMax}
        labels={points.map((point) => point.seasonLabel)}
      >
        {points.map((point, index) => {
          const goalsHeight = (point.goals / stackMax) * plotHeight;
          const assistsHeight = (point.assists / stackMax) * plotHeight;
          const stackGap = goalsHeight > 0 && assistsHeight > 0 ? 2 : 0;
          const x = PAD.left + slot * index + (slot - barWidth) / 2;
          const summary = `${point.seasonLabel}: ${point.goals} ${t('playerPage.charts.goals')}, ${point.assists} ${t('playerPage.charts.assists')}, ${point.pointsPerGame.toFixed(2)} ${t('playerPage.charts.pointsPerGame')}`;
          return (
            <g key={`${point.seasonLabel}-${index}`}>
              <title>{summary}</title>
              {goalsHeight > 0 && (
                <rect
                  className="player-chart__bar player-chart__bar--goals"
                  x={x}
                  y={plotBottom - goalsHeight}
                  width={barWidth}
                  height={goalsHeight}
                  rx={2}
                />
              )}
              {assistsHeight > 0 && (
                <rect
                  className="player-chart__bar player-chart__bar--assists"
                  x={x}
                  y={plotBottom - goalsHeight - assistsHeight - stackGap}
                  width={barWidth}
                  height={assistsHeight}
                  rx={2}
                />
              )}
            </g>
          );
        })}
        <path className="player-chart__line player-chart__line--points" d={linePath(linePoints)} />
        {linePoints.map((point, index) => (
          <circle
            key={points[index]?.seasonLabel ?? index}
            className="player-chart__dot player-chart__dot--points"
            cx={point.x}
            cy={point.y}
            r={4}
          />
        ))}
      </ChartFrame>
      <ul className="player-chart__legend">
        <li><span className="player-chart__swatch player-chart__swatch--goals" />{t('playerPage.charts.goals')}</li>
        <li><span className="player-chart__swatch player-chart__swatch--assists" />{t('playerPage.charts.assists')}</li>
        <li><span className="player-chart__swatch player-chart__swatch--points" />{t('playerPage.charts.pointsPerGame')}</li>
      </ul>
    </div>
  );
}

interface GoalieChartProps {
  points: GoalieSeasonPoint[];
}

function GoalieChart({ points }: GoalieChartProps) {
  const { t } = useTranslation();
  const plotWidth = CHART_WIDTH - PAD.left - PAD.right;
  const slot = plotWidth / points.length;
  const saveMax = 100;
  const averageMax = axisCeiling(Math.max(...points.map((point) => point.goalsAgainstAverage), 1));
  const saveLine: PlotPoint[] = points.map((point, index) => ({
    x: PAD.left + slot * index + slot / 2,
    y: plotY(point.savePercentage, saveMax),
  }));
  const averageLine: PlotPoint[] = points.map((point, index) => ({
    x: PAD.left + slot * index + slot / 2,
    y: plotY(point.goalsAgainstAverage, averageMax),
  }));

  return (
    <div className="player-chart-block">
      <ChartFrame
        title={t('playerPage.charts.goalieTitle')}
        ariaLabel={t('playerPage.charts.goalieTitle')}
        leftMax={saveMax}
        rightMax={averageMax}
        labels={points.map((point) => point.seasonLabel)}
      >
        <path className="player-chart__line player-chart__line--saves" d={linePath(saveLine)} />
        <path className="player-chart__line player-chart__line--average" d={linePath(averageLine)} />
        {points.map((point, index) => {
          const summary = `${point.seasonLabel}: ${point.savePercentage.toFixed(1)}% ${t('playerPage.charts.savePercentage')}, ${point.goalsAgainstAverage.toFixed(2)} ${t('playerPage.charts.goalsAgainstAverage')}`;
          return (
            <g key={`${point.seasonLabel}-${index}`}>
              <title>{summary}</title>
              <circle className="player-chart__dot player-chart__dot--saves" cx={saveLine[index]?.x ?? 0} cy={saveLine[index]?.y ?? 0} r={4} />
              <circle className="player-chart__dot player-chart__dot--average" cx={averageLine[index]?.x ?? 0} cy={averageLine[index]?.y ?? 0} r={4} />
            </g>
          );
        })}
      </ChartFrame>
      <ul className="player-chart__legend">
        <li><span className="player-chart__swatch player-chart__swatch--saves" />{t('playerPage.charts.savePercentage')}</li>
        <li><span className="player-chart__swatch player-chart__swatch--average" />{t('playerPage.charts.goalsAgainstAverage')}</li>
      </ul>
    </div>
  );
}

export default function PlayerDevelopmentCharts({
  skaterSeasons,
  goalieSeasons,
}: PlayerDevelopmentChartsProps) {
  const { t } = useTranslation();
  const skaterPoints = aggregateSkaterSeasons(skaterSeasons);
  const goaliePoints = aggregateGoalieSeasons(goalieSeasons);

  return (
    <div className="player-charts">
      {skaterPoints.length >= 2 && <SkaterChart points={skaterPoints} />}
      {goaliePoints.length >= 2 && <GoalieChart points={goaliePoints} />}
      {goalieSeasons.length > 0 && goaliePoints.length < 2 && (
        <p className="no-data-message">{t('playerPage.charts.needMoreGoalieSeasons')}</p>
      )}
    </div>
  );
}
