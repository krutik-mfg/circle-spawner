using System;
using System.Collections;
using System.Text;
using Models;
using UnityEngine;
using UnityEngine.Networking;

namespace API
{
    public class ApiManager : MonoBehaviour
    {
        public static ApiManager Instance { get; private set; }

        private const string BaseUrl = "http://localhost:3000";

        private string _token;
        private string _guestId;
        private string _username;
        private string _sessionId;

        public string Username => _username;
        public string SessionId => _sessionId;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Restore saved session
            _token = PlayerPrefs.GetString("token", "");
            _guestId = PlayerPrefs.GetString("guestId", "");
            _username = PlayerPrefs.GetString("username", "");
        }

        public bool IsLoggedIn() => !string.IsNullOrEmpty(_token);

        // ── Guest Login ──────────────────────────────────────────
        public IEnumerator GuestLogin(Action<bool, string> callback)
        {
            var request = new UnityWebRequest($"{BaseUrl}/auth/guest", "POST");
            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                callback(false, request.error);
                yield break;
            }

            var response = JsonUtility.FromJson<GuestLoginResponse>(request.downloadHandler.text);
            _token = response.token;
            _guestId = response.guestId;
            _username = response.username;

            PlayerPrefs.SetString("token", _token);
            PlayerPrefs.SetString("guestId", _guestId);
            PlayerPrefs.SetString("username", _username);
            PlayerPrefs.Save();

            callback(true, _username);
        }

        // ── Start Game Session ────────────────────────────────────
        public IEnumerator StartSession(Action<bool, string> callback)
        {
            var request = new UnityWebRequest($"{BaseUrl}/game/start", "POST");
            request.SetRequestHeader("Authorization", $"Bearer {_token}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                callback(false, request.error);
                yield break;
            }

            var response = JsonUtility.FromJson<SessionResponse>(request.downloadHandler.text);
            _sessionId = response.session.sessionId;
            callback(true, _sessionId);
        }

        // ── Submit Score ──────────────────────────────────────────
        public IEnumerator SubmitScore(int score, Action<bool, int> callback)
        {
            var payload = new ScoreRequest { sessionId = _sessionId, score = score };
            string json = JsonUtility.ToJson(payload);

            var request = new UnityWebRequest($"{BaseUrl}/game/score", "POST");
            request.SetRequestHeader("Authorization", $"Bearer {_token}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                callback(false, 0);
                yield break;
            }

            var response = JsonUtility.FromJson<ScoreResponse>(request.downloadHandler.text);
            callback(true, response.highScore);
        }

        // ── Leaderboard ───────────────────────────────────────────
        public IEnumerator GetLeaderboard(Action<bool, LeaderboardEntry[]> callback)
        {
            var request = UnityWebRequest.Get($"{BaseUrl}/game/leaderboard");
            request.SetRequestHeader("Authorization", $"Bearer {_token}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                callback(false, null);
                yield break;
            }

            var response = JsonUtility.FromJson<LeaderboardResponse>(request.downloadHandler.text);
            callback(true, response.leaderboard);
        }
    }
}
