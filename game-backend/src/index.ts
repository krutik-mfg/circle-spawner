import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import { connectCouchbase } from './config/couchbase';
import authRoutes from './routes/auth';
import gameRoutes from './routes/game';

dotenv.config();

const app = express();
const PORT = process.env.PORT || 3000;

app.use(cors());
app.use(express.json());

// Routes
app.use('/auth', authRoutes);
app.use('/game', gameRoutes);

app.get('/health', (_req, res) => {
  res.json({ status: 'ok', timestamp: new Date().toISOString() });
});

async function start() {
  await connectCouchbase();
  app.listen(PORT, () => {
    console.log(`Game backend running on http://localhost:${PORT}`);
  });
}

start().catch(console.error);
