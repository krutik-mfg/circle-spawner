// ── Player (one doc per user) ─────────────────────────────────────────────────
export interface Player {
  type: 'player';
  guestId: string;          // internal UUID — used as document key
  username: string;
  passwordHash?: string;    // undefined for guest accounts
  isGuest: boolean;
  highScore: number;
  totalGames: number;
  totalScore: number;       // sum of every submitted score
  createdAt: string;        // ISO timestamp
  updatedAt: string;
}

// ── Game Session (one doc per play-through) ───────────────────────────────────
export interface GameSession {
  type: 'session';
  sessionId: string;
  guestId: string;
  score: number;
  level: number;            // highest level reached
  status: 'active' | 'completed';
  startedAt: string;
  endedAt?: string;
  duration?: number;        // seconds played
}

// ── Leaderboard entry returned to clients ─────────────────────────────────────
export interface LeaderboardEntry {
  rank: number;
  username: string;
  highScore: number;
  totalGames: number;
}

// ── JWT payload shape ─────────────────────────────────────────────────────────
export interface JwtPayload {
  guestId: string;
  username: string;
  isGuest: boolean;
}
