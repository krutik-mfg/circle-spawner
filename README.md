# Circle Spawner

A Unity-based game with a Node.js/TypeScript backend. Players can log in as guests or create accounts, spawn circles, collect coins, and compete on a global leaderboard.

## Project Structure

```
circle-spawner/
├── game-client/          # Unity game client (C#)
│   └── game-client/      # Unity project root
│       ├── Assets/
│       │   ├── Scripts/
│       │   │   ├── API/          # Backend communication (ApiManager)
│       │   │   ├── Game/         # Core gameplay (CoinSpawner, GameManager, etc.)
│       │   │   ├── Models/       # API request/response models
│       │   │   └── UI/           # Login screen & HUD
│       │   └── Scenes/
│       ├── Packages/
│       └── ProjectSettings/
└── game-backend/         # Express + TypeScript REST API
    └── src/
        ├── config/       # Couchbase connection
        ├── controllers/  # Auth & game logic
        ├── middleware/   # JWT auth
        ├── models/       # Player model
        ├── routes/       # API routes
        └── index.ts      # Entry point
```

## Backend

### Tech Stack
- **Runtime:** Node.js + TypeScript
- **Framework:** Express 5
- **Database:** Couchbase
- **Auth:** JWT

### Setup

```bash
cd game-backend
npm install
```

Create a `.env` file:

```env
PORT=3000
CB_CONNECTION_STRING=couchbase://localhost
CB_USERNAME=Administrator
CB_PASSWORD=your_password
CB_BUCKET_NAME=game_db
JWT_SECRET=your_secret
JWT_EXPIRES_IN=30d
```

### Running

```bash
# Development
npm run dev

# Production
npm run build
npm start
```

### API Endpoints

**Auth**
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/auth/guest` | Create anonymous account |
| POST | `/auth/register` | Register with username/password |
| POST | `/auth/login` | Sign in |

**Game** *(JWT required except leaderboard)*
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/game/leaderboard` | Top scores (`?limit=10`) |
| POST | `/game/start` | Start a game session |
| POST | `/game/score` | Submit score |
| GET | `/game/rank` | My current rank |
| GET | `/game/history` | My score history (`?limit=10`) |
| GET | `/game/me` | My profile |

**Health**
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Server health check |

## Game Client

Built with Unity. Open `game-client/game-client` in Unity Hub.

**Requirements:** Unity 2022+ (URP)

The client connects to the backend for authentication, score submission, and leaderboard display.
