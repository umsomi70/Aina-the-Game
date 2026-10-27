using UnityEngine;

namespace Gidigbo.Interaction
{
    public class InspectableObject : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string objectName = "Inspectable Item";
        [Tooltip("How close the player needs to be to interact with this specific object.")]
        [SerializeField] private float customInteractionRange = 4.0f;

        [Header("Visual Feedback (Highlight)")]
        [SerializeField] private MeshRenderer targetMeshRenderer;
        [SerializeField] private Color highlightTint = Color.yellow;

        private Material _material;
        private Color _originalColor;

        public float InteractionRange => customInteractionRange;

        private void Awake()
        {
            if (targetMeshRenderer == null)
                targetMeshRenderer = GetComponentInChildren<MeshRenderer>();

            if (targetMeshRenderer != null)
            {
                // Grab instance material so we don't accidentally edit asset colors globally
                _material = targetMeshRenderer.material;
                if (_material.HasProperty("_Color"))
                {
                    _originalColor = _material.color;
                }
                else if (_material.HasProperty("_BaseColor")) // URP compatibility
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
                // Tint the object's color to show highlight
                Color targetColor = _originalColor * highlightTint;
                if (_material.HasProperty("_Color")) _material.color = targetColor;
                if (_material.HasProperty("_BaseColor")) _material.SetColor("_BaseColor", targetColor);
            }
        }

        public void OnLookExit()
        {
            if (_material != null)
            {
                // Revert to original color
                if (_material.HasProperty("_Color")) _material.color = _originalColor;
                if (_material.HasProperty("_BaseColor")) _material.SetColor("_BaseColor", _originalColor);
            }
        }

        public void Interact()
        {
            Debug.Log($"Interacted with: {objectName}. Ready for Inspection / Rotation / Map UI trigger!");
        }
    }
}