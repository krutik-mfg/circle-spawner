import { Router } from 'express';
import { guestLogin, register, login } from '../controllers/authController';

const router = Router();

router.post('/guest',    guestLogin); // POST /auth/guest    — anonymous account
router.post('/register', register);   // POST /auth/register — create named account
router.post('/login',    login);      // POST /auth/login    — sign in

export default router;
