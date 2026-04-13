using ArenaShooter.Combat;
using UnityEngine;

namespace ArenaShooter.Enemies
{
    [RequireComponent(typeof(Health))]
    public class TankEnemy : MonoBehaviour
    {
        private enum AttackState
        {
            Chasing,
            WindingUp,
            Recovering
        }

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform visualRoot;

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 2f;
        [SerializeField, Min(1f)] private float rotationSpeed = 10f;

        [Header("Attack")]
        [SerializeField, Min(0.1f)] private float attackRange = 2.2f;
        [SerializeField, Min(1f)] private float damage = 30f;
        [SerializeField, Min(0.05f)] private float windUpDuration = 0.6f;
        [SerializeField, Min(0.05f)] private float recoveryDuration = 0.8f;
        [SerializeField] private LayerMask attackMask = ~0;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer[] renderersToTint;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color windUpColor = new Color(1f, 0.35f, 0.1f);

        private Health health;
        private EnemyProceduralAnimation proceduralAnimation;
        private AttackState attackState;
        private float stateEndTime;

        private void Awake()
        {
            health = GetComponent<Health>();
            proceduralAnimation = GetComponent<EnemyProceduralAnimation>();

            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            if (renderersToTint == null || renderersToTint.Length == 0)
            {
                renderersToTint = GetComponentsInChildren<Renderer>();
            }

            health.Died += HandleDeath;
            SetTint(normalColor);
        }

        private void Start()
        {
            if (target == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

                if (playerObject != null)
                {
                    target = playerObject.transform;
                }
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
            }
        }

        private void Update()
        {
            if (!health.IsAlive)
            {
                return;
            }

            TryAcquireTarget();

            if (target == null)
            {
                return;
            }

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.001f)
            {
                return;
            }

            float planarDistance = toTarget.magnitude;
            float attackDistance = GetHorizontalDistanceToTarget();
            Vector3 direction = toTarget / planarDistance;
            RotateTowards(direction);

            switch (attackState)
            {
                case AttackState.Chasing:
                    UpdateChasing(attackDistance, direction);
                    break;
                case AttackState.WindingUp:
                    UpdateWindingUp();
                    break;
                case AttackState.Recovering:
                    UpdateRecovering();
                    break;
            }
        }

        private void UpdateChasing(float distance, Vector3 direction)
        {
            if (distance > attackRange)
            {
                transform.position += direction * moveSpeed * Time.deltaTime;
                return;
            }

            BeginWindUp();
        }

        private void BeginWindUp()
        {
            attackState = AttackState.WindingUp;
            stateEndTime = Time.time + windUpDuration;
            SetTint(windUpColor);
        }

        private void UpdateWindingUp()
        {
            if (Time.time < stateEndTime)
            {
                return;
            }

            DealAreaDamage();
            attackState = AttackState.Recovering;
            stateEndTime = Time.time + recoveryDuration;
            SetTint(normalColor);
        }

        private void UpdateRecovering()
        {
            if (Time.time >= stateEndTime)
            {
                attackState = AttackState.Chasing;
            }
        }

        private void DealAreaDamage()
        {
            proceduralAnimation?.TriggerAttack();

            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, attackMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hits.Length; i++)
            {
                IDamageable damageable =
                    hits[i].GetComponent<IDamageable>() ??
                    hits[i].GetComponentInParent<IDamageable>();

                if (damageable == null || !damageable.IsAlive)
                {
                    continue;
                }

                if (hits[i].transform == transform || hits[i].transform.IsChildOf(transform))
                {
                    continue;
                }

                if (!IsWithinHorizontalAttackRange(hits[i]))
                {
                    continue;
                }

                if (damageable is Health targetHealth)
                {
                    targetHealth.TakeDamage(damage, this, $"{name} slam");
                }
            }
        }

        private void RotateTowards(Vector3 direction)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void SetTint(Color color)
        {
            if (renderersToTint == null)
            {
                return;
            }

            for (int i = 0; i < renderersToTint.Length; i++)
            {
                if (renderersToTint[i] != null)
                {
                    renderersToTint[i].material.color = color;
                }
            }
        }

        private void HandleDeath()
        {
            enabled = false;
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

        private bool IsWithinHorizontalAttackRange(Collider targetCollider)
        {
            Vector3 closestPoint = targetCollider.ClosestPoint(transform.position);
            Vector3 enemyPosition = transform.position;
            closestPoint.y = enemyPosition.y;
            return Vector3.Distance(enemyPosition, closestPoint) <= attackRange;
        }

        private void TryAcquireTarget()
        {
            if (target != null)
            {
                Health targetHealth = target.GetComponent<Health>() ?? target.GetComponentInParent<Health>();

                if (targetHealth != null && targetHealth.IsAlive)
                {
                    return;
                }
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                target = playerObject.transform;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = windUpColor;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
