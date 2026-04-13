using UnityEngine;

namespace Game
{
    public class CoinSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private Vector2 spawnAreaMin = new Vector2(-8f, -4f);
        [SerializeField] private Vector2 spawnAreaMax = new Vector2(8f, 4f);
        [SerializeField] private int maxCoins = 10;

        private float _timer;

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

            _timer += Time.deltaTime;
            if (_timer >= spawnInterval && FindObjectsByType<Coin>(FindObjectsSortMode.None).Length < maxCoins)
            {
                SpawnCoin();
                _timer = 0f;
            }
        }

        void SpawnCoin()
        {
            Vector2 pos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            Instantiate(coinPrefab, pos, Quaternion.identity);
        }
    }
}
