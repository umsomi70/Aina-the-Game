
using UnityEngine;
using UnityEngine.InputSystem;
using Gidigbo.Player;
using Gidigbo.UI;

namespace Gidigbo.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ThirdPersonPlayerController playerController;
        [SerializeField] private OverTheShoulderCamera shoulderCamera;
        [SerializeField] private UIManager uiManager;

        [Header("Default Settings")]
        [SerializeField] private float defaultInteractionRange = 4.0f;
        [SerializeField] private LayerMask interactableLayer;

        [Header("Door Settings")]
        [SerializeField] private string doorInteractionPrompt = "Enter / Exit";

        private Camera _cam;
        private IInteractable _currentInteractable;
        private IInteractable _lastInteractable;
        private InspectableObject _inspectedObject;

        private DoorController _currentDoor;
        private DoorController _lastDoor;

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

            if (uiManager == null)
                uiManager = FindFirstObjectByType<UIManager>();
        }

        private void Update()
        {
            HandleInput();

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
            _lastDoor = _currentDoor;

            _currentInteractable = null;
            _currentDoor = null;

            _currentActiveRange = defaultInteractionRange;

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                defaultInteractionRange,
                interactableLayer))
            {
                // Check for inspectable objects first.
                if (hit.collider.TryGetComponent<InspectableObject>(out var inspectableObj))
                {
                    _currentInteractable = inspectableObj;
                    _currentActiveRange = inspectableObj.InteractionRange;
                }
                // Check for generic interactables.
                else if (hit.collider.TryGetComponent<IInteractable>(out var genericInteractable))
                {
                    _currentInteractable = genericInteractable;
                }
                // Check for doors.
                else
                {
                    _currentDoor = hit.collider.GetComponentInParent<DoorController>();
                }
            }

            // Update interaction prompt when target changes.
            if (_currentInteractable != _lastInteractable ||
                _currentDoor != _lastDoor)
            {
                _lastInteractable?.OnLookExit();

                uiManager?.HideLookPrompt();

                _currentInteractable?.OnLookEnter();

                if (_currentInteractable != null)
                {
                    uiManager?.ShowLookPrompt(
                        _currentInteractable.GetInteractionPrompt()
                    );
                }
                else if (_currentDoor != null)
                {
                    uiManager?.ShowLookPrompt(doorInteractionPrompt);
                }
            }
        }

        private void HandleInput()
        {
            bool actionPressed =
                (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
                (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame);

            if (!actionPressed) return;

            // Door interaction.
            if (!_isInspecting && _currentDoor != null)
            {
                if (playerController != null)
                {
                    _currentDoor.Interact(playerController.transform);
                }

                return;
            }

            // Existing inspection interaction.
            if (!_isInspecting && _currentInteractable != null)
            {
                if (_currentInteractable is InspectableObject inspectObj)
                {
                    _inspectedObject = inspectObj;
                    _isInspecting = true;

                    uiManager?.HideLookPrompt();
                    inspectObj.OnInspectEnter();

                    playerController?.SetMovementLocked(true);
                    shoulderCamera?.EnterInspectionMode(
                        inspectObj.Transform,
                        inspectObj.InspectionZoomDistance
                    );

                    // If it's a standard inspectable (not a map), show standard exit UI.
                    if (!(inspectObj is MapLevelSelector))
                    {
                        uiManager?.ShowInspectionUI(false);
                    }
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

            uiManager?.HideInspectionUI();
            playerController?.SetMovementLocked(false);
            shoulderCamera?.ExitInspectionMode();
        }

        private void OnDrawGizmos()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null || _isInspecting) return;

            Gizmos.color =
                _currentInteractable != null || _currentDoor != null
                ? Color.green
                : Color.red;

            Gizmos.DrawRay(
                _cam.transform.position,
                _cam.transform.forward * _currentActiveRange
            );
        }
    }
}
