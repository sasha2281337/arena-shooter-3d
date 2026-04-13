using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaShooter.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string attackActionName = "Attack";
        [SerializeField] private string previousWeaponActionName = "Previous";
        [SerializeField] private string nextWeaponActionName = "Next";
        [SerializeField] private Key reloadKey = Key.R;

        public event Action FirePressed;
        public event Action FireReleased;
        public event Action ReloadPressed;
        public event Action NextWeaponPressed;
        public event Action PreviousWeaponPressed;
        public event Action<int> WeaponSlotPressed;

        public Vector2 MoveInput => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        public bool IsFireHeld => attackAction != null && attackAction.IsPressed();

        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction attackAction;
        private InputAction previousWeaponAction;
        private InputAction nextWeaponAction;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();

            if (playerInput.actions == null)
            {
                Debug.LogError("PlayerInputReader requires a PlayerInput component with an assigned Input Actions asset.", this);
                enabled = false;
                return;
            }

            moveAction = playerInput.actions[moveActionName];
            attackAction = playerInput.actions[attackActionName];
            previousWeaponAction = playerInput.actions[previousWeaponActionName];
            nextWeaponAction = playerInput.actions[nextWeaponActionName];

            if (moveAction == null || attackAction == null)
            {
                Debug.LogError($"Missing input actions. Expected '{moveActionName}' and '{attackActionName}'.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (attackAction != null)
            {
                attackAction.performed += OnAttackPerformed;
                attackAction.canceled += OnAttackCanceled;
            }

            if (previousWeaponAction != null)
            {
                previousWeaponAction.performed += OnPreviousWeaponPerformed;
            }

            if (nextWeaponAction != null)
            {
                nextWeaponAction.performed += OnNextWeaponPerformed;
            }
        }

        private void OnDisable()
        {
            if (attackAction != null)
            {
                attackAction.performed -= OnAttackPerformed;
                attackAction.canceled -= OnAttackCanceled;
            }

            if (previousWeaponAction != null)
            {
                previousWeaponAction.performed -= OnPreviousWeaponPerformed;
            }

            if (nextWeaponAction != null)
            {
                nextWeaponAction.performed -= OnNextWeaponPerformed;
            }
        }

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current[reloadKey].wasPressedThisFrame)
            {
                ReloadPressed?.Invoke();
            }

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                WeaponSlotPressed?.Invoke(0);
            }

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                WeaponSlotPressed?.Invoke(1);
            }
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            FirePressed?.Invoke();
        }

        private void OnAttackCanceled(InputAction.CallbackContext context)
        {
            FireReleased?.Invoke();
        }

        private void OnPreviousWeaponPerformed(InputAction.CallbackContext context)
        {
            PreviousWeaponPressed?.Invoke();
        }

        private void OnNextWeaponPerformed(InputAction.CallbackContext context)
        {
            NextWeaponPressed?.Invoke();
        }
    }
}
