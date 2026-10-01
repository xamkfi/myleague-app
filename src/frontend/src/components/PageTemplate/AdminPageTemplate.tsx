import type { ReactNode } from 'react';
import { useEffect, useState, useCallback, useRef } from 'react';
import { createPortal } from 'react-dom';
import { useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import AdminNavBar from '../Navigation/AdminNavBar';
import AdminBackButton from '../AdminBackButton/AdminBackButton';
import { rememberAdminLocation } from '../../utils/adminReturnTo';
import { InProgressMatchesProvider } from '../../hooks/InProgressMatchesProvider';
import { InProgressFootballMatchesProvider } from '../../hooks/InProgressFootballMatchesProvider';
import { InProgressHockeyMatchesProvider } from '../../hooks/InProgressHockeyMatchesProvider';
import type { SportKind } from '../../utils/sportRoutes';
import './AdminPageTemplate.scss';

function contentSport(pathname: string): SportKind | null {
  if (pathname.startsWith('/admin/floorball')) {
    return 'floorball';
  }
  if (pathname.startsWith('/admin/football')) {
    return 'football';
  }
  if (pathname.startsWith('/admin/hockey')) {
    return 'hockey';
  }
  return null;
}

const SIDEBAR_COLLAPSED_KEY = 'admin-sidebar-collapsed';
const NARROW_LAYOUT_QUERY = '(max-width: 768px)';
const MOBILE_SIDEBAR_PX = 250;
const SWIPE_EDGE_PX = 28;
const SWIPE_LOCK_PX = 10;
const SWIPE_OPEN_RATIO = 0.4;
const SWIPE_FLICK_PX_PER_MS = 0.5;

interface DrawerDrag {
  pointerId: number;
  startX: number;
  startY: number;
  lastX: number;
  lastTime: number;
  velocity: number;
  origin: number;
  axis: 'pending' | 'x' | 'y';
}

interface AdminPageTemplateProps {
  title: string;
  children?: ReactNode;
}

function AdminPageTemplate({ title, children }: AdminPageTemplateProps) {
  const { t } = useTranslation();
  const location = useLocation();
  const sport = contentSport(location.pathname);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(() => {
    try {
      return localStorage.getItem(SIDEBAR_COLLAPSED_KEY) === 'true';
    } catch {
      return false;
    }
  });
  const [isNarrow, setIsNarrow] = useState(() => window.matchMedia(NARROW_LAYOUT_QUERY).matches);
  const [mobileNavOpen, setMobileNavOpen] = useState(false);
  const [drawerDragging, setDrawerDragging] = useState(false);
  const mobileNavOpenRef = useRef(mobileNavOpen);
  mobileNavOpenRef.current = mobileNavOpen;
  const drawerVisibleRef = useRef(0);

  const handleToggleSidebar = useCallback(() => {
    setSidebarCollapsed(prev => {
      const next = !prev;
      try { localStorage.setItem(SIDEBAR_COLLAPSED_KEY, String(next)); } catch { /* noop */ }
      return next;
    });
  }, []);

  useEffect(() => {
    const media = window.matchMedia(NARROW_LAYOUT_QUERY);
    const handleChange = () => {
      setIsNarrow(media.matches);
      if (!media.matches) {
        setMobileNavOpen(false);
      }
    };
    media.addEventListener('change', handleChange);
    return () => media.removeEventListener('change', handleChange);
  }, []);

  useEffect(() => {
    if (!isNarrow) {
      return;
    }

    const root = document.querySelector('.admin-page-container');
    if (!root) {
      return;
    }

    const applyLabels = () => {
      root.querySelectorAll<HTMLTableElement>('table.admin-table').forEach((table) => {
        const headerRow = table.tHead?.rows[0];
        if (!headerRow) {
          return;
        }

        const headers = Array.from(headerRow.cells).map((cell) => cell.textContent?.replace(/\s+/g, ' ').trim() ?? '');
        Array.from(table.tBodies).forEach((body) => {
          Array.from(body.rows).forEach((row) => {
            if (row.classList.contains('teams-table__expanded-row')) {
              return;
            }

            Array.from(row.cells).forEach((cell, index) => {
              if (cell.colSpan > 1) {
                return;
              }

              const label = headers[index] ?? '';
              if (label) {
                if (cell.dataset.label !== label) {
                  cell.dataset.label = label;
                }
              } else if (cell.dataset.label) {
                delete cell.dataset.label;
              }
            });
          });
        });
      });
    };

    applyLabels();
    const observer = new MutationObserver(applyLabels);
    observer.observe(root, { childList: true, subtree: true });
    return () => observer.disconnect();
  }, [isNarrow, location.pathname]);

  useEffect(() => {
    document.title = `${title} - MAHL Admin`;
    return () => {
      document.title = 'MAHL';
    };
  }, [title]);

  useEffect(() => {
    rememberAdminLocation(`${location.pathname}${location.search}`);
    setMobileNavOpen(false);
  }, [location.pathname, location.search]);

  const handleToggleMobileNav = useCallback(() => {
    setMobileNavOpen(prev => !prev);
  }, []);

  useEffect(() => {
    if (!isNarrow) {
      return;
    }

    let drag: DrawerDrag | null = null;
    let suppressClick = false;
    let releaseTimer = 0;

    const navbar = (): HTMLElement | null => document.querySelector('.admin-navbar');
    const backdrop = (): HTMLElement | null => document.querySelector('.admin-page-backdrop');

    const paint = (visible: number) => {
      const shown = Math.min(MOBILE_SIDEBAR_PX, Math.max(0, visible));
      drawerVisibleRef.current = shown;
      const bar = navbar();
      if (bar) {
        bar.style.transition = 'none';
        bar.style.transform = `translateX(${shown - MOBILE_SIDEBAR_PX}px)`;
      }
      const shade = backdrop();
      if (shade) {
        shade.style.transition = 'none';
        shade.style.opacity = String(shown / MOBILE_SIDEBAR_PX);
      }
    };

    const releasePaint = () => {
      const bar = navbar();
      if (bar) {
        bar.style.transition = '';
        bar.style.transform = '';
      }
      const shade = backdrop();
      if (shade) {
        shade.style.transition = '';
        shade.style.opacity = '';
      }
    };

    const finish = (open: boolean) => {
      const bar = navbar();
      if (bar) {
        bar.style.transition = 'transform 0.22s ease';
        bar.style.transform = open ? 'translateX(0)' : 'translateX(-100%)';
      }
      const shade = backdrop();
      if (shade) {
        shade.style.transition = 'opacity 0.22s ease';
        shade.style.opacity = open ? '1' : '0';
      }
      setDrawerDragging(false);
      setMobileNavOpen(open);
      window.clearTimeout(releaseTimer);
      releaseTimer = window.setTimeout(releasePaint, 240);
    };

    const onPointerDown = (event: PointerEvent) => {
      if (!event.isPrimary || event.pointerType === 'mouse' || drag) {
        return;
      }
      const target = event.target;
      if (!(target instanceof Element)) {
        return;
      }
      if (target.closest('input, textarea, select, [contenteditable="true"]')) {
        return;
      }

      const open = mobileNavOpenRef.current;
      const fromEdge = event.clientX <= SWIPE_EDGE_PX;
      const onDrawer = Boolean(target.closest('.admin-navbar, .admin-page-backdrop, .admin-page-edge'));
      if (!open && !fromEdge) {
        return;
      }
      if (open && !onDrawer) {
        return;
      }

      drag = {
        pointerId: event.pointerId,
        startX: event.clientX,
        startY: event.clientY,
        lastX: event.clientX,
        lastTime: event.timeStamp,
        velocity: 0,
        origin: open ? MOBILE_SIDEBAR_PX : 0,
        axis: 'pending',
      };
    };

    const onPointerMove = (event: PointerEvent) => {
      if (!drag || event.pointerId !== drag.pointerId) {
        return;
      }

      const dx = event.clientX - drag.startX;
      const dy = event.clientY - drag.startY;
      if (drag.axis === 'pending') {
        if (Math.abs(dx) < SWIPE_LOCK_PX && Math.abs(dy) < SWIPE_LOCK_PX) {
          return;
        }
        drag.axis = Math.abs(dx) > Math.abs(dy) * 1.2 ? 'x' : 'y';
        if (drag.axis === 'y') {
          drag = null;
          return;
        }
        window.clearTimeout(releaseTimer);
        setDrawerDragging(true);
      }

      event.preventDefault();
      const elapsed = event.timeStamp - drag.lastTime;
      if (elapsed > 0) {
        drag.velocity = (event.clientX - drag.lastX) / elapsed;
      }
      drag.lastX = event.clientX;
      drag.lastTime = event.timeStamp;
      paint(drag.origin + dx);
    };

    const onPointerUp = (event: PointerEvent) => {
      if (!drag || event.pointerId !== drag.pointerId) {
        return;
      }
      const current = drag;
      drag = null;
      if (current.axis !== 'x') {
        return;
      }

      suppressClick = true;
      const dx = event.clientX - current.startX;
      const visible = Math.min(MOBILE_SIDEBAR_PX, Math.max(0, current.origin + dx));
      const flickOpen = current.velocity > SWIPE_FLICK_PX_PER_MS;
      const flickClose = current.velocity < -SWIPE_FLICK_PX_PER_MS;
      finish(flickOpen || (!flickClose && visible > MOBILE_SIDEBAR_PX * SWIPE_OPEN_RATIO));
    };

    const onClickCapture = (event: MouseEvent) => {
      if (!suppressClick) {
        return;
      }
      suppressClick = false;
      event.preventDefault();
      event.stopPropagation();
    };

    document.addEventListener('pointerdown', onPointerDown);
    document.addEventListener('pointermove', onPointerMove, { passive: false });
    document.addEventListener('pointerup', onPointerUp);
    document.addEventListener('pointercancel', onPointerUp);
    document.addEventListener('click', onClickCapture, true);
    return () => {
      document.removeEventListener('pointerdown', onPointerDown);
      document.removeEventListener('pointermove', onPointerMove);
      document.removeEventListener('pointerup', onPointerUp);
      document.removeEventListener('pointercancel', onPointerUp);
      document.removeEventListener('click', onClickCapture, true);
      window.clearTimeout(releaseTimer);
      releasePaint();
    };
  }, [isNarrow]);

  useEffect(() => {
    if (!drawerDragging) {
      return;
    }
    const shade = document.querySelector<HTMLElement>('.admin-page-backdrop');
    if (!shade) {
      return;
    }
    shade.style.transition = 'none';
    shade.style.opacity = String(drawerVisibleRef.current / MOBILE_SIDEBAR_PX);
  }, [drawerDragging]);

  return (
    <InProgressMatchesProvider>
      <InProgressFootballMatchesProvider>
        <InProgressHockeyMatchesProvider>
          <div className={`admin-page-container ${!isNarrow && sidebarCollapsed ? 'admin-page-container--collapsed' : ''} ${mobileNavOpen ? 'admin-page-container--nav-open' : ''} ${drawerDragging ? 'admin-page-container--nav-dragging' : ''}`}>
            <AdminNavBar
              collapsed={isNarrow ? false : sidebarCollapsed}
              mobileOpen={mobileNavOpen}
              onToggleCollapse={handleToggleSidebar}
            />
            {(mobileNavOpen || drawerDragging) && (
              <button
                type="button"
                className="admin-page-backdrop"
                aria-label={t('admin.nav.closeMenu', 'Close menu')}
                onClick={() => setMobileNavOpen(false)}
              />
            )}
            {isNarrow && createPortal(
              <button
                type="button"
                className={`admin-page-edge${mobileNavOpen ? ' admin-page-edge--open' : ''}`}
                aria-label={mobileNavOpen
                  ? t('admin.nav.closeMenu', 'Close menu')
                  : t('admin.nav.openMenu', 'Menu')}
                onClick={handleToggleMobileNav}
              >
                <span className="admin-page-edge__tab" aria-hidden="true">
                  {mobileNavOpen ? (
                    <span className="admin-page-edge__close">×</span>
                  ) : (
                    <span className="admin-page-edge__dots">
                      <span />
                      <span />
                      <span />
                    </span>
                  )}
                </span>
              </button>,
              document.body,
            )}
            <div className="admin-page-content">
              <button
                type="button"
                className="admin-page-menu-button"
                aria-expanded={mobileNavOpen}
                aria-label={mobileNavOpen
                  ? t('admin.nav.closeMenu', 'Close menu')
                  : t('admin.nav.openMenu', 'Menu')}
                onClick={handleToggleMobileNav}
              >
                <span className="admin-page-menu-button__bars" aria-hidden="true" />
                {mobileNavOpen
                  ? t('admin.nav.closeMenu', 'Close menu')
                  : t('admin.nav.openMenu', 'Menu')}
              </button>
              <div className="admin-page-body" data-sport={sport ?? undefined}>
                <AdminBackButton />
                {children || (
                  <p className="placeholder-text">This admin page is under construction.</p>
                )}
              </div>
            </div>
          </div>
        </InProgressHockeyMatchesProvider>
      </InProgressFootballMatchesProvider>
    </InProgressMatchesProvider>
  );
}

export default AdminPageTemplate;

