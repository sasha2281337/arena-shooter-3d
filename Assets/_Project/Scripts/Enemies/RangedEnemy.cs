using ArenaShooter.Combat;
using UnityEngine;

namespace ArenaShooter.Enemies
{
    [RequireComponent(typeof(Health))]
    public class RangedEnemy : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform firePoint;

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 2.8f;
        [SerializeField, Min(0.1f)] private float preferredDistance = 8f;
        [SerializeField, Min(0.1f)] private float distanceTolerance = 1.5f;
        [SerializeField, Min(1f)] private float rotationSpeed = 12f;
        [SerializeField] private LayerMask lineOfSightMask = ~0;
        [SerializeField, Min(0f)] private float spawnAdvanceDistance = 2.5f;
        [SerializeField, Range(0.1f, 1f)] private float strafeSpeedMultiplier = 0.65f;
        [SerializeField, Min(0.1f)] private float minStrafeDirectionDuration = 1.2f;
        [SerializeField, Min(0.1f)] private float maxStrafeDirectionDuration = 2.6f;

        [Header("Attack")]
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField, Min(1f)] private float damage = 12f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 1.5f;
        [SerializeField, Min(1f)] private float attackRange = 14f;

        private Health health;
        private EnemyProceduralAnimation proceduralAnimation;
        private IDamageable targetDamageable;
        private float nextAttackTime;
        private Vector3 spawnAdvanceStartPosition;
        private float forcedAdvanceEndTime;
        private int strafeDirection = 1;
        private float nextStrafeDirectionChangeTime;
        private Quaternion visualLocalRotationOffset = Quaternion.identity;
        private Vector3 lastPosition;
        private float nextStuckCheckTime;

        private void Awake()
        {
            health = GetComponent<Health>();
            proceduralAnimation = GetComponent<EnemyProceduralAnimation>();

            if (spawnAdvanceDistance <= 0f)
            {
                spawnAdvanceDistance = 2.5f;
            }

            if (visualRoot == null)
            {
                visualRoot = ResolveVisibleChildTransform();
            }

            if (firePoint == null)
            {
                firePoint = transform;
            }

            if (visualRoot != null && visualRoot != transform)
            {
                visualLocalRotationOffset = visualRoot.localRotation;
            }

            if (proceduralAnimation != null && visualRoot != null)
            {
                proceduralAnimation.SetAnimationTargetIfMissing(visualRoot);
            }

            health.Died += HandleDeath;
        }

        private void OnEnable()
        {
            spawnAdvanceStartPosition = transform.position;
            forcedAdvanceEndTime = Time.time + 0.75f;
            strafeDirection = Random.value > 0.5f ? 1 : -1;
            ScheduleNextStrafeDirectionChange();
            lastPosition = transform.position;
            nextStuckCheckTime = Time.time + 0.35f;
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

            if (target == null || targetDamageable == null || !targetDamageable.IsAlive)
            {
                return;
            }

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.001f)
            {
                return;
            }

            float distance = toTarget.magnitude;
            Vector3 directionToTarget = toTarget / distance;
            bool hasLineOfSight = HasLineOfSight();

            RotateTowards(directionToTarget);
            UpdateMovement(distance, directionToTarget, hasLineOfSight);
            NudgeIfStuck(directionToTarget);

            if (distance <= attackRange && hasLineOfSight)
            {
                TryAttack();
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            targetDamageable =
                newTarget != null
                    ? newTarget.GetComponent<IDamageable>() ?? newTarget.GetComponentInParent<IDamageable>()
                    : null;
        }

        private void TryAcquireTarget()
        {
            if (target != null && targetDamageable != null && targetDamageable.IsAlive)
            {
                return;
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                SetTarget(playerObject.transform);
            }
        }

        private void UpdateMovement(float distance, Vector3 directionToTarget, bool hasLineOfSight)
        {
            if (Time.time < forcedAdvanceEndTime || !HasClearedSpawnAdvanceDistance())
            {
                transform.position += directionToTarget * moveSpeed * Time.deltaTime;
                return;
            }

            if (!hasLineOfSight)
            {
                transform.position += directionToTarget * moveSpeed * Time.deltaTime;
                return;
            }

            if (distance > preferredDistance + distanceTolerance)
            {
                transform.position += directionToTarget * moveSpeed * Time.deltaTime;
                return;
            }

            if (distance < preferredDistance - distanceTolerance)
            {
                transform.position -= directionToTarget * moveSpeed * Time.deltaTime;
                return;
            }

            StrafeAroundTarget(directionToTarget, distance);
        }

        private void TryAttack()
        {
            if (projectilePrefab == null || Time.time < nextAttackTime)
            {
                return;
            }

            Vector3 spawnPosition = firePoint.position;
            Vector3 targetAimPoint = GetTargetAimPoint();
            Vector3 shootDirection = targetAimPoint - spawnPosition;

            if (shootDirection.sqrMagnitude < 0.001f)
            {
                return;
            }

            nextAttackTime = Time.time + attackCooldown;
            shootDirection.Normalize();

            proceduralAnimation?.TriggerAttack();
            EnemyProjectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(shootDirection, Vector3.up));
            projectile.Launch(shootDirection, damage, transform);
        }

        private Vector3 GetTargetAimPoint()
        {
            if (target == null)
            {
                return transform.position + Vector3.up;
            }

            CharacterController characterController = target.GetComponent<CharacterController>();

            if (characterController != null)
            {
                return characterController.bounds.center;
            }

            Collider targetCollider = target.GetComponent<Collider>() ?? target.GetComponentInChildren<Collider>();

            if (targetCollider != null)
            {
                return targetCollider.bounds.center;
            }

            Renderer targetRenderer = target.GetComponentInChildren<Renderer>();

            if (targetRenderer != null)
            {
                return targetRenderer.bounds.center;
            }

            return target.position + Vector3.up;
        }

        private void RotateTowards(Vector3 direction)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (visualRoot != null && visualRoot != transform)
            {
                visualRoot.rotation = transform.rotation * visualLocalRotationOffset;
            }
        }

        private void HandleDeath()
        {
            enabled = false;
        }

        private bool HasLineOfSight()
        {
            if (target == null)
            {
                return false;
            }

            Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
            Vector3 targetPoint = GetTargetAimPoint();
            Vector3 direction = targetPoint - origin;
            float distance = direction.magnitude;

            if (distance <= 0.001f)
            {
                return true;
            }

            if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance, lineOfSightMask, QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            return hit.transform == target || hit.transform.IsChildOf(target);
        }

        private bool HasClearedSpawnAdvanceDistance()
        {
            if (spawnAdvanceDistance <= 0f)
            {
                return true;
            }

            Vector3 offset = transform.position - spawnAdvanceStartPosition;
            offset.y = 0f;
            return offset.sqrMagnitude >= spawnAdvanceDistance * spawnAdvanceDistance;
        }

        private void StrafeAroundTarget(Vector3 directionToTarget, float distance)
        {
            if (Time.time >= nextStrafeDirectionChangeTime)
            {
                strafeDirection *= -1;
                ScheduleNextStrafeDirectionChange();
            }

            Vector3 strafe = Vector3.Cross(Vector3.up, directionToTarget) * strafeDirection;
            float radialError = distance - preferredDistance;
            Vector3 radialCorrection = directionToTarget * Mathf.Clamp(radialError, -0.6f, 0.6f);
            Vector3 moveDirection = (strafe + radialCorrection * 0.35f).normalized;
            float strafeSpeed = moveSpeed * Mathf.Max(0.9f, strafeSpeedMultiplier);
            transform.position += moveDirection * strafeSpeed * Time.deltaTime;
        }

        private void ScheduleNextStrafeDirectionChange()
        {
            float duration = Random.Range(minStrafeDirectionDuration, maxStrafeDirectionDuration);
            nextStrafeDirectionChangeTime = Time.time + duration;
        }

        private void NudgeIfStuck(Vector3 directionToTarget)
        {
            if (Time.time < nextStuckCheckTime)
            {
                return;
            }

            float movedDistance = Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;
            nextStuckCheckTime = Time.time + 0.35f;

            if (movedDistance >= 0.05f)
            {
                return;
            }

            transform.position += directionToTarget * (moveSpeed * 0.5f) * Time.deltaTime;
        }

        private Transform ResolveVisibleChildTransform()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer candidate = renderers[i];

                if (candidate == null || !candidate.enabled)
                {
                    continue;
                }

                Transform candidateTransform = candidate.transform;

                if (candidateTransform == transform || candidateTransform == firePoint || candidateTransform.IsChildOf(firePoint))
                {
                    continue;
                }

                return candidateTransform;
            }

            return transform;
        }
    }
}
