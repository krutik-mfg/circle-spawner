import { v4 as uuidv4 } from 'uuid';
import { getCollection, getCluster } from '../config/couchbase';
import { GameSession, Player, LeaderboardEntry } from '../models/player';

const BUCKET = () => process.env.CB_BUCKET_NAME!;

// ── Start a new game session ──────────────────────────────────────────────────
export async function startSession(guestId: string): Promise<GameSession> {
  const collection = getCollection();
  const sessionId = uuidv4();

  const session: GameSession = {
    type: 'session',
    sessionId,
    guestId,
    score: 0,
    level: 1,
    status: 'active',
    startedAt: new Date().toISOString(),
  };

  await collection.insert(`session::${sessionId}`, session);
  return session;
}

// ── Submit score when a game ends ─────────────────────────────────────────────
// score     — final score
// level     — highest level reached
// Returns the player's updated highScore + whether this is a new high score
export async function submitScore(
  sessionId: string,
  guestId: string,
  score: number,
  level: number = 1
): Promise<{ highScore: number; isNewHighScore: boolean; totalGames: number }> {
  const collection = getCollection();
  const now = new Date().toISOString();

  // 1. Get the session to calculate duration
  let startedAt: string | undefined;
  try {
    const sessionDoc = await collection.get(`session::${sessionId}`);
    startedAt = (sessionDoc.content as GameSession).startedAt;
  } catch {
    // session not found — still accept the score submission
  }

  const duration = startedAt
    ? Math.round((Date.now() - new Date(startedAt).getTime()) / 1000)
    : undefined;

  // 2. Mark session as completed
  const completedSession: GameSession = {
    type: 'session',
    sessionId,
    guestId,
    score,
    level,
    status: 'completed',
    startedAt: startedAt ?? now,
    endedAt: now,
    duration,
  };
  await collection.upsert(`session::${sessionId}`, completedSession);

  // 3. Update player stats
  const playerResult = await collection.get(`player::${guestId}`);
  const player = playerResult.content as Player;

  const isNewHighScore = score > player.highScore;
  const highScore = Math.max(player.highScore, score);
  const totalGames = player.totalGames + 1;
  const totalScore = (player.totalScore ?? 0) + score;

  await collection.upsert(`player::${guestId}`, {
    ...player,
    highScore,
    totalGames,
    totalScore,
    updatedAt: now,
  } as Player);

  return { highScore, isNewHighScore, totalGames };
}

// ── Leaderboard: top N players by high score ──────────────────────────────────
export async function getLeaderboard(limit: number = 10): Promise<LeaderboardEntry[]> {
  const cluster = getCluster();

  const result = await cluster.query(
    `SELECT username, highScore, totalGames
     FROM \`${BUCKET()}\`
     WHERE type = 'player' AND highScore > 0
     ORDER BY highScore DESC
     LIMIT $1`,
    { parameters: [limit] }
  );

  return result.rows.map((row: any, index: number) => ({
    rank: index + 1,
    username: row.username,
    highScore: row.highScore,
    totalGames: row.totalGames ?? 0,
  }));
}

// ── Get a single player's rank on the leaderboard ────────────────────────────
export async function getPlayerRank(guestId: string): Promise<{ rank: number; highScore: number } | null> {
  const cluster = getCluster();

  // Count how many players have a higher score
  const result = await cluster.query(
    `SELECT COUNT(*) AS aboveCount
     FROM \`${BUCKET()}\`
     WHERE type = 'player' AND highScore > (
       SELECT RAW highScore FROM \`${BUCKET()}\` USE KEYS 'player::' || $1
     )[0]`,
    { parameters: [guestId] }
  );

  // Also fetch the player's own score
  const playerResult = await cluster.query(
    `SELECT highScore FROM \`${BUCKET()}\` USE KEYS 'player::' || $1`,
    { parameters: [guestId] }
  );

  if (playerResult.rows.length === 0) return null;

  const aboveCount: number = result.rows[0]?.aboveCount ?? 0;
  const highScore: number = playerResult.rows[0]?.highScore ?? 0;

  return { rank: aboveCount + 1, highScore };
}

// ── Get a player's last N game sessions ───────────────────────────────────────
export async function getScoreHistory(
  guestId: string,
  limit: number = 10
): Promise<{ sessionId: string; score: number; level: number; endedAt: string; duration?: number }[]> {
  const cluster = getCluster();

  const result = await cluster.query(
    `SELECT sessionId, score, level, endedAt, duration
     FROM \`${BUCKET()}\`
     WHERE type = 'session' AND guestId = $1 AND status = 'completed'
     ORDER BY endedAt DESC
     LIMIT $2`,
    { parameters: [guestId, limit] }
  );

  return result.rows;
}

// ── Get full player profile + stats ──────────────────────────────────────────
export async function getPlayerProfile(
  guestId: string
): Promise<{ username: string; highScore: number; totalGames: number; totalScore: number; isGuest: boolean; createdAt: string } | null> {
  const collection = getCollection();
  try {
    const result = await collection.get(`player::${guestId}`);
    const p = result.content as Player;
    return {
      username: p.username,
      highScore: p.highScore,
      totalGames: p.totalGames,
      totalScore: p.totalScore ?? 0,
      isGuest: p.isGuest,
      createdAt: p.createdAt,
    };
  } catch {
    return null;
  }
}
