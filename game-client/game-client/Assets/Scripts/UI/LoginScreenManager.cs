using API;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class LoginScreenManager : MonoBehaviour
    {
        [SerializeField] private Button guestLoginButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text statusText;
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private Transform leaderboardContent;

        void Start()
        {
            // Style the login buttons
            StyleButton(guestLoginButton, "Play as Guest", new Color(0.2f, 0.6f, 1f));
            StyleButton(leaderboardButton, "Leaderboard", new Color(0.2f, 0.8f, 0.4f));

            // Style status text
            if (statusText != null)
            {
                statusText.fontSize = 28;
                statusText.color = Color.white;
                statusText.alignment = TextAnchor.MiddleCenter;
                statusText.text = "Welcome! Login to play.";
            }

            // Position buttons vertically centered
            SetAnchored(guestLoginButton.GetComponent<RectTransform>(), 0, 60, 300, 60);
            SetAnchored(leaderboardButton.GetComponent<RectTransform>(), 0, -20, 300, 60);
            SetAnchored(statusText?.GetComponent<RectTransform>(), 0, 160, 400, 50);

            guestLoginButton.onClick.AddListener(OnGuestLogin);
            leaderboardButton.onClick.AddListener(OnShowLeaderboard);
            if (closeButton != null) closeButton.onClick.AddListener(() => leaderboardPanel.SetActive(false));

            // Hide leaderboard panel
            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);

            if (ApiManager.Instance != null && ApiManager.Instance.IsLoggedIn())
                statusText.text = $"Welcome back, {ApiManager.Instance.Username}!";
        }

        void StyleButton(Button btn, string label, Color color)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = color;
            var txt = btn.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.text = label;
                txt.fontSize = 26;
                txt.color = Color.white;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        void SetAnchored(RectTransform rt, float x, float y, float w, float h)
        {
            if (rt == null) return;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        void OnGuestLogin()
        {
            guestLoginButton.interactable = false;
            statusText.text = "Logging in...";
            StartCoroutine(ApiManager.Instance.GuestLogin((ok, result) =>
            {
                if (ok)
                {
                    statusText.text = $"Welcome, {result}!";
                    Invoke(nameof(LoadGame), 1f);
                }
                else
                {
                    statusText.text = "Login failed. Is the server running?";
                    guestLoginButton.interactable = true;
                }
            }));
        }

        void LoadGame() => SceneManager.LoadScene("GameScene");

        void OnShowLeaderboard()
        {
            // Style the leaderboard panel
            if (leaderboardPanel != null)
            {
                var rt = leaderboardPanel.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                var img = leaderboardPanel.GetComponent<Image>();
                if (img != null) img.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            }

            // Style close button
            if (closeButton != null)
            {
                var rt = closeButton.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                rt.anchoredPosition = new Vector2(-20, -20);
                rt.sizeDelta = new Vector2(60, 60);
                StyleButton(closeButton, "X", new Color(0.8f, 0.2f, 0.2f));
            }

            StartCoroutine(ApiManager.Instance.GetLeaderboard((ok, entries) =>
            {
                if (!ok || entries == null) return;

                foreach (Transform child in leaderboardContent)
                    Destroy(child.gameObject);

                // Add title
                AddText(leaderboardContent, "🏆  LEADERBOARD", 36, Color.yellow);

                int rank = 1;
                foreach (var entry in entries)
                {
                    Color c = rank == 1 ? Color.yellow : rank == 2 ? new Color(0.8f, 0.8f, 0.8f) : Color.white;
                    AddText(leaderboardContent, $"{rank}.  {entry.username}   —   {entry.highScore} pts", 26, c);
                    rank++;
                }

                leaderboardPanel.SetActive(true);
            }));
        }

        void AddText(Transform parent, string content, int size, Color color)
        {
            GameObject go = new GameObject("LeaderboardText");
            go.transform.SetParent(parent, false);
            Text t = go.AddComponent<Text>();
            t.text = content;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = size + 20;
        }
    }
}
