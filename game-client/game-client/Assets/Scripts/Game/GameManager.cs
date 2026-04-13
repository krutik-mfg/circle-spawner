using API;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public int Score { get; private set; }
        public bool IsGameOver { get; private set; }

        [SerializeField] private float gameDuration = 30f;
        private float _timeLeft;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            _timeLeft = gameDuration;
            Score = 0;
            IsGameOver = false;
            StartCoroutine(ApiManager.Instance.StartSession((ok, sessionId) =>
            {
                if (!ok) Debug.LogWarning("Could not start session");
            }));
        }

        void Update()
        {
            if (IsGameOver) return;

            _timeLeft -= Time.deltaTime;
            UI.UIManager.Instance?.UpdateTimer(_timeLeft);

            if (_timeLeft <= 0f) EndGame();
        }

        public void AddScore(int amount)
        {
            Score += amount;
            UI.UIManager.Instance?.UpdateScore(Score);
        }

        public void EndGame()
        {
            if (IsGameOver) return;
            IsGameOver = true;

            StartCoroutine(ApiManager.Instance.SubmitScore(Score, (ok, highScore) =>
            {
                UI.UIManager.Instance?.ShowGameOver(Score, highScore);
            }));
        }

        public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        public void GoToMenu() => SceneManager.LoadScene("LoginScene");
    }
}
