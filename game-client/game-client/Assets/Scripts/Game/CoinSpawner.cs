using UnityEngine;

namespace Game
{
    public class CoinSpawner : MonoBehaviour
    {
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private Vector2 spawnAreaMin = new Vector2(-3f, -4f);
        [SerializeField] private Vector2 spawnAreaMax = new Vector2(3f, 4f);
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
            // Create coin from code — no prefab needed
            GameObject coin = new GameObject("Coin");
            coin.tag = "Coin";

            // Sprite
            SpriteRenderer sr = coin.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = new Color(1f, 0.85f, 0f); // yellow

            // Collider
            CircleCollider2D col = coin.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;

            // Coin script
            coin.AddComponent<Coin>();

            // Position
            Vector2 pos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            coin.transform.position = pos;
            coin.transform.localScale = Vector3.one * 0.8f;
        }

        Sprite CreateCircleSprite()
        {
            Texture2D tex = new Texture2D(128, 128);
            Vector2 center = new Vector2(64, 64);
            float radius = 60f;

            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
                }
            }
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
