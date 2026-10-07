# Frontend

React single-page app for MyLeague. It has three areas:

- **Public site**: floorball, football, and ice hockey (`/sports/floorball`, `/sports/football`, `/sports/icehockey`), plus clubs, news, event calendar, rules, age groups, MAHL info pages, registration, and all-time statistics.
- **Admin** (`/admin`, role `SystemAdmin`): clubs, divisions, persons, users, site settings, news, rules, info pages, footer contacts, data subject rights, and teams, players, officials, seasons, tournaments, and live match control for all three sports.
- **Club admin** (`/club-admin`, roles `ClubAdmin` or `SystemAdmin`): club info, team rosters, and match-day rosters for any sport.

The three sports are peers. When you add a feature to one, check the other two.

Conventions are in [.claude/rules/frontend.md](../../.claude/rules/frontend.md). This README covers setup and where things live.

## Stack

| Part | Version / note |
|------|----------------|
| React | 18.3 |
| TypeScript | 5.8, `strict` |
| Vite | 6.4, with `@vitejs/plugin-react` |
| Routing | React Router 7 (`createBrowserRouter`) |
| Styling | SCSS per component. Tailwind 4 is loaded through `@tailwindcss/vite` (no `tailwind.config`) but is barely used |
| i18n | i18next + react-i18next, `fi` and `en` |
| Live updates | `@microsoft/signalr` 8 |
| Rich text | Quill / react-quill, sanitized with DOMPurify |
| Excel import | exceljs |
| Package manager | pnpm 10.11 (pinned in `packageManager`) |

## Prerequisites

- Node.js 22 (the version CI uses in [frontend-ci.yaml](../../.github/workflows/frontend-ci.yaml))
- pnpm 10. With Corepack: `corepack enable`. Use pnpm only. Ignore the stray `package-lock.json` in this folder.
- A running API on `http://localhost:8080`. See [.claude/skills/run-local/SKILL.md](../../.claude/skills/run-local/SKILL.md).

## Quick start

Start PostgreSQL and Seq in Docker and the API on the host first (see the run-local skill). Then:

```bash
cd src/frontend
pnpm install
pnpm dev
```

Open http://localhost:5173. Run the UI on the host with Vite. Do not use the Compose `frontend` service for local testing.

If lists are empty, seed the database with the [Seeder](../tools/Seeder/README.md):

```bash
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=all
```

Seeded dev logins: `test@myleague.local` (site admin) and `clubadmin@myleague.local` (club admin). In Development the API returns the login code in the response, so no email is needed.

## Scripts

| Command | What it does |
|---------|--------------|
| `pnpm dev` | Vite dev server on port 5173 (binds `0.0.0.0`) |
| `pnpm build` | `tsc -p tsconfig.app.json --noEmit`, then `vite build` into `dist/` |
| `pnpm lint` | `eslint .` |
| `pnpm preview` | Serves the built `dist/` locally |

There is no test runner. Before you finish a change, run `pnpm lint` and `pnpm build`.

## Environment

Vite reads `VITE_*` keys from these files:

| File | Committed | Key | Use |
|------|-----------|-----|-----|
| `.env.development` | yes | `VITE_API_URL` | `pnpm dev`. Keep it at `http://localhost:8080/api` |
| `.env.production` | yes | `VITE_API_URL` | `pnpm build`. The deploy workflows overwrite this file before they build |

`src/constants/config.ts` exports `API_URL`. It strips trailing slashes and falls back to `/api` when `VITE_API_URL` is empty. With the fallback, the Vite dev server proxies `/api` to `http://localhost:8080` (or `http://webapi:8080` when the `DOCKER` env var is set).

Do not change `.env.development` to the launchSettings ports (`65532` / `65533`).

A local `.env` may also exist with Azure keys for scripts. Vite ignores keys without the `VITE_` prefix.

## Folder map

```
src/
├── main.tsx, App.tsx      entry; App wraps the router in AuthProvider and AudienceProvider
├── api/                   HTTP clients, grouped by area
│   ├── admin/ auth/ clubAdmin/ common/ news/
│   ├── floorball/ football/ hockey/
│   ├── utils/             authFetch.ts, tokenManager.ts, ParseErrorResponse.tsx, isNotFoundError.ts
│   └── version/
├── assets/                images, icons, logos
├── audience/              adult / youth / women theme registry
├── components/            shared UI (PageTemplate, LoadingSpinner, ProtectedRoute, MatchTimer, admin/ ...)
├── constants/             config.ts (API_URL), sports, notification constants
├── context/               AuthContext.tsx, AudienceContext.tsx
├── functions/             ResizeImage.tsx
├── hooks/                 useMatchData, useMatchTimer, useIntervalWhen, in-progress match providers ...
├── i18n/                  i18n.ts, locales/fi/translation.json, locales/en/translation.json
├── pages/                 route-level screens; AdminPage/ and ClubAdminPage/ hold the admin areas
├── router/                routes.tsx, SuspenseWrapper.tsx
├── services/              signalRService.ts and content helpers
├── styles/                variables.scss, common.scss, themes, AdminTable.scss
├── types/                 shared DTO types per area
└── utils/                 lazyWithRetry.ts, route and display helpers
```

Import aliases (from `vite.config.ts`): `@variables` → `src/styles/variables.scss`, `@common` → `src/styles/common.css`, `@components` → `src/components`.

`__APP_VERSION__` is defined at build time as `yyyy-MM-dd.<git short sha>`.

## Adding a page

1. Create `src/pages/<Name>Page/<Name>Page.tsx` and a `<Name>Page.scss` next to it. Put page-only components in a `components/` folder beside it.
2. Write a function component with an explicit props type. Do not use `React.FC` or `any`.
3. Add an API module under `src/api/<area>/` that calls `authFetch`, and add types under `src/types/`.
4. Register the route in `src/router/routes.tsx` with `lazyWithRetry(() => import('...'))` inside `SuspenseWrapper`. Wrap admin routes in `<ProtectedRoute>` (defaults to `SystemAdmin`). For club-admin routes, pass `allowedRoles={['ClubAdmin', 'SystemAdmin']}` and `loginPath="/club-admin/login"`.
5. Add every user-facing string to both locale files.
6. Run `pnpm lint` and `pnpm build`.

For a full vertical slice (backend and frontend), follow [.claude/skills/create-feature/SKILL.md](../../.claude/skills/create-feature/SKILL.md).

`lazyWithRetry` reloads the page once if a lazy chunk fails to load. This happens after a deploy changes the chunk hashes.

## i18n

- Translations are in `src/i18n/locales/fi/translation.json` and `src/i18n/locales/en/translation.json`. Add every key to both.
- Use `const { t } = useTranslation()`. JSX must not contain Finnish or English literals.
- Finnish is the primary language. i18next picks the language from `localStorage` first, then the browser. If neither matches, it falls back to `en`. The `LanguageToggle` component switches between `fi` and `en`.

## API calls and auth

- Call the API with `authFetch` from `src/api/utils/authFetch.ts`. It adds `Authorization: Bearer <token>` when a session exists. It refreshes the token before the request if it is close to expiry, and retries once after a refresh if the server answers 401. Without a session it behaves like `fetch`.
- Read error bodies with `parseErrorResponse` (or `unwrapApiErrorMessage`) from `src/api/utils/ParseErrorResponse.tsx`.
- Sign-in is passwordless. The user enters an email (`POST /api/Auth/login`), receives a code, and verifies it. The API returns an access token and a refresh token.
- `src/api/utils/tokenManager.ts` stores tokens in `localStorage` under `myleague_auth_tokens`. It rotates the refresh token and uses the Web Locks API so two tabs do not refresh at the same time. `SessionExpiryWarning` warns the user before the session ends.
- `AuthContext` exposes the user and role. `ProtectedRoute` redirects to the login page when there is no session, and sends a user with the wrong role to their own area.

## Live updates

- `src/services/signalRService.ts` connects to the hub at `/api/hubs/domainevent`. It passes the JWT through `accessTokenFactory` and reconnects automatically.
- In dev the hub URL is hard-coded to `http://localhost:8080/api/hubs/domainevent`. In a build it is derived from `API_URL`.
- Floorball and football match pages, and their admin match pages, subscribe to SignalR.
- Ice hockey pages poll REST instead. The match page polls every 3 s while live and every 30 s while upcoming. League, team, tournament, and admin match pages poll every 4 s while a match is live.
- Match timer state lives in the API's memory, so the API runs as a single instance.

## Build and deploy

- `pnpm build` writes `dist/`. `public/staticwebapp.config.json` sets the SPA fallback to `index.html` and cache headers for Azure Static Web Apps.
- Azure Static Web Apps hosts the frontend. The workflows build with `VITE_API_URL` set for the target environment and upload `dist/`. See [infra/README.md](../../infra/README.md).
- `Dockerfile` runs the Vite dev server on port 5173. It exists only for the CI container check and the Compose `frontend` service. It is not a production image, and there is no nginx config.

## Pre-commit hook

The repo root has a husky pre-commit hook that runs lint-staged. lint-staged passes staged `src/frontend/**/*.{ts,tsx}` files to `lint-frontend.mjs`, which runs ESLint with `--max-warnings 0`. A warning that `pnpm lint` allows will still block the commit. To install the hook, run `npm install` once at the repo root.

## Related

- [Root README](../../README.md)
- [WebAPI README](../backend/WebAPI/README.md): endpoints
- [infra/README.md](../../infra/README.md): Azure resources and deploy workflows
