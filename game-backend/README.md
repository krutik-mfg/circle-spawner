# Game Backend — Node.js + TypeScript + Couchbase

A backend for a Unity coin-collector game with guest login, session tracking, and leaderboard.

---

## Stack
- **Node.js + TypeScript** — Express server
- **Couchbase** — document database
- **Unity** — game client (calls these REST APIs)

---

## Setup

### 1. Install Couchbase locally
Download from https://www.couchbase.com/downloads/
Create a bucket named `game_db` and enable the Query service.

### 2. Create the N1QL index (run once in Couchbase Query Workbench)
```sql
CREATE PRIMARY INDEX ON `game_db`;
```

### 3. Configure .env
```
PORT=3000
CB_CONNECTION_STRING=couchbase://localhost
CB_USERNAME=Administrator
CB_PASSWORD=password
CB_BUCKET_NAME=game_db
```

### 4. Run the server
```bash
npm run dev
```

---

## API Endpoints

### Guest Login
```
POST /auth/guest
```
**Response:**
```json
{
  "message": "Guest account created",
  "guestId": "uuid",
  "username": "Guest_ABC123",
  "token": "base64-token"
}
```

### Start Game Session
```
POST /game/start
Authorization: Bearer <token>
```
**Response:**
```json
{
  "message": "Game session started",
  "session": { "sessionId": "uuid", "status": "active", ... }
}
```

### Submit Score
```
POST /game/score
Authorization: Bearer <token>
Content-Type: application/json

{ "sessionId": "uuid", "score": 1500 }
```
**Response:**
```json
{ "message": "Score submitted", "highScore": 1500 }
```

### Leaderboard
```
GET /game/leaderboard
Authorization: Bearer <token>
```
**Response:**
```json
{
  "leaderboard": [
    { "username": "Guest_ABC123", "highScore": 1500 }
  ]
}
```

---

## Unity Integration (C# examples)

### Guest Login
```csharp
IEnumerator GuestLogin() {
    var request = new UnityWebRequest("http://localhost:3000/auth/guest", "POST");
    request.SetRequestHeader("Content-Type", "application/json");
    request.downloadHandler = new DownloadHandlerBuffer();
    yield return request.SendWebRequest();

    var json = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
    PlayerPrefs.SetString("token", json.token);
    PlayerPrefs.SetString("guestId", json.guestId);
}
```

### Submit Score
```csharp
IEnumerator SubmitScore(string sessionId, int score) {
    string token = PlayerPrefs.GetString("token");
    string body = JsonUtility.ToJson(new ScorePayload { sessionId = sessionId, score = score });

    var request = new UnityWebRequest("http://localhost:3000/game/score", "POST");
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("Authorization", "Bearer " + token);
    request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
    request.downloadHandler = new DownloadHandlerBuffer();
    yield return request.SendWebRequest();
}
```
