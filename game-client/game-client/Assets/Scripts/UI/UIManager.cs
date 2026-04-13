using API;
using Models;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text usernameText;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Transform leaderboardContent;

        private Text _finalScoreText;
        private Text _highScoreText;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            gameOverPanel.SetActive(false);

            if (usernameText != null && ApiManager.Instance != null)
                usernameText.text = $"Player: {ApiManager.Instance.Username}";

            restartButton.onClick.AddListener(Game.GameManager.Instance.RestartGame);
            menuButton.onClick.AddListener(Game.GameManager.Instance.GoToMenu);

            StyleButton(restartButton, "Play Again", new Color(0.2f, 0.7f, 0.3f));
            StyleButton(menuButton, "Menu", new Color(0.3f, 0.3f, 0.8f));

            SetupGameOverPanel();
        }

        void SetupGameOverPanel()
        {
            // Make panel full screen dark overlay
            var panelRT = gameOverPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            var panelImg = gameOverPanel.GetComponent<Image>();
            if (panelImg != null) panelImg.color = new Color(0.1f, 0.12f, 0.2f, 0.97f);

            // Create GAME OVER title if not exists
            var title = gameOverPanel.transform.Find("TitleText");
            Text titleText = title != null ? title.GetComponent<Text>() : CreateText(gameOverPanel.transform, "TitleText");
            titleText.text = "GAME OVER";
            titleText.fontSize = 64;
            titleText.color = new Color(1f, 0.3f, 0.3f);
            titleText.alignment = TextAnchor.MiddleCenter;
            SetRect(titleText.GetComponent<RectTransform>(), 0, 160, 600, 80);

            // Player + Score text
            var scoreObj = gameOverPanel.transform.Find("FinalScoreText");
            _finalScoreText = scoreObj != null ? scoreObj.GetComponent<Text>() : CreateText(gameOverPanel.transform, "FinalScoreText");
            _finalScoreText.fontSize = 40;
            _finalScoreText.color = Color.white;
            _finalScoreText.alignment = TextAnchor.MiddleCenter;
            SetRect(_finalScoreText.GetComponent<RectTransform>(), 0, 70, 600, 60);

            // High score text
            var hsObj = gameOverPanel.transform.Find("HighScoreText");
            _highScoreText = hsObj != null ? hsObj.GetComponent<Text>() : CreateText(gameOverPanel.transform, "HighScoreText");
            _highScoreText.fontSize = 32;
            _highScoreText.color = Color.yellow;
            _highScoreText.alignment = TextAnchor.MiddleCenter;
            SetRect(_highScoreText.GetComponent<RectTransform>(), 0, 10, 600, 50);

            // Buttons
            SetRect(restartButton.GetComponent<RectTransform>(), 0, -70, 280, 60);
            SetRect(menuButton.GetComponent<RectTransform>(), 0, -150, 280, 60);

            // Leaderboard scroll view
            if (leaderboardContent != null)
            {
                var scrollRT = leaderboardContent.parent?.parent?.GetComponent<RectTransform>();
                if (scrollRT != null)
                {
                    scrollRT.anchorMin = new Vector2(0.1f, 0f);
                    scrollRT.anchorMax = new Vector2(0.9f, 0f);
                    scrollRT.pivot = new Vector2(0.5f, 0f);
                    scrollRT.anchoredPosition = new Vector2(0, 20);
                    scrollRT.sizeDelta = new Vector2(0, 220);
                }

                // Add layout group to content if not present
                if (leaderboardContent.GetComponent<VerticalLayoutGroup>() == null)
                {
                    var vlg = leaderboardContent.gameObject.AddComponent<VerticalLayoutGroup>();
                    vlg.childAlignment = TextAnchor.UpperCenter;
                    vlg.spacing = 8;
                    vlg.childControlWidth = true;
                    vlg.childForceExpandWidth = true;
                }
            }
        }

        Text CreateText(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return t;
        }

        void SetRect(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
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
                txt.fontSize = 28;
                txt.color = Color.white;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = $"Score: {score}";
        }

        public void UpdateTimer(float time)
        {
            if (timerText == null) return;
            int t = Mathf.CeilToInt(time);
            timerText.text = $"Time: {t}s";
            timerText.color = t <= 10 ? Color.red : Color.white;
        }

        public void ShowGameOver(int score, int highScore)
        {
            gameOverPanel.SetActive(true);

            string username = ApiManager.Instance?.Username ?? "Guest";
            if (_finalScoreText != null)
                _finalScoreText.text = $"{username}\nScore: {score}";

            if (_highScoreText != null)
                _highScoreText.text = $"Best: {highScore}";

            LoadLeaderboard();
        }

        void LoadLeaderboard()
        {
            if (leaderboardContent == null) return;
            StartCoroutine(ApiManager.Instance.GetLeaderboard((ok, entries) =>
            {
                if (!ok || entries == null) return;
                foreach (Transform child in leaderboardContent) Destroy(child.gameObject);

                AddEntry("TOP PLAYERS", 26, Color.yellow);
                int rank = 1;
                foreach (LeaderboardEntry entry in entries)
                {
                    Color c = rank == 1 ? Color.yellow :
                              rank == 2 ? new Color(0.85f, 0.85f, 0.85f) :
                              rank == 3 ? new Color(0.8f, 0.5f, 0.2f) : Color.white;
                    AddEntry($"{rank}.  {entry.username}   {entry.highScore} pts", 22, c);
                    rank++;
                }
            }));
        }

        void AddEntry(string content, int size, Color color)
        {
            var go = new GameObject("Entry");
            go.transform.SetParent(leaderboardContent, false);
            var t = go.AddComponent<Text>();
            t.text = content;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = size + 14;
        }
    }
}
