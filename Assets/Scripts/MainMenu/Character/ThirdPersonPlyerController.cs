using UnityEngine;
using UnityEngine.InputSystem;

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
        public float acceleration = 12.0f;
        public float rotationSpeed = 15.0f;

        [Header("Gravity & Grounding")]
        public float gravity = -19.62f;
        public float groundedOffset = -0.1f;
        public float groundedRadius = 0.28f;
        public LayerMask groundLayers;

        private CharacterController _controller;
        private Vector3 _currentVelocity;
        private float _verticalVelocity;
        private bool _isGrounded;
        private Vector3 _moveInputDirection;
        private bool _isSprinting;

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
            // Gather input in Update to prevent input polling loss between physics frames
            GatherInput();
        }

        private void FixedUpdate()
        {
            CheckGrounded();
            HandleMovement();
        }

        private void GatherInput()
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            }

            _moveInputDirection = new Vector3(horizontal, 0f, vertical).normalized;
            _isSprinting = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        }

        private void CheckGrounded()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
            _isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

            if (_isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -2.0f;
            }
        }

        private void HandleMovement()
        {
            float targetSpeed = _isSprinting ? runSpeed : walkSpeed;
            if (_moveInputDirection.magnitude < 0.1f)
            {
                targetSpeed = 0f;
            }

            // Align Movement Direction to Camera Facing Vector
            Quaternion cameraYaw = mainCamera != null ? mainCamera.GetHorizontalRotation() : Quaternion.identity;
            Vector3 moveDirection = cameraYaw * _moveInputDirection;

            // Character Rotation
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }

            // Horizontal Movement Smoothing
            Vector3 targetVelocity = moveDirection * targetSpeed;
            _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

            // Gravity Calculation
            _verticalVelocity += gravity * Time.fixedDeltaTime;

            // Apply Move Vector using FixedDeltaTime for smooth physics motion
            Vector3 finalMove = _currentVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(finalMove * Time.fixedDeltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
            Gizmos.DrawWireSphere(spherePosition, groundedRadius);
        }
    }
}