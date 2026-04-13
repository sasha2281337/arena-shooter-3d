using ArenaShooter.Combat;
using UnityEngine;

namespace ArenaShooter.Enemies
{
    [RequireComponent(typeof(Health))]
    public class DummyEnemy : MonoBehaviour
    {
        [SerializeField] private bool facePlayer = true;
        [SerializeField] private Transform visualRoot;
        [SerializeField, Min(1f)] private float turnSpeed = 10f;

        private Health health;
        private Transform playerTransform;

        private void Awake()
        {
            health = GetComponent<Health>();

            if (visualRoot == null)
            {
                visualRoot = transform;
            }
        }

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }

        private void Update()
        {
            if (!facePlayer || playerTransform == null || !health.IsAlive)
            {
                return;
            }

            Vector3 direction = playerTransform.position - visualRoot.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}
