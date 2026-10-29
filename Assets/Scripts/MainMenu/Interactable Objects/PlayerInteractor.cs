using UnityEngine;
using UnityEngine.InputSystem;
using Gidigbo.Player;

namespace Gidigbo.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ThirdPersonPlayerController playerController;
        [SerializeField] private OverTheShoulderCamera shoulderCamera;

        [Header("Default Settings")]
        [SerializeField] private float defaultInteractionRange = 4.0f;
        [SerializeField] private LayerMask interactableLayer;

        private Camera _cam;
        private IInteractable _currentInteractable;
        private IInteractable _lastInteractable;
        private InspectableObject _inspectedObject;
        private float _currentActiveRange;
        private bool _isInspecting;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = Camera.main;
            _currentActiveRange = defaultInteractionRange;

            if (playerController == null)
                playerController = FindFirstObjectByType<ThirdPersonPlayerController>();
            if (shoulderCamera == null && _cam != null)
                shoulderCamera = _cam.GetComponent<OverTheShoulderCamera>();
        }

        private void Update()
        {
            HandleInput();

            // Poll map scrolling inputs if currently inspecting a MapLevelSelector
            if (_isInspecting && _inspectedObject is MapLevelSelector mapSelector)
            {
                mapSelector.HandleMapInput();
            }
            else if (!_isInspecting)
            {
                CheckForInteractables();
            }
        }

        private void CheckForInteractables()
        {
            Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
            _lastInteractable = _currentInteractable;
            _currentInteractable = null;
            _currentActiveRange = defaultInteractionRange;

            if (Physics.Raycast(ray, out RaycastHit hit, defaultInteractionRange, interactableLayer))
            {
                if (hit.collider.TryGetComponent<InspectableObject>(out var inspectableObj))
                {
                    _currentInteractable = inspectableObj;
                    _currentActiveRange = inspectableObj.InteractionRange;
                }
                else if (hit.collider.TryGetComponent<IInteractable>(out var genericInteractable))
                {
                    _currentInteractable = genericInteractable;
                }
            }

            if (_currentInteractable != _lastInteractable)
            {
                _lastInteractable?.OnLookExit();
                _currentInteractable?.OnLookEnter();
            }
        }

        private void HandleInput()
        {
            bool actionPressed = (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
                                 (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame);

            if (!actionPressed) return;

            if (!_isInspecting && _currentInteractable != null)
            {
                if (_currentInteractable is InspectableObject inspectObj)
                {
                    _inspectedObject = inspectObj;
                    _isInspecting = true;

                    inspectObj.OnInspectEnter();
                    playerController?.SetMovementLocked(true);
                    shoulderCamera?.EnterInspectionMode(inspectObj.Transform, inspectObj.InspectionZoomDistance);
                }
            }
            else if (_isInspecting)
            {
                ToggleExitInspection();
            }
        }

        public void ToggleExitInspection()
        {
            if (_inspectedObject != null)
            {
                _inspectedObject.OnInspectExit();
            }

            _isInspecting = false;
            _inspectedObject = null;

            playerController?.SetMovementLocked(false);
            shoulderCamera?.ExitInspectionMode();
        }

        private void OnDrawGizmos()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null || _isInspecting) return;

            Gizmos.color = _currentInteractable != null ? Color.green : Color.red;
            Gizmos.DrawRay(_cam.transform.position, _cam.transform.forward * _currentActiveRange);
        }
    }
}