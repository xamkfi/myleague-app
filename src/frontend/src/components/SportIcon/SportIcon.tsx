import { SportsCategory } from '../../types/common/sports';
import type { SportKind } from '../../utils/sportRoutes';
import FloorballIcon from '../../assets/sportIcons/Floorball.svg';
import FootballIcon from '../../assets/sportIcons/Football.svg';
import IceHockeyIcon from '../../assets/sportIcons/IceHockey.svg';
import SportsIcon from '../../assets/adminIcons/Sports.svg';
import './SportIcon.scss';

export type SportIconSport = SportsCategory | SportKind | 'icehockey';

export type SportIconSize = 'sm' | 'md' | 'lg';

interface SportIconProps {
  sport: SportIconSport | string;
  size?: SportIconSize;
  className?: string;
  decorative?: boolean;
  inverted?: boolean;
  alt?: string;
}

function resolveSportSrc(sport: string): string {
  const normalized = sport.toLowerCase();
  if (normalized === 'floorball') {
    return FloorballIcon;
  }
  if (normalized === 'football') {
    return FootballIcon;
  }
  if (normalized === 'icehockey' || normalized === 'hockey') {
    return IceHockeyIcon;
  }
  return SportsIcon;
}

function SportIcon({
  sport,
  size = 'md',
  className,
  decorative = true,
  inverted = false,
  alt = '',
}: SportIconProps) {
  const classNames = [
    'sport-icon',
    `sport-icon--${size}`,
    inverted ? 'sport-icon--inverted' : '',
    className ?? '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <img
      src={resolveSportSrc(sport)}
      alt={decorative ? '' : alt}
      className={classNames}
      aria-hidden={decorative ? true : undefined}
    />
  );
}

export default SportIcon;
