import * as couchbase from 'couchbase';
import dotenv from 'dotenv';

dotenv.config();

let cluster: couchbase.Cluster;
let bucket: couchbase.Bucket;
let collection: couchbase.Collection;

export async function connectCouchbase() {
  cluster = await couchbase.connect(process.env.CB_CONNECTION_STRING!, {
    username: process.env.CB_USERNAME!,
    password: process.env.CB_PASSWORD!,
  });

  bucket = cluster.bucket(process.env.CB_BUCKET_NAME!);
  collection = bucket.defaultCollection();

  console.log('✅ Connected to Couchbase');

  await ensureIndexes();
}

// Create N1QL indexes needed for leaderboard + username lookup queries.
// IF NOT EXISTS makes these safe to run on every startup.
async function ensureIndexes() {
  const b = process.env.CB_BUCKET_NAME!;
  const indexes = [
    // Primary index (required for any N1QL query)
    `CREATE PRIMARY INDEX IF NOT EXISTS ON \`${b}\``,
    // Leaderboard: filter by type, sort by highScore
    `CREATE INDEX IF NOT EXISTS idx_player_score ON \`${b}\`(highScore DESC) WHERE type='player'`,
    // Username lookup for register / login
    `CREATE INDEX IF NOT EXISTS idx_player_username ON \`${b}\`(username) WHERE type='player'`,
    // Score history: filter sessions by guestId + status
    `CREATE INDEX IF NOT EXISTS idx_session_guest ON \`${b}\`(guestId, endedAt DESC) WHERE type='session'`,
  ];

  for (const stmt of indexes) {
    try {
      await cluster.query(stmt);
    } catch (e: any) {
      // Index already exists errors are safe to ignore
      if (!e.message?.includes('already exist')) {
        console.warn('Index warning:', e.message);
      }
    }
  }
  console.log('✅ Couchbase indexes ready');
}

export function getCollection(): couchbase.Collection {
  return collection;
}

export function getCluster(): couchbase.Cluster {
  return cluster;
}
