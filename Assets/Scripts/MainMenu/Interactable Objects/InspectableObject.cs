using UnityEngine;

namespace Gidigbo.Interaction
{
    public class InspectableObject : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string objectName = "Inspectable Item";
        [SerializeField] private MeshRenderer targetMeshRenderer;

        private MaterialPropertyBlock _propertyBlock;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            if (targetMeshRenderer == null)
                targetMeshRenderer = GetComponentInChildren<MeshRenderer>();

            _propertyBlock = new MaterialPropertyBlock();
        }

        public string GetInteractionPrompt()
        {
            return $"Inspect {objectName}";
        }

        public void OnLookEnter()
        {
            // Simple highlight effect: Turn on emission or scale up slightly
            if (targetMeshRenderer != null)
            {
                targetMeshRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(EmissionColor, Color.yellow * 0.5f);
                targetMeshRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        public void OnLookExit()
        {
            // Remove highlight
            if (targetMeshRenderer != null)
            {
                targetMeshRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(EmissionColor, Color.black);
                targetMeshRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        public void Interact()
        {
            Debug.Log($"Interacted with: {objectName}. Ready for Inspection / Rotation / Map UI trigger!");
        }
    }
}