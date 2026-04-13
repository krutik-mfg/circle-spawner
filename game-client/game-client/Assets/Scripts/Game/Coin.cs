using UnityEngine;

namespace Game
{
    public class Coin : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed = 90f;

        void Update()
        {
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
        }
    }
}
