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
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Transform leaderboardContent;
        [SerializeField] private GameObject leaderboardEntryPrefab;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            gameOverPanel.SetActive(false);
            usernameText.text = ApiManager.Instance.Username;
            restartButton.onClick.AddListener(Game.GameManager.Instance.RestartGame);
            menuButton.onClick.AddListener(Game.GameManager.Instance.GoToMenu);
        }

        public void UpdateScore(int score) => scoreText.text = $"Score: {score}";

        public void UpdateTimer(float time)
        {
            int t = Mathf.CeilToInt(time);
            timerText.text = $"Time: {t}s";
            timerText.color = t <= 10 ? Color.red : Color.white;
        }

        public void ShowGameOver(int score, int highScore)
        {
            gameOverPanel.SetActive(true);
            finalScoreText.text = $"Score: {score}";
            highScoreText.text = $"Best: {highScore}";
            LoadLeaderboard();
        }

        void LoadLeaderboard()
        {
            StartCoroutine(ApiManager.Instance.GetLeaderboard((ok, entries) =>
            {
                if (!ok || entries == null) return;
                foreach (Transform child in leaderboardContent) Destroy(child.gameObject);

                int rank = 1;
                foreach (LeaderboardEntry entry in entries)
                {
                    var go = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                    go.GetComponentInChildren<Text>().text = $"{rank}. {entry.username}  —  {entry.highScore}";
                    rank++;
                }
            }));
        }
    }
}
