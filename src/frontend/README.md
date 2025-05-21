# MyLeague App - Frontend

This is the frontend application for MyLeague, a sports league management platform.

## Project Structure

The project follows a well-organized folder structure:

```
src/
  ├── assets/              # Static assets (images, fonts, icons)
  ├── components/          # Reusable UI components
  │   ├── Navigation/      # Navigation components
  │   ├── HeroSection/     # Hero section components
  │   └── MatchSidebar/    # Match sidebar components
  ├── pages/               # Route-level components
  │   └── HomePage/        # Home page component
  ├── hooks/               # Custom React hooks
  ├── utils/               # Shared utility functions
  ├── context/             # React context providers
  ├── store/               # State management setup
  ├── styles/              # Global styles and themes
  ├── api/                 # Backend communication logic
  ├── constants/           # Project-wide constants
  └── types/               # Global TypeScript definitions
```

## Component Structure

Each component typically follows this structure:
- `ComponentName.tsx` - The main component file
- `ComponentName.css` - Component-specific styles
- `index.ts` - Barrel file for cleaner imports
- `ComponentName.test.tsx` - Tests for the component (when applicable)

## Naming Conventions

- **Components**: PascalCase (e.g., `UserCard.tsx`)
- **Functions/hooks**: camelCase (e.g., `useFetch.ts`)
- **Files**: camelCase (e.g., `matchService.ts`)
- **CSS classes**: kebab-case (e.g., `team-logo`)

## Getting Started

1. Install dependencies:
   ```
   npm install
   ```

2. Start development server:
   ```
   npm run dev
   ```

## Features

- Navigation menu with dropdown capabilities
- Hero section with call-to-action
- Match sidebar displaying upcoming games and standings
- Responsive design for various screen sizes

## Technologies Used

- React
- TypeScript
- CSS (with potential for CSS modules or a CSS-in-JS solution)
- Modern JavaScript features and patterns
