using UnityEngine;

namespace ArenaShooter.CameraSystem
{
    public class TopDownCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 14f, -10f);
        [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.12f;
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);
        [SerializeField, Min(0.1f)] private float rotationLerpSpeed = 10f;

        private Vector3 currentVelocity;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 targetPosition = target.position + worldOffset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, positionSmoothTime);

            Vector3 lookDirection = (target.position + lookAtOffset) - transform.position;

            if (lookDirection.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
        }
    }
}
