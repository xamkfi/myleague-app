# MyLeague App - Frontend

This is the frontend application for MyLeague, a sports league management platform built with React, TypeScript, and Vite.

## Technologies

- **React 18** with TypeScript
- **Vite** - Fast build tool and dev server
- **TailwindCSS 4** - Utility-first CSS framework
- **SCSS** - Component-scoped styles
- **React Router 7** - Client-side routing
- **i18next** - Internationalization (EN / FI)
- **SignalR** - Real-time match updates
- **pnpm** - Package manager

## Project Structure

```
src/
  ├── api/                 # Backend API service layer
  ├── assets/              # Static assets (images, fonts, icons)
  ├── components/          # Reusable UI components
  ├── constants/           # Project-wide constants
  ├── context/             # React context providers
  ├── hooks/               # Custom React hooks
  ├── i18n/                # Internationalization (locales, config)
  ├── pages/               # Route-level page components
  │   ├── AdminPage/       # Admin panel pages
  │   └── ...              # Public-facing pages
  ├── router/              # Route definitions
  ├── services/            # External services (SignalR, etc.)
  ├── styles/              # Global styles and themes
  ├── types/               # Global TypeScript type definitions
  └── utils/               # Shared utility functions
```

## Getting Started

### Prerequisites

- **Node.js 18+**
- **pnpm** (install with `npm install -g pnpm`)
- Backend API running (see [root README](../../README.md) for full setup)

### Install & Run

1. **Install dependencies:**
   ```bash
   cd src/frontend
   pnpm install
   ```

2. **Start development server:**
   ```bash
   pnpm dev
   ```

3. **Access the application:**
   - Frontend: http://localhost:5173

> **Note:** The backend API and database must be running for the frontend to function. Make sure to also run the [Seeder](../tools/Seeder/README.md) to populate the database with initial test data.

### Other Commands

```bash
# Build for production
pnpm build

# Run linting
pnpm lint

# Preview production build
pnpm preview
```

## Naming Conventions

- **Components**: PascalCase (e.g., `UserCard.tsx`)
- **Hooks**: camelCase with `use` prefix (e.g., `useFetch.ts`)
- **Services/utils**: camelCase (e.g., `matchService.ts`)
- **CSS classes**: kebab-case (e.g., `team-logo`)
- **Types**: PascalCase (e.g., `FloorballMatchDto`)

## Key Features

- Admin dashboard for managing clubs, teams, players, referees, seasons, and matches
- Live match management with real-time updates via SignalR
- Match overview with filtering, search, and status-based views
- Internationalization (English and Finnish)
- Responsive design
