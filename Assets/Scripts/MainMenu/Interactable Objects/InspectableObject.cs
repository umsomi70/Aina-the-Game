using UnityEngine;

namespace Gidigbo.Interaction
{
    public class InspectableObject : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string objectName = "Inspectable Item";
        [SerializeField] private float customInteractionRange = 4.0f;
        [SerializeField] private float inspectionZoomDistance = 1.0f;

        [Header("Visual Feedback (Highlight)")]
        [SerializeField] private MeshRenderer targetMeshRenderer;
        [SerializeField] private Color highlightTint = Color.yellow;

        private Material _material;
        private Color _originalColor;

        public float InteractionRange => customInteractionRange;
        public float InspectionZoomDistance => inspectionZoomDistance;
        public Transform Transform => transform;

        private void Awake()
        {
            if (targetMeshRenderer == null)
                targetMeshRenderer = GetComponentInChildren<MeshRenderer>();

            if (targetMeshRenderer != null)
            {
                _material = targetMeshRenderer.material;
                if (_material.HasProperty("_Color"))
                {
                    _originalColor = _material.color;
                }
                else if (_material.HasProperty("_BaseColor"))
                {
                    _originalColor = _material.GetColor("_BaseColor");
                }
            }
        }

        public string GetInteractionPrompt()
        {
            return $"Inspect {objectName}";
        }

        public void OnLookEnter()
        {
            if (_material != null)
            {
                Color targetColor = _originalColor * highlightTint;
                if (_material.HasProperty("_Color")) _material.color = targetColor;
                if (_material.HasProperty("_BaseColor")) _material.SetColor("_BaseColor", targetColor);
            }
        }

        public void OnLookExit()
        {
            if (_material != null)
            {
                if (_material.HasProperty("_Color")) _material.color = _originalColor;
                if (_material.HasProperty("_BaseColor")) _material.SetColor("_BaseColor", _originalColor);
            }
        }

        public void Interact()
        {
            // Handled via PlayerInteractor state machine toggle
        }
    }
}