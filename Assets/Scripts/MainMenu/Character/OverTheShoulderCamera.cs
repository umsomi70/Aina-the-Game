using UnityEngine;
using UnityEngine.InputSystem;

namespace Gidigbo.Player
{
    public class OverTheShoulderCamera : MonoBehaviour
    {
        [Header("Target Settings")]
        public Transform target;

        [Header("Offsets & Distance")]
        public float sideOffset = 0.75f;
        public float verticalOffset = 1.5f;
        public float distance = 2.5f;

        [Header("Look Sensitivity & Smoothing")]
        public float mouseSensitivity = 2.0f;
        public float rotationSmoothTime = 0.08f; // Smooths out mouse acceleration/snapping
        public float minYAngle = -30.0f;
        public float maxYAngle = 60.0f;

        [Header("Collision")]
        public LayerMask obstacleLayers;
        public float cameraRadius = 0.2f;

        private float _targetYaw;
        private float _targetPitch;
        private float _currentYaw;
        private float _currentPitch;
        private float _yawVelocity;
        private float _pitchVelocity;
        private Vector3 _currentVelocity;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Vector3 euler = transform.eulerAngles;
            _targetYaw = _currentYaw = euler.y;
            _targetPitch = _currentPitch = euler.x;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Read Mouse Delta
            Vector2 mouseDelta = Vector2.zero;
            if (Mouse.current != null)
            {
                mouseDelta = Mouse.current.delta.ReadValue();
            }

            // Accumulate target angles with sensitivity scaling
            _targetYaw += mouseDelta.x * mouseSensitivity * 0.1f;
            _targetPitch -= mouseDelta.y * mouseSensitivity * 0.1f;
            _targetPitch = Mathf.Clamp(_targetPitch, minYAngle, maxYAngle);

            // Smoothly damp the rotation angles to eliminate snapping and stiffness
            _currentYaw = Mathf.SmoothDamp(_currentYaw, _targetYaw, ref _yawVelocity, rotationSmoothTime);
            _currentPitch = Mathf.SmoothDamp(_currentPitch, _targetPitch, ref _pitchVelocity, rotationSmoothTime);

            // Compute Rotation & Desired Position
            Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
            Vector3 focusPoint = target.position + Vector3.up * verticalOffset;
            Vector3 desiredOffset = rotation * new Vector3(sideOffset, 0f, -distance);
            Vector3 targetPosition = focusPoint + desiredOffset;

            // Obstacle Collision Avoidance
            Vector3 direction = targetPosition - focusPoint;
            float maxDistance = direction.magnitude;

            if (Physics.SphereCast(focusPoint, cameraRadius, direction.normalized, out RaycastHit hit, maxDistance, obstacleLayers))
            {
                targetPosition = focusPoint + direction.normalized * (hit.distance - cameraRadius);
            }

            // Smooth position dampening for fluid follow feel
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, 0.04f);
            transform.rotation = rotation;
        }

        public Quaternion GetHorizontalRotation()
        {
            return Quaternion.Euler(0f, _currentYaw, 0f);
        }
    }
}