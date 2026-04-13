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
        [SerializeField] private Text statusText;
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private Transform leaderboardContent;
        [SerializeField] private GameObject leaderboardEntryPrefab;

        void Start()
        {
            guestLoginButton.onClick.AddListener(OnGuestLogin);
            leaderboardButton.onClick.AddListener(OnShowLeaderboard);
            leaderboardPanel.SetActive(false);

            if (ApiManager.Instance.IsLoggedIn())
                statusText.text = $"Welcome back, {ApiManager.Instance.Username}!";
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
            StartCoroutine(ApiManager.Instance.GetLeaderboard((ok, entries) =>
            {
                if (!ok) return;

                foreach (Transform child in leaderboardContent)
                    Destroy(child.gameObject);

                int rank = 1;
                foreach (var entry in entries)
                {
                    var go = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                    go.GetComponentInChildren<Text>().text = $"{rank}. {entry.username}  —  {entry.highScore} pts";
                    rank++;
                }

                leaderboardPanel.SetActive(true);
            }));
        }
    }
}
