import { Request, Response } from 'express';
import { createGuestPlayer, registerPlayer, loginPlayer } from '../services/authService';

// POST /auth/guest — create an anonymous guest account
export async function guestLogin(_req: Request, res: Response): Promise<void> {
  try {
    const result = await createGuestPlayer();
    res.status(201).json({ message: 'Guest account created', ...result });
  } catch (error) {
    console.error('Guest login error:', error);
    res.status(500).json({ error: 'Failed to create guest account' });
  }
}

// POST /auth/register — create a named account with a password
// Body: { username: string, password: string }
export async function register(req: Request, res: Response): Promise<void> {
  try {
    const { username, password } = req.body;

    if (!username || !password) {
      res.status(400).json({ error: 'username and password are required' });
      return;
    }
    if (username.length < 3 || username.length > 20) {
      res.status(400).json({ error: 'username must be 3–20 characters' });
      return;
    }
    if (password.length < 6) {
      res.status(400).json({ error: 'password must be at least 6 characters' });
      return;
    }

    const result = await registerPlayer(username.trim(), password);
    res.status(201).json({ message: 'Account created', ...result });
  } catch (error: any) {
    if (error.message === 'USERNAME_TAKEN') {
      res.status(409).json({ error: 'Username already taken' });
      return;
    }
    console.error('Register error:', error);
    res.status(500).json({ error: 'Failed to create account' });
  }
}

// POST /auth/login — sign in with username + password
// Body: { username: string, password: string }
export async function login(req: Request, res: Response): Promise<void> {
  try {
    const { username, password } = req.body;

    if (!username || !password) {
      res.status(400).json({ error: 'username and password are required' });
      return;
    }

    const result = await loginPlayer(username.trim(), password);
    res.status(200).json({ message: 'Login successful', ...result });
  } catch (error: any) {
    if (error.message === 'INVALID_CREDENTIALS') {
      res.status(401).json({ error: 'Invalid username or password' });
      return;
    }
    console.error('Login error:', error);
    res.status(500).json({ error: 'Login failed' });
  }
}
