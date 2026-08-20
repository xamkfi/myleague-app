# Frontend

React SPA for MyLeague: public league pages and an admin console. Full stack setup is in the [root README](../../README.md).

## Stack

- React 18.3 + TypeScript 5.8
- Vite 6.3
- Tailwind CSS 4.1 (Vite plugin; no `tailwind.config.js`) and SCSS
- React Router 7
- i18next (Finnish default, English)
- SignalR (`@microsoft/signalr`) for live match updates
- pnpm 10, Node 22

## Structure

```
src/
  ├── api/                 # HTTP clients per resource
  ├── assets/              # Images, fonts, icons
  ├── audience/            # Adult / youth / women theming
  ├── components/          # Shared UI
  ├── constants/
  ├── context/
  ├── functions/
  ├── hooks/
  ├── i18n/                # locales/fi, locales/en
  ├── pages/               # Route-level screens
  ├── router/
  ├── services/            # SignalR and other non-REST clients
  ├── styles/
  ├── types/
  └── utils/
```

Public pages cover floorball and football (home, clubs, leagues, teams, players, matches, tournaments, news, calendar, rules). Ice hockey is listed on the sports page but is not enabled.

Admin covers clubs, divisions, persons, users, news, rules, info pages, and floorball/football management (including live match control and tournament JSON import). Club admins have a narrower roster / match-day area.

## Run

Prerequisites: Node 22+, pnpm, and a running WebAPI ([root README](../../README.md)).

```bash
cd src/frontend
pnpm install
pnpm dev          # http://localhost:5173
pnpm lint
pnpm build
pnpm preview
```

`/.env.development` sets `VITE_API_URL=http://localhost:8080/api` (Docker). For a local `dotnet run` API, use `http://localhost:65533/api` and restart Vite.

Seed the database with the [Seeder](../tools/Seeder/README.md) so lists and standings have data.

## Conventions

| Kind | Style | Example |
|------|--------|---------|
| Components | PascalCase | `UserCard.tsx` |
| Hooks | `use` + camelCase | `useFetch.ts` |
| Services / utils | camelCase | `matchService.ts` |
| CSS classes | kebab-case | `team-logo` |
| Types | PascalCase | `FloorballMatchDto` |

## Related

- [Root README](../../README.md) — ports, auth, Docker
- [WebAPI](../backend/WebAPI/README.md) — endpoints
- [Azure / CI](../../infra/README.md) — Static Web App deploy
