import { Request, Response } from 'express';
import { AuthRequest } from '../middleware/auth';
import {
  startSession,
  submitScore,
  getLeaderboard,
  getPlayerRank,
  getScoreHistory,
  getPlayerProfile,
} from '../services/gameService';

// POST /game/start — begin a new play session
export async function startGame(req: AuthRequest, res: Response): Promise<void> {
  try {
    const session = await startSession(req.player!.guestId);
    res.status(201).json({ message: 'Game session started', session });
  } catch (error) {
    console.error('Start game error:', error);
    res.status(500).json({ error: 'Failed to start game session' });
  }
}

// POST /game/score — submit final score when a game ends
// Body: { sessionId: string, score: number, level?: number }
export async function submitGameScore(req: AuthRequest, res: Response): Promise<void> {
  try {
    const { sessionId, score, level } = req.body;

    if (!sessionId || score === undefined) {
      res.status(400).json({ error: 'sessionId and score are required' });
      return;
    }
    if (typeof score !== 'number' || score < 0) {
      res.status(400).json({ error: 'score must be a non-negative number' });
      return;
    }

    const result = await submitScore(sessionId, req.player!.guestId, score, level ?? 1);
    res.status(200).json({ message: 'Score submitted', ...result });
  } catch (error) {
    console.error('Submit score error:', error);
    res.status(500).json({ error: 'Failed to submit score' });
  }
}

// GET /game/leaderboard?limit=10 — top players ranked by high score
export async function leaderboard(req: Request, res: Response): Promise<void> {
  try {
    const limit = Math.min(parseInt(req.query.limit as string) || 10, 100);
    const rows = await getLeaderboard(limit);
    res.status(200).json({ leaderboard: rows });
  } catch (error) {
    console.error('Leaderboard error:', error);
    res.status(500).json({ error: 'Failed to fetch leaderboard' });
  }
}

// GET /game/rank — the requesting player's rank and high score
export async function myRank(req: AuthRequest, res: Response): Promise<void> {
  try {
    const result = await getPlayerRank(req.player!.guestId);
    if (!result) {
      res.status(404).json({ error: 'Player not found' });
      return;
    }
    res.status(200).json(result);
  } catch (error) {
    console.error('Rank error:', error);
    res.status(500).json({ error: 'Failed to fetch rank' });
  }
}

// GET /game/history?limit=10 — the player's recent game sessions
export async function scoreHistory(req: AuthRequest, res: Response): Promise<void> {
  try {
    const limit = Math.min(parseInt(req.query.limit as string) || 10, 50);
    const history = await getScoreHistory(req.player!.guestId, limit);
    res.status(200).json({ history });
  } catch (error) {
    console.error('History error:', error);
    res.status(500).json({ error: 'Failed to fetch score history' });
  }
}

// GET /game/me — the player's full profile and stats
export async function myProfile(req: AuthRequest, res: Response): Promise<void> {
  try {
    const profile = await getPlayerProfile(req.player!.guestId);
    if (!profile) {
      res.status(404).json({ error: 'Player not found' });
      return;
    }
    res.status(200).json(profile);
  } catch (error) {
    console.error('Profile error:', error);
    res.status(500).json({ error: 'Failed to fetch profile' });
  }
}
