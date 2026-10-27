using UnityEngine;
using UnityEngine.InputSystem;

namespace Gidigbo.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Default Settings")]
        [SerializeField] private float defaultInteractionRange = 4.0f;
        [SerializeField] private LayerMask interactableLayer;

        private Camera _cam;
        private IInteractable _currentInteractable;
        private IInteractable _lastInteractable;
        private float _currentActiveRange;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = Camera.main;
            _currentActiveRange = defaultInteractionRange;
        }

        private void Update()
        {
            CheckForInteractables();
            HandleInput();
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

            // Handle look enter/exit callbacks
            if (_currentInteractable != _lastInteractable)
            {
                _lastInteractable?.OnLookExit();
                _currentInteractable?.OnLookEnter();
            }
        }

        private void HandleInput()
        {
            bool interactPressed = (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
                                  (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

            if (interactPressed && _currentInteractable != null)
            {
                _currentInteractable.Interact();
            }
        }

        private void OnDrawGizmos()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;

            Gizmos.color = _currentInteractable != null ? Color.green : Color.red;
            Gizmos.DrawRay(_cam.transform.position, _cam.transform.forward * _currentActiveRange);
        }
    }
}