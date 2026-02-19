import { useMemo, useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import type { CalendarEvent } from '../../../types/calendar';
import EventCard from './EventCard';
import './EventAgendaList.scss';

interface EventAgendaListProps {
  events: CalendarEvent[];
  selectedDay: number | null;
  year: number;
  month: number;
}

interface DayGroup {
  dateKey: string;
  day: number;
  label: string;
  events: CalendarEvent[];
}

function formatDayLabel(dateKey: string): string {
  const d = new Date(dateKey + 'T00:00:00');
  return d.toLocaleDateString(undefined, {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
  });
}

export default function EventAgendaList({ events, selectedDay, year, month }: EventAgendaListProps) {
  const { t } = useTranslation();
  const containerRef = useRef<HTMLDivElement>(null);
  const dayRefs = useRef<Map<number, HTMLDivElement>>(new Map());

  const grouped = useMemo(() => {
    const byDate = new Map<string, CalendarEvent[]>();
    events.forEach((e) => {
      const list = byDate.get(e.date) ?? [];
      list.push(e);
      byDate.set(e.date, list);
    });

    const groups: DayGroup[] = [];
    const sortedKeys = [...byDate.keys()].sort();
    sortedKeys.forEach((dateKey) => {
      const dayEvents = byDate.get(dateKey)!;
      dayEvents.sort((a, b) => (a.time ?? '').localeCompare(b.time ?? ''));
      const day = parseInt(dateKey.split('-')[2], 10);
      groups.push({
        dateKey,
        day,
        label: formatDayLabel(dateKey),
        events: dayEvents,
      });
    });
    return groups;
  }, [events]);

  const visibleGroups = useMemo(() => {
    if (selectedDay === null) return grouped;
    return grouped.filter((g) => g.day === selectedDay);
  }, [grouped, selectedDay]);

  useEffect(() => {
    if (selectedDay !== null) {
      const el = dayRefs.current.get(selectedDay);
      if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }
  }, [selectedDay]);

  const setDayRef = (day: number, el: HTMLDivElement | null) => {
    if (el) dayRefs.current.set(day, el);
    else dayRefs.current.delete(day);
  };

  if (events.length === 0) {
    return (
      <div className="event-agenda-list__empty">
        <div className="event-agenda-list__empty-icon">
          <svg width="48" height="48" viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
            <rect x="6" y="10" width="36" height="32" rx="4" stroke="currentColor" strokeWidth="2" fill="none" />
            <path d="M6 18h36" stroke="currentColor" strokeWidth="2" />
            <rect x="14" y="6" width="2" height="8" rx="1" fill="currentColor" />
            <rect x="32" y="6" width="2" height="8" rx="1" fill="currentColor" />
          </svg>
        </div>
        <p className="event-agenda-list__empty-text">{t('eventCalendarPage.noEventsMonth')}</p>
      </div>
    );
  }

  if (visibleGroups.length === 0 && selectedDay !== null) {
    const selectedDateKey = `${year}-${month.toString().padStart(2, '0')}-${selectedDay.toString().padStart(2, '0')}`;
    const dayLabel = formatDayLabel(selectedDateKey);
    return (
      <div className="event-agenda-list__empty">
        <p className="event-agenda-list__empty-text">
          {t('eventCalendarPage.noEventsDay', { day: dayLabel })}
        </p>
      </div>
    );
  }

  return (
    <div className="event-agenda-list" ref={containerRef}>
      {visibleGroups.map((group) => (
        <div
          key={group.dateKey}
          className="event-agenda-list__group"
          ref={(el) => setDayRef(group.day, el)}
        >
          <div className="event-agenda-list__date-header">
            <span className="event-agenda-list__date-label">{group.label}</span>
            <span className="event-agenda-list__event-count">
              {group.events.length} {group.events.length === 1
                ? t('eventCalendarPage.eventSingular')
                : t('eventCalendarPage.eventPlural')}
            </span>
          </div>
          <div className="event-agenda-list__events">
            {group.events.map((event) => (
              <EventCard key={event.id} event={event} />
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
