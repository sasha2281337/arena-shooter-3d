using UnityEngine;
using UnityEngine.InputSystem;
using System;
using ArenaShooter.Combat;

namespace ArenaShooter.Player
{
    public class PlayerAim : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private LayerMask aimMask = ~0;
        [SerializeField, Min(1f)] private float maxAimDistance = 200f;
        [SerializeField, Min(1f)] private float fallbackDistance = 30f;
        [SerializeField, Min(1f)] private float rotationSpeed = 20f;

        public Vector3 CurrentAimPoint { get; private set; }
        private Health ownHealth;

        private void Awake()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            ownHealth = GetComponent<Health>();
        }

        private void Update()
        {
            if (worldCamera == null)
            {
                return;
            }

            CurrentAimPoint = ResolveAimPoint();
            RotateTowardsAimPoint();
        }

        private Vector3 ResolveAimPoint()
        {
            if (Mouse.current == null)
            {
                return transform.position + transform.forward * fallbackDistance;
            }

            Ray screenRay = worldCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            RaycastHit[] hits = Physics.RaycastAll(screenRay, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore);

            if (hits.Length > 0)
            {
                Array.Sort(hits, static (left, right) => left.distance.CompareTo(right.distance));

                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].transform == transform || hits[i].transform.IsChildOf(transform))
                    {
                        continue;
                    }

                    Health hitHealth =
                        hits[i].transform.GetComponent<Health>() ??
                        hits[i].transform.GetComponentInParent<Health>();

                    if (hitHealth != null && hitHealth == ownHealth)
                    {
                        continue;
                    }

                    return hits[i].point;
                }
            }

            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

            if (groundPlane.Raycast(screenRay, out float enterDistance))
            {
                return screenRay.GetPoint(enterDistance);
            }

            return transform.position + transform.forward * fallbackDistance;
        }

        private void RotateTowardsAimPoint()
        {
            Vector3 direction = CurrentAimPoint - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
