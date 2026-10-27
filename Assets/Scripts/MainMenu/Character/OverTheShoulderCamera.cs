using UnityEngine;
using UnityEngine.InputSystem; // Added New Input System namespace

namespace Gidigbo.Player
{
    public class OverTheShoulderCamera : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("Target transform for the camera to follow (e.g., Player object or head/chest pivot).")]
        public Transform target;

        [Header("Offsets & Distance")]
        [Tooltip("Horizontal offset: Positive = Right shoulder, Negative = Left shoulder.")]
        public float sideOffset = 0.75f;
        [Tooltip("Vertical offset relative to the target.")]
        public float verticalOffset = 1.5f;
        [Tooltip("Distance behind the target.")]
        public float distance = 2.5f;

        [Header("Look Sensitivity")]
        public float mouseSensitivityX = 0.1f;
        public float mouseSensitivityY = 0.1f;
        public float minYAngle = -30.0f;
        public float maxYAngle = 60.0f;

        [Header("Smoothing & Collision")]
        public float cameraSmoothTime = 0.05f;
        public LayerMask obstacleLayers;
        public float cameraRadius = 0.2f;

        private float _currentYaw;
        private float _currentPitch;
        private Vector3 _currentVelocity;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Vector3 euler = transform.eulerAngles;
            _currentYaw = euler.y;
            _currentPitch = euler.x;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Read Mouse Delta via New Input System
            Vector2 mouseDelta = Vector2.zero;
            if (Mouse.current != null)
            {
                mouseDelta = Mouse.current.delta.ReadValue();
            }

            // Handle Look Input
            _currentYaw += mouseDelta.x * mouseSensitivityX;
            _currentPitch -= mouseDelta.y * mouseSensitivityY;
            _currentPitch = Mathf.Clamp(_currentPitch, minYAngle, maxYAngle);

            // Compute Rotation & Desired Position
            Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
            Vector3 focusPoint = target.position + Vector3.up * verticalOffset;
            Vector3 desiredOffset = rotation * new Vector3(sideOffset, 0f, -distance);
            Vector3 targetPosition = focusPoint + desiredOffset;

            // Basic Obstacle Collision Avoidance
            Vector3 direction = targetPosition - focusPoint;
            float maxDistance = direction.magnitude;

            if (Physics.SphereCast(focusPoint, cameraRadius, direction.normalized, out RaycastHit hit, maxDistance, obstacleLayers))
            {
                targetPosition = focusPoint + direction.normalized * (hit.distance - cameraRadius);
            }

            // Smooth Interpolation
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, cameraSmoothTime);
            transform.rotation = rotation;
        }

        public Quaternion GetHorizontalRotation()
        {
            return Quaternion.Euler(0f, _currentYaw, 0f);
        }
    }
}