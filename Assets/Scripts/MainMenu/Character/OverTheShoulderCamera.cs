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
        public float rotationSmoothTime = 0.08f;
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

        // Inspection Zoom Overrides
        private bool _isInspecting;
        private Transform _inspectTarget;
        private float _inspectZoomDistance = 1.2f;

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

            Vector2 mouseDelta = Vector2.zero;
            if (Mouse.current != null)
            {
                mouseDelta = Mouse.current.delta.ReadValue();
            }

            if (!_isInspecting)
            {
                // Normal Orbiting
                _targetYaw += mouseDelta.x * mouseSensitivity * 0.1f;
                _targetPitch -= mouseDelta.y * mouseSensitivity * 0.1f;
                _targetPitch = Mathf.Clamp(_targetPitch, minYAngle, maxYAngle);
            }
            else
            {
                // Subtle rotation dampening during inspection if mouse moves
                _targetYaw += mouseDelta.x * mouseSensitivity * 0.02f;
                _targetPitch -= mouseDelta.y * mouseSensitivity * 0.02f;
                _targetPitch = Mathf.Clamp(_targetPitch, -15f, 25f);
            }

            _currentYaw = Mathf.SmoothDamp(_currentYaw, _targetYaw, ref _yawVelocity, rotationSmoothTime);
            _currentPitch = Mathf.SmoothDamp(_currentPitch, _targetPitch, ref _pitchVelocity, rotationSmoothTime);

            Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);

            // Compute focus point (inspect target if active, otherwise player)
            Vector3 focusPoint;
            float currentDist;
            float currentSideOffset;
            float currentVertOffset;

            if (_isInspecting && _inspectTarget != null)
            {
                focusPoint = _inspectTarget.position;
                currentDist = _inspectZoomDistance;
                currentSideOffset = 0f; // Center up on the inspected object
                currentVertOffset = 0.1f;
            }
            else
            {
                focusPoint = target.position + Vector3.up * verticalOffset;
                currentDist = distance;
                currentSideOffset = sideOffset;
                currentVertOffset = 0f;
            }

            Vector3 desiredOffset = rotation * new Vector3(currentSideOffset, currentVertOffset, -currentDist);
            Vector3 targetPosition = focusPoint + desiredOffset;

            // Obstacle Collision Avoidance
            Vector3 direction = targetPosition - focusPoint;
            float maxDistance = direction.magnitude;

            if (!_isInspecting && Physics.SphereCast(focusPoint, cameraRadius, direction.normalized, out RaycastHit hit, maxDistance, obstacleLayers))
            {
                targetPosition = focusPoint + direction.normalized * (hit.distance - cameraRadius);
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, 0.05f);
            transform.rotation = rotation;
        }

        public void EnterInspectionMode(Transform inspectTarget, float zoomDistance)
        {
            _isInspecting = true;
            _inspectTarget = inspectTarget;
            _inspectZoomDistance = zoomDistance;
        }

        public void ExitInspectionMode()
        {
            _isInspecting = false;
            _inspectTarget = null;
        }

        public Quaternion GetHorizontalRotation()
        {
            return Quaternion.Euler(0f, _currentYaw, 0f);
        }
    }
}