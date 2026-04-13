using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 8f;

        private Rigidbody2D _rb;
        private Vector2 _input;

        void Awake() => _rb = GetComponent<Rigidbody2D>();

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
            _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        void FixedUpdate()
        {
            _rb.velocity = _input.normalized * moveSpeed;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Coin"))
            {
                GameManager.Instance?.AddScore(10);
                Destroy(other.gameObject);
            }
        }
    }
}
