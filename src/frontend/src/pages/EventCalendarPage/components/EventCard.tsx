import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { CalendarEvent } from '../../../types/calendar';
import './EventCard.scss';

interface EventCardProps {
  event: CalendarEvent;
}

export default function EventCard({ event }: EventCardProps) {
  const { t } = useTranslation();
  const isLive = event.status === 'live';
  const isCompleted = event.status === 'completed';
  const hasScore = isLive || isCompleted;

  return (
    <Link to={event.link} className={`event-card event-card--${event.status ?? 'scheduled'}`}>
      <div className="event-card__time-col">
        <span className={`event-card__status-dot event-card__status-dot--${event.status ?? 'scheduled'}`} />
        <span className="event-card__time">{event.time ?? '--:--'}</span>
      </div>

      <div className="event-card__info">
        <span className="event-card__title">{event.title}</span>
        <div className="event-card__meta">
          {event.venue && <span className="event-card__venue">{event.venue}</span>}
          {event.subtitle && <span className="event-card__subtitle">{event.subtitle}</span>}
        </div>
      </div>

      <div className="event-card__right">
        {hasScore ? (
          <span className="event-card__score">
            {event.homeScore} – {event.awayScore}
          </span>
        ) : (
          <span className="event-card__badge">{t('eventCalendarPage.upcoming')}</span>
        )}
        {isLive && <span className="event-card__live-label">{t('eventCalendarPage.status.live')}</span>}
      </div>
    </Link>
  );
}
