# 🧙 Wizard RPG — Frontend

Vue 3 + TypeScript + Vite single-page application for the Wizard RPG game.

## Tech Stack

- **Vue 3** with `<script setup>` SFCs
- **TypeScript** (strict mode, `erasableSyntaxOnly`)
- **Pinia** for state management
- **Vue Router** for SPA routing with auth guards
- **Axios** for HTTP with JWT interceptor & automatic token refresh
- **Tailwind CSS v4** for styling
- **Playwright** for E2E testing

## Project Structure

```
src/
├── api/            # Axios API client + per-domain modules
│   ├── client.ts   # Base axios instance with JWT interceptor
│   ├── auth.ts     # /api/auth endpoints
│   ├── player.ts   # /api/player endpoints
│   ├── bank.ts     # /api/bank endpoints
│   ├── battle.ts   # /api/battle endpoints
│   ├── broom.ts    # /api/broomgame endpoints
│   ├── fellowship.ts # /api/fellowship endpoints
│   ├── item.ts     # /api/item endpoints
│   └── admin.ts    # /api/admin endpoints
├── components/     # Shared Vue components
│   └── NavBar.vue  # Top navigation with auth-aware links
├── router/         # Vue Router configuration
│   └── index.ts    # Route definitions + auth/admin guards
├── stores/         # Pinia stores
│   ├── auth.ts     # Login, register, logout, JWT management
│   ├── player.ts   # Player profile and leaderboard
│   ├── bank.ts     # Bank account, deposits, withdrawals, items
│   ├── battle.ts   # Battles, spells, turns
│   ├── broom.ts    # Broom leagues and bets
│   ├── fellowship.ts # Fellowship CRUD
│   └── admin.ts    # Admin player/item management
├── types/          # TypeScript type definitions
│   └── index.ts    # All DTOs, enums (as const objects)
├── views/          # Page-level Vue components
│   ├── LoginView.vue
│   ├── RegisterView.vue
│   ├── DashboardView.vue
│   ├── ProfileView.vue
│   ├── BattleView.vue
│   ├── BattleDetailView.vue
│   ├── BankView.vue
│   ├── BroomGameView.vue
│   ├── FellowshipView.vue
│   ├── LeaderboardView.vue
│   ├── ItemsView.vue
│   └── AdminView.vue
├── App.vue         # Root component with NavBar + router-view
├── main.ts         # App bootstrap (Pinia, Router)
└── style.css       # Tailwind CSS import
e2e/                # Playwright E2E tests
├── login.spec.ts
├── register.spec.ts
├── navigation.spec.ts
├── dashboard.spec.ts
├── battle.spec.ts
├── bank.spec.ts
├── fellowship.spec.ts
├── broom.spec.ts
└── admin.spec.ts
```

## Pages & Features

| Route | View | Auth | Description |
|-------|------|------|-------------|
| `/login` | LoginView | Guest | Email + password login form |
| `/register` | RegisterView | Guest | Registration with optional referral code |
| `/dashboard` | DashboardView | ✅ | Player overview, stats, bank summary, quick actions |
| `/profile` | ProfileView | ✅ | View/edit username and email |
| `/battle` | BattleView | ✅ | Challenge wizards, view spells, see pending/active/finished battles |
| `/battle/:id` | BattleDetailView | ✅ | Cast spells, view battle log, see winner |
| `/bank` | BankView | ✅ | Deposit/withdraw gold, view stored items |
| `/broom` | BroomGameView | ✅ | Browse leagues, place bets, view bet history |
| `/fellowship` | FellowshipView | ✅ | Create/join/leave fellowships, view members |
| `/leaderboard` | LeaderboardView | Public | Top wizards by level/XP/gold |
| `/items` | ItemsView | Public | Browse all game items |
| `/admin` | AdminView | Admin | Manage players, items, broom leagues |

## Development

```bash
# Install dependencies
npm install

# Start dev server (port 5173)
npm run dev

# Type check
npx vue-tsc -b

# Build for production
npm run build

# Run E2E tests (starts dev server automatically)
npm run test:e2e

# Run E2E tests with UI
npm run test:e2e:ui
```

## API Configuration

The frontend connects to the backend API via:

- **Production** (Docker): proxied through nginx at `/api/` → `http://backend:8080/api/`
- **Development**: uses `VITE_API_BASE_URL` env var (defaults to `http://localhost:5000`)

## Authentication Flow

1. User registers or logs in → receives JWT `accessToken` + `refreshToken`
2. Tokens stored in `localStorage`, injected via axios interceptor
3. On 401 response, interceptor attempts token refresh automatically
4. On refresh failure, user is redirected to login
5. Router guards prevent unauthenticated access to protected routes
