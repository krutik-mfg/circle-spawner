import { Router } from 'express';
import { authMiddleware } from '../middleware/auth';
import {
  startGame,
  submitGameScore,
  leaderboard,
  myRank,
  scoreHistory,
  myProfile,
} from '../controllers/gameController';

const router = Router();

// Public routes (no auth needed)
router.get('/leaderboard', leaderboard); // GET  /game/leaderboard?limit=10

// Protected routes (JWT required)
router.use(authMiddleware);

router.post('/start',   startGame);       // POST /game/start
router.post('/score',   submitGameScore); // POST /game/score
router.get('/rank',     myRank);          // GET  /game/rank
router.get('/history',  scoreHistory);    // GET  /game/history?limit=10
router.get('/me',       myProfile);       // GET  /game/me

export default router;
