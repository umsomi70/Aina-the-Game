using UnityEngine;
using UnityEngine.InputSystem;

namespace Gidigbo.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private float interactionRange = 3.0f;
        [SerializeField] private LayerMask interactableLayer;

        private Camera _cam;
        private IInteractable _currentInteractable;
        private IInteractable _lastInteractable;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = Camera.main;
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

            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
            {
                if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    _currentInteractable = interactable;
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
            // Press 'E' or Left Mouse Click to interact
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
            Gizmos.DrawRay(_cam.transform.position, _cam.transform.forward * interactionRange);
        }
    }
}