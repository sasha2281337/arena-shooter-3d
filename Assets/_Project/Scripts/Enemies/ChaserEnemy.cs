using ArenaShooter.Combat;
using UnityEngine;

namespace ArenaShooter.Enemies
{
    [RequireComponent(typeof(Health))]
    public class ChaserEnemy : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform visualRoot;
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.5f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.6f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 1f;
        [SerializeField, Min(1f)] private float damage = 10f;
        [SerializeField, Min(1f)] private float rotationSpeed = 12f;

        private Health health;
        private EnemyProceduralAnimation proceduralAnimation;
        private IDamageable targetDamageable;
        private Health targetHealth;
        private float nextAttackTime;

        private void Awake()
        {
            health = GetComponent<Health>();
            proceduralAnimation = GetComponent<EnemyProceduralAnimation>();

            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            health.Died += HandleDeath;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
            }
        }

        private void Start()
        {
            if (target == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

                if (playerObject != null)
                {
                    SetTarget(playerObject.transform);
                }
            }
        }

        private void Update()
        {
            if (!health.IsAlive)
            {
                return;
            }

            TryAcquireTarget();

            if (target == null || targetDamageable == null || !targetDamageable.IsAlive)
            {
                return;
            }

            Vector3 targetPosition = target.position;
            Vector3 directionToTarget = targetPosition - transform.position;
            directionToTarget.y = 0f;

            if (directionToTarget.sqrMagnitude < 0.001f)
            {
                return;
            }

            float planarDistanceToTarget = directionToTarget.magnitude;
            float attackDistance = GetHorizontalDistanceToTarget();
            Vector3 moveDirection = directionToTarget / planarDistanceToTarget;

            RotateTowards(moveDirection);

            if (attackDistance > attackRange)
            {
                transform.position += moveDirection * moveSpeed * Time.deltaTime;
                return;
            }

            TryAttack();
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            targetDamageable =
                newTarget != null
                    ? newTarget.GetComponent<IDamageable>() ?? newTarget.GetComponentInParent<IDamageable>()
                    : null;
            targetHealth =
                newTarget != null
                    ? newTarget.GetComponent<Health>() ?? newTarget.GetComponentInParent<Health>()
                    : null;
        }

        private void TryAttack()
        {
            if (Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + attackCooldown;

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage, this, $"{name} melee");
                proceduralAnimation?.TriggerAttack();
            }
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void HandleDeath()
        {
            enabled = false;
        }

        private void TryAcquireTarget()
        {
            if (target != null && targetDamageable != null)
            {
                return;
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                SetTarget(playerObject.transform);
            }
        }

        private float GetHorizontalDistanceToTarget()
        {
            if (target == null)
            {
                return float.MaxValue;
            }

            Vector3 enemyPosition = transform.position;
            Vector3 closestPoint = GetClosestPointOnTarget(enemyPosition);
            closestPoint.y = enemyPosition.y;
            return Vector3.Distance(enemyPosition, closestPoint);
        }

        private Vector3 GetClosestPointOnTarget(Vector3 fromPosition)
        {
            CharacterController controller = target.GetComponent<CharacterController>();

            if (controller != null)
            {
                return controller.bounds.ClosestPoint(fromPosition);
            }

            Collider targetCollider = target.GetComponent<Collider>() ?? target.GetComponentInChildren<Collider>();

            if (targetCollider != null)
            {
                return targetCollider.ClosestPoint(fromPosition);
            }

            return target.position;
        }
    }
}
