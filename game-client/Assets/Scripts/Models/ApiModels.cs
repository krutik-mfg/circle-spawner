using System;

namespace Models
{
    [Serializable]
    public class GuestLoginResponse
    {
        public string message;
        public string guestId;
        public string username;
        public string token;
    }

    [Serializable]
    public class SessionResponse
    {
        public string message;
        public SessionData session;
    }

    [Serializable]
    public class SessionData
    {
        public string sessionId;
        public string guestId;
        public int score;
        public string status;
        public string startedAt;
    }

    [Serializable]
    public class ScoreRequest
    {
        public string sessionId;
        public int score;
    }

    [Serializable]
    public class ScoreResponse
    {
        public string message;
        public int highScore;
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string username;
        public int highScore;
    }

    [Serializable]
    public class LeaderboardResponse
    {
        public LeaderboardEntry[] leaderboard;
    }
}
