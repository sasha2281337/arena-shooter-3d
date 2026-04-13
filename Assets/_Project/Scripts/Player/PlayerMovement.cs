using ArenaShooter.Upgrades;
using UnityEngine;

namespace ArenaShooter.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerUpgradeController upgradeController;
        [SerializeField] private Transform cameraTransform;
        [SerializeField, Min(0.1f)] private float moveSpeed = 7f;
        [SerializeField, Min(0.1f)] private float acceleration = 20f;
        [SerializeField, Min(0.1f)] private float deceleration = 24f;
        [SerializeField] private float gravity = -20f;

        public Vector3 WorldMoveDirection { get; private set; }

        private CharacterController characterController;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (upgradeController == null)
            {
                upgradeController = GetComponent<PlayerUpgradeController>();
            }

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (inputReader == null || cameraTransform == null)
            {
                return;
            }

            Vector2 moveInput = inputReader.MoveInput;
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            WorldMoveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x);

            if (WorldMoveDirection.sqrMagnitude > 1f)
            {
                WorldMoveDirection.Normalize();
            }

            float speedMultiplier = upgradeController != null ? upgradeController.MoveSpeedMultiplier : 1f;
            Vector3 targetHorizontalVelocity = WorldMoveDirection * moveSpeed * speedMultiplier;
            float changeRate = WorldMoveDirection.sqrMagnitude > 0.001f ? acceleration : deceleration;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetHorizontalVelocity, changeRate * Time.deltaTime);

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
            characterController.Move(finalVelocity * Time.deltaTime);
        }
    }
}
