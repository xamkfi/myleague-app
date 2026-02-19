import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import type { CalendarEvent } from '../../../types/calendar';
import './MiniCalendar.scss';

const WEEKDAY_LABELS = ['Ma', 'Ti', 'Ke', 'To', 'Pe', 'La', 'Su'];

interface MiniCalendarProps {
  year: number;
  month: number;
  events: CalendarEvent[];
  selectedDay: number | null;
  onSelectDay: (day: number | null) => void;
  onPrevMonth: () => void;
  onNextMonth: () => void;
  onToday: () => void;
}

function getMonthGrid(year: number, month: number): (number | null)[] {
  const first = new Date(year, month - 1, 1);
  const daysInMonth = new Date(year, month, 0).getDate();
  const firstWeekday = (first.getDay() + 6) % 7;
  const cells: (number | null)[] = [];
  for (let i = 0; i < firstWeekday; i++) cells.push(null);
  for (let d = 1; d <= daysInMonth; d++) cells.push(d);
  return cells;
}

function getDateKey(year: number, month: number, day: number): string {
  return `${year}-${month.toString().padStart(2, '0')}-${day.toString().padStart(2, '0')}`;
}

export default function MiniCalendar({
  year,
  month,
  events,
  selectedDay,
  onSelectDay,
  onPrevMonth,
  onNextMonth,
  onToday,
}: MiniCalendarProps) {
  const { t } = useTranslation();
  const grid = useMemo(() => getMonthGrid(year, month), [year, month]);

  const eventDays = useMemo(() => {
    const days = new Set<string>();
    events.forEach((e) => days.add(e.date));
    return days;
  }, [events]);

  const today = new Date();
  const isCurrentMonth = today.getFullYear() === year && today.getMonth() + 1 === month;
  const todayDay = isCurrentMonth ? today.getDate() : null;

  const monthTitle = new Date(year, month - 1, 1).toLocaleDateString(undefined, {
    month: 'long',
    year: 'numeric',
  });

  const handleDayClick = (day: number) => {
    onSelectDay(selectedDay === day ? null : day);
  };

  return (
    <div className="mini-calendar">
      <div className="mini-calendar__header">
        <button
          type="button"
          className="mini-calendar__nav-btn"
          onClick={onPrevMonth}
          aria-label={t('eventCalendarPage.prevMonth')}
        >
          ‹
        </button>
        <span className="mini-calendar__month-title">{monthTitle}</span>
        <button
          type="button"
          className="mini-calendar__nav-btn"
          onClick={onNextMonth}
          aria-label={t('eventCalendarPage.nextMonth')}
        >
          ›
        </button>
      </div>

      <div className="mini-calendar__weekdays">
        {WEEKDAY_LABELS.map((label) => (
          <div key={label} className="mini-calendar__weekday">{label}</div>
        ))}
      </div>

      <div className="mini-calendar__grid">
        {grid.map((day, index) => {
          if (day === null) {
            return <div key={`empty-${index}`} className="mini-calendar__cell mini-calendar__cell--empty" />;
          }

          const dateKey = getDateKey(year, month, day);
          const hasEvents = eventDays.has(dateKey);
          const isToday = day === todayDay;
          const isSelected = day === selectedDay;

          return (
            <button
              key={dateKey}
              type="button"
              className={[
                'mini-calendar__cell',
                isToday && 'mini-calendar__cell--today',
                isSelected && 'mini-calendar__cell--selected',
                hasEvents && 'mini-calendar__cell--has-events',
              ].filter(Boolean).join(' ')}
              onClick={() => handleDayClick(day)}
            >
              <span className="mini-calendar__day-num">{day}</span>
              {hasEvents && <span className="mini-calendar__dot" />}
            </button>
          );
        })}
      </div>

      <button
        type="button"
        className="mini-calendar__today-btn"
        onClick={onToday}
      >
        {t('eventCalendarPage.today')}
      </button>
    </div>
  );
}
