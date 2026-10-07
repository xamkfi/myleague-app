---
paths:
  - "src/frontend/**"
---

# Frontend

React 18 + TypeScript + Vite. Write function components with **explicit props types**. Do not use `React.FC`, and do not use `any`. Colocate a page with its `components/` folder and `*.scss`. Public pages for one sport go under `pages/{floorball,football,hockey}/`, the same split as `api/` and `types/`.

| Kind | Convention | Example |
|------|------------|---------|
| Pages / components | PascalCase | `ClubsPage.tsx` |
| Hooks | `use` + camelCase | `useFetch.ts` |
| API modules | camelCase `*Service` | `clubService.ts` |
| CSS | BEM-ish kebab | `.clubs-page__hero` |
| Types | PascalCase | `FloorballMatchDto` |

## Data and auth

- HTTP goes through `authFetch` from `src/api/utils/authFetch.ts`, which injects the JWT and retries once after a 401 refresh
- Parse failures with `parseErrorResponse` from `src/api/utils/ParseErrorResponse.tsx`
- Types live in `src/types/` or next to the service
- API modules are grouped by area: `src/api/{admin,auth,clubAdmin,common,floorball,football,hockey,news}`
- Floorball, football, and ice hockey are all public (`/sports/{floorball,football,icehockey}`). Keep the three in step

## i18n and routing

The UI ships in Finnish and English. The language comes from localStorage, then the browser, and falls back to `en` (`src/i18n/i18n.ts`). Add every key to both `src/i18n/locales/fi/translation.json` and `src/i18n/locales/en/translation.json`. Use `useTranslation()`. JSX must not contain user-facing English or Finnish literals.

Register screens in `src/router/routes.tsx` with `lazyWithRetry(() => import('...'))`. Wrap admin and club-admin routes in `ProtectedRoute`.

## UI

Reuse `PageTemplate`, `LoadingSpinner`, and the existing admin tables and modals. Styling is SCSS-first: each component has its own `.scss` with BEM-ish class names. Tailwind 4 is installed through the Vite plugin (there is no `tailwind.config`), but it is barely used, so do not mix utility classes into SCSS components.

Use `pnpm` only. Ignore the stray `package-lock.json`. Before finishing, run `pnpm lint` and `pnpm build`; `build` also runs `tsc`. There is no frontend test runner.
