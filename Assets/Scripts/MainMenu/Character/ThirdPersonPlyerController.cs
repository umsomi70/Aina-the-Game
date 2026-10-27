using UnityEngine;
using UnityEngine.InputSystem; // Added New Input System namespace

namespace Gidigbo.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonPlayerController : MonoBehaviour
    {
        [Header("References")]
        public OverTheShoulderCamera mainCamera;

        [Header("Movement Speeds")]
        public float walkSpeed = 2.5f;
        public float runSpeed = 5.0f;
        public float acceleration = 10.0f;
        public float rotationSpeed = 12.0f;

        [Header("Gravity & Grounding")]
        public float gravity = -19.62f;
        public float groundedOffset = -0.1f;
        public float groundedRadius = 0.28f;
        public LayerMask groundLayers;

        private CharacterController _controller;
        private Vector3 _currentVelocity;
        private float _verticalVelocity;
        private bool _isGrounded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (mainCamera == null && Camera.main != null)
            {
                mainCamera = Camera.main.GetComponent<OverTheShoulderCamera>();
            }
        }

        private void Update()
        {
            CheckGrounded();
            HandleMovement();
        }

        private void CheckGrounded()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
            _isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

            if (_isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -2.0f; // Small constant downward force to stay sticked to ground
            }
        }

        private void HandleMovement()
        {
            // Input Collection via New Input System (WASD / Left Stick)
            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            }

            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

            // Sprint check (Left Shift)
            bool isSprinting = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
            float targetSpeed = isSprinting ? runSpeed : walkSpeed;

            if (inputDirection.magnitude < 0.1f)
            {
                targetSpeed = 0f;
            }

            // Align Movement Direction to Camera Facing Vector
            Quaternion cameraYaw = mainCamera != null ? mainCamera.GetHorizontalRotation() : Quaternion.identity;
            Vector3 moveDirection = cameraYaw * inputDirection;

            // Character Rotation (Rotates character towards camera look direction when moving)
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Horizontal Movement Smoothing
            Vector3 targetVelocity = moveDirection * targetSpeed;
            _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, acceleration * Time.deltaTime);

            // Gravity Calculation
            _verticalVelocity += gravity * Time.deltaTime;

            // Apply Move Vector
            Vector3 finalMove = _currentVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(finalMove * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize Grounded Sphere
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
            Gizmos.DrawWireSphere(spherePosition, groundedRadius);
        }
    }
}