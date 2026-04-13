import { v4 as uuidv4 } from 'uuid';
import bcrypt from 'bcryptjs';
import jwt from 'jsonwebtoken';
import { getCollection, getCluster } from '../config/couchbase';
import { Player, JwtPayload } from '../models/player';

// ── Helper: sign a JWT for a player ──────────────────────────────────────────
function signToken(player: Player): string {
  const payload: JwtPayload = {
    guestId: player.guestId,
    username: player.username,
    isGuest: player.isGuest,
  };
  return jwt.sign(payload, process.env.JWT_SECRET!, {
    expiresIn: (process.env.JWT_EXPIRES_IN || '30d') as any,
  });
}

// ── Guest login ───────────────────────────────────────────────────────────────
// Creates an anonymous account with an auto-generated username.
export async function createGuestPlayer(): Promise<{ guestId: string; username: string; token: string }> {
  const collection = getCollection();
  const guestId = uuidv4();
  const username = `Guest_${guestId.slice(0, 6).toUpperCase()}`;
  const now = new Date().toISOString();

  const player: Player = {
    type: 'player',
    guestId,
    username,
    isGuest: true,
    highScore: 0,
    totalGames: 0,
    totalScore: 0,
    createdAt: now,
    updatedAt: now,
  };

  await collection.insert(`player::${guestId}`, player);
  return { guestId, username, token: signToken(player) };
}

// ── Register with username + password ────────────────────────────────────────
export async function registerPlayer(
  username: string,
  password: string
): Promise<{ guestId: string; username: string; token: string }> {
  const collection = getCollection();
  const cluster = getCluster();
  const bucket = process.env.CB_BUCKET_NAME!;

  // Check if username is already taken
  const check = await cluster.query(
    `SELECT guestId FROM \`${bucket}\` WHERE type='player' AND username=$1 LIMIT 1`,
    { parameters: [username] }
  );
  if (check.rows.length > 0) {
    throw new Error('USERNAME_TAKEN');
  }

  const guestId = uuidv4();
  const now = new Date().toISOString();
  const passwordHash = await bcrypt.hash(password, 12); // 12 salt rounds = secure enough for a game

  const player: Player = {
    type: 'player',
    guestId,
    username,
    passwordHash,
    isGuest: false,
    highScore: 0,
    totalGames: 0,
    totalScore: 0,
    createdAt: now,
    updatedAt: now,
  };

  await collection.insert(`player::${guestId}`, player);
  return { guestId, username, token: signToken(player) };
}

// ── Login with username + password ───────────────────────────────────────────
export async function loginPlayer(
  username: string,
  password: string
): Promise<{ guestId: string; username: string; token: string }> {
  const cluster = getCluster();
  const bucket = process.env.CB_BUCKET_NAME!;

  // Find player by username
  const result = await cluster.query(
    `SELECT * FROM \`${bucket}\` WHERE type='player' AND username=$1 LIMIT 1`,
    { parameters: [username] }
  );

  if (result.rows.length === 0) {
    throw new Error('INVALID_CREDENTIALS');
  }

  // Couchbase wraps row under the bucket name
  const player: Player = result.rows[0][bucket] ?? result.rows[0];

  if (!player.passwordHash) {
    // This is a guest account — cannot log in with a password
    throw new Error('INVALID_CREDENTIALS');
  }

  const match = await bcrypt.compare(password, player.passwordHash);
  if (!match) {
    throw new Error('INVALID_CREDENTIALS');
  }

  return { guestId: player.guestId, username: player.username, token: signToken(player) };
}

// ── Fetch a player by their guestId ──────────────────────────────────────────
export async function getPlayer(guestId: string): Promise<Player | null> {
  const collection = getCollection();
  try {
    const result = await collection.get(`player::${guestId}`);
    return result.content as Player;
  } catch {
    return null;
  }
}
