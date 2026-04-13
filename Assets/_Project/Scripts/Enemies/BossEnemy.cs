using ArenaShooter.Combat;
using UnityEngine;

namespace ArenaShooter.Enemies
{
    [RequireComponent(typeof(Health))]
    public class BossEnemy : MonoBehaviour
    {
        private enum BossState
        {
            Repositioning,
            ShootingBurst,
            StompWindUp,
            Recovering
        }

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform firePoint;

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 2.2f;
        [SerializeField, Min(1f)] private float rotationSpeed = 10f;
        [SerializeField, Min(1f)] private float preferredDistance = 10f;
        [SerializeField, Min(0.1f)] private float distanceTolerance = 2f;
        [SerializeField] private LayerMask lineOfSightMask = ~0;
        [SerializeField, Min(0f)] private float spawnAdvanceDistance = 3.5f;
        [SerializeField, Range(0.1f, 1f)] private float strafeSpeedMultiplier = 0.45f;
        [SerializeField, Min(0.1f)] private float minStrafeDirectionDuration = 1.8f;
        [SerializeField, Min(0.1f)] private float maxStrafeDirectionDuration = 3.4f;

        [Header("Projectile Attack")]
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField, Min(1f)] private float projectileDamage = 12f;
        [SerializeField, Min(1)] private int burstCount = 5;
        [SerializeField, Min(0.05f)] private float timeBetweenBurstShots = 0.18f;
        [SerializeField, Min(0.1f)] private float timeBetweenBursts = 2.2f;
        [SerializeField, Range(0f, 35f)] private float burstSpreadAngle = 8f;

        [Header("Stomp Attack")]
        [SerializeField, Min(0.1f)] private float stompRange = 3.5f;
        [SerializeField, Min(1f)] private float stompDamage = 35f;
        [SerializeField, Min(0.05f)] private float stompWindUpDuration = 0.75f;
        [SerializeField, Min(0.05f)] private float recoveryDuration = 0.8f;
        [SerializeField] private LayerMask stompMask = ~0;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer[] renderersToTint;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color attackColor = new Color(1f, 0.2f, 0.1f);

        private Health health;
        private EnemyProceduralAnimation proceduralAnimation;
        private BossState state;
        private float nextStateTime;
        private float nextBurstTime;
        private int burstShotsRemaining;
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
                spawnAdvanceDistance = 3.5f;
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

            if (renderersToTint == null || renderersToTint.Length == 0)
            {
                renderersToTint = GetComponentsInChildren<Renderer>();
            }

            health.Died += HandleDeath;
            state = BossState.Repositioning;
            nextStateTime = Time.time + 1f;
            SetTint(normalColor);
        }

        private void OnEnable()
        {
            spawnAdvanceStartPosition = transform.position;
            forcedAdvanceEndTime = Time.time + 1f;
            strafeDirection = Random.value > 0.5f ? 1 : -1;
            ScheduleNextStrafeDirectionChange();
            lastPosition = transform.position;
            nextStuckCheckTime = Time.time + 0.4f;
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
            Vector3 directionToTarget = toTarget / planarDistance;
            bool hasLineOfSight = HasLineOfSight();
            RotateTowards(directionToTarget);
            NudgeIfStuck(directionToTarget);

            switch (state)
            {
                case BossState.Repositioning:
                    UpdateRepositioning(planarDistance, directionToTarget, hasLineOfSight);
                    break;
                case BossState.ShootingBurst:
                    UpdateShootingBurst(hasLineOfSight);
                    break;
                case BossState.StompWindUp:
                    UpdateStompWindUp();
                    break;
                case BossState.Recovering:
                    UpdateRecovering();
                    break;
            }
        }

        private void UpdateRepositioning(float planarDistance, Vector3 directionToTarget, bool hasLineOfSight)
        {
            float stompDistance = GetHorizontalDistanceToTarget();

            if (stompDistance <= stompRange)
            {
                BeginStomp();
                return;
            }

            if (Time.time < forcedAdvanceEndTime || !HasClearedSpawnAdvanceDistance())
            {
                transform.position += directionToTarget * moveSpeed * Time.deltaTime;
            }
            else if (!hasLineOfSight)
            {
                transform.position += directionToTarget * moveSpeed * Time.deltaTime;
            }
            else if (planarDistance > preferredDistance + distanceTolerance)
            {
                transform.position += directionToTarget * moveSpeed * Time.deltaTime;
            }
            else if (planarDistance < preferredDistance - distanceTolerance)
            {
                transform.position -= directionToTarget * moveSpeed * Time.deltaTime;
            }
            else
            {
                StrafeAroundTarget(directionToTarget, planarDistance);
            }

            if (Time.time >= nextStateTime && hasLineOfSight)
            {
                BeginBurst();
            }
        }

        private void BeginBurst()
        {
            state = BossState.ShootingBurst;
            burstShotsRemaining = burstCount;
            nextBurstTime = Time.time;
            SetTint(attackColor);
        }

        private void UpdateShootingBurst(bool hasLineOfSight)
        {
            if (!hasLineOfSight)
            {
                state = BossState.Repositioning;
                nextStateTime = Time.time + 0.2f;
                SetTint(normalColor);
                return;
            }

            if (burstShotsRemaining <= 0)
            {
                BeginRecovery(timeBetweenBursts);
                return;
            }

            if (Time.time < nextBurstTime)
            {
                return;
            }

            ShootProjectile();
            burstShotsRemaining--;
            nextBurstTime = Time.time + timeBetweenBurstShots;
        }

        private void BeginStomp()
        {
            state = BossState.StompWindUp;
            nextStateTime = Time.time + stompWindUpDuration;
            SetTint(attackColor);
        }

        private void UpdateStompWindUp()
        {
            if (Time.time < nextStateTime)
            {
                return;
            }

            DealStompDamage();
            BeginRecovery(recoveryDuration);
        }

        private void BeginRecovery(float duration)
        {
            state = BossState.Recovering;
            nextStateTime = Time.time + duration;
            SetTint(normalColor);
        }

        private void UpdateRecovering()
        {
            if (Time.time < nextStateTime)
            {
                return;
            }

            state = BossState.Repositioning;
            nextStateTime = Time.time + 0.5f;
        }

        private void ShootProjectile()
        {
            if (projectilePrefab == null)
            {
                return;
            }

            Vector3 spawnPosition = firePoint.position;
            Vector3 baseShootDirection = GetTargetAimPoint() - spawnPosition;

            if (baseShootDirection.sqrMagnitude < 0.001f)
            {
                return;
            }

            baseShootDirection.Normalize();
            float yaw = Random.Range(-burstSpreadAngle, burstSpreadAngle);
            Vector3 shootDirection = Quaternion.AngleAxis(yaw, Vector3.up) * baseShootDirection;

            proceduralAnimation?.TriggerAttack();
            EnemyProjectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(shootDirection, Vector3.up));
            projectile.Launch(shootDirection, projectileDamage, transform);
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

        private void DealStompDamage()
        {
            proceduralAnimation?.TriggerAttack();

            Collider[] hits = Physics.OverlapSphere(transform.position, stompRange, stompMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform == transform || hits[i].transform.IsChildOf(transform))
                {
                    continue;
                }

                if (!IsWithinHorizontalStompRange(hits[i]))
                {
                    continue;
                }

                IDamageable damageable =
                    hits[i].GetComponent<IDamageable>() ??
                    hits[i].GetComponentInParent<IDamageable>();

                if (damageable == null || !damageable.IsAlive)
                {
                    continue;
                }

                Health targetHealth =
                    hits[i].GetComponent<Health>() ??
                    hits[i].GetComponentInParent<Health>();

                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(stompDamage, this, $"{name} stomp");
                }
            }
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

        private bool HasLineOfSight()
        {
            if (target == null)
            {
                return false;
            }

            Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.75f;
            Vector3 targetPoint = GetTargetAimPoint();
            Vector3 direction = targetPoint - origin;
            float distance = direction.magnitude;

            if (distance <= 0.01f)
            {
                return true;
            }

            direction /= distance;

            if (!Physics.Raycast(origin, direction, out RaycastHit hit, distance, lineOfSightMask, QueryTriggerInteraction.Ignore))
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
            Vector3 radialCorrection = directionToTarget * Mathf.Clamp(radialError, -1f, 1f);
            Vector3 moveDirection = (strafe + radialCorrection * 0.4f).normalized;
            float strafeSpeed = moveSpeed * Mathf.Max(0.75f, strafeSpeedMultiplier);
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
            nextStuckCheckTime = Time.time + 0.4f;

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

        private float GetHorizontalDistanceToTarget()
        {
            if (target == null)
            {
                return float.MaxValue;
            }

            Vector3 closestPoint = GetClosestPointOnTarget(transform.position);
            Vector3 offset = closestPoint - transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        private Vector3 GetClosestPointOnTarget(Vector3 origin)
        {
            CharacterController characterController = target.GetComponent<CharacterController>();

            if (characterController != null)
            {
                return characterController.bounds.ClosestPoint(origin);
            }

            Collider targetCollider = target.GetComponent<Collider>() ?? target.GetComponentInChildren<Collider>();

            if (targetCollider != null)
            {
                return targetCollider.ClosestPoint(origin);
            }

            return target.position;
        }

        private bool IsWithinHorizontalStompRange(Collider other)
        {
            Vector3 closestPoint = other.ClosestPoint(transform.position);
            Vector3 offset = closestPoint - transform.position;
            offset.y = 0f;
            return offset.sqrMagnitude <= stompRange * stompRange;
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = attackColor;
            Gizmos.DrawWireSphere(transform.position, stompRange);
        }
    }
}
