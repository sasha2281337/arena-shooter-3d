using UnityEngine;

namespace ArenaShooter.Enemies
{
    public class EnemyProceduralAnimation : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform animationTarget;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float movementThreshold = 0.02f;
        [SerializeField, Min(0f)] private float bobAmplitude = 0.05f;
        [SerializeField, Min(0f)] private float bobFrequency = 9f;
        [SerializeField, Min(0f)] private float swayAngle = 5f;
        [SerializeField, Min(0f)] private float settleSpeed = 10f;

        [Header("Attack")]
        [SerializeField, Min(0f)] private float lungeDistance = 0.16f;
        [SerializeField, Min(0.01f)] private float lungeDuration = 0.1f;
        [SerializeField, Min(0.01f)] private float recoverDuration = 0.18f;
        [SerializeField, Min(0f)] private float squashAmount = 0.06f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private Vector3 baseLocalScale;
        private Vector3 previousWorldPosition;
        private Vector3 lastMoveDirection = Vector3.forward;
        private float walkPhase;
        private float moveBlend;
        private float attackTimer = -1f;

        private void Awake()
        {
            if (animationTarget == null)
            {
                animationTarget = ResolveAnimationTarget();
            }

            baseLocalPosition = animationTarget.localPosition;
            baseLocalRotation = animationTarget.localRotation;
            baseLocalScale = animationTarget.localScale;
            previousWorldPosition = transform.position;
        }

        private void OnEnable()
        {
            previousWorldPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (animationTarget == null)
            {
                return;
            }

            float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 worldDelta = transform.position - previousWorldPosition;
            previousWorldPosition = transform.position;

            worldDelta.y = 0f;
            float speed = worldDelta.magnitude / deltaTime;
            bool isMoving = speed > movementThreshold;

            if (isMoving)
            {
                lastMoveDirection = worldDelta.normalized;
                walkPhase += deltaTime * bobFrequency * Mathf.Clamp(speed, 0.8f, 1.6f);
            }

            float targetMoveBlend = isMoving ? 1f : 0f;
            moveBlend = Mathf.MoveTowards(moveBlend, targetMoveBlend, settleSpeed * deltaTime);

            float bob = Mathf.Sin(walkPhase) * bobAmplitude * moveBlend;
            float sway = Mathf.Sin(walkPhase * 0.5f) * swayAngle * moveBlend;
            float foreAft = Mathf.Cos(walkPhase) * bobAmplitude * 0.4f * moveBlend;

            float lungeBlend = EvaluateAttackBlend(deltaTime);
            float squashBlend = lungeBlend * squashAmount;

            animationTarget.localPosition =
                baseLocalPosition +
                new Vector3(0f, bob, foreAft + (lungeBlend * lungeDistance));

            animationTarget.localRotation =
                baseLocalRotation *
                Quaternion.Euler(sway * 0.5f, 0f, -sway);

            animationTarget.localScale = new Vector3(
                baseLocalScale.x * (1f + squashBlend),
                baseLocalScale.y * (1f - squashBlend),
                baseLocalScale.z * (1f + squashBlend * 0.6f));
        }

        public void TriggerAttack()
        {
            attackTimer = 0f;
        }

        public void SetAnimationTargetIfMissing(Transform target)
        {
            if (animationTarget != null || target == null)
            {
                return;
            }

            animationTarget = target;
            baseLocalPosition = animationTarget.localPosition;
            baseLocalRotation = animationTarget.localRotation;
            baseLocalScale = animationTarget.localScale;
            previousWorldPosition = transform.position;
        }

        private float EvaluateAttackBlend(float deltaTime)
        {
            if (attackTimer < 0f)
            {
                return 0f;
            }

            attackTimer += deltaTime;

            if (attackTimer <= lungeDuration)
            {
                float t = attackTimer / lungeDuration;
                return Mathf.SmoothStep(0f, 1f, t);
            }

            float recoveryTime = attackTimer - lungeDuration;

            if (recoveryTime <= recoverDuration)
            {
                float t = recoveryTime / recoverDuration;
                return 1f - Mathf.SmoothStep(0f, 1f, t);
            }

            attackTimer = -1f;
            return 0f;
        }

        private Transform ResolveAnimationTarget()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];

                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (renderer.transform == transform)
                {
                    continue;
                }

                return renderer.transform;
            }

            return transform;
        }
    }
}
