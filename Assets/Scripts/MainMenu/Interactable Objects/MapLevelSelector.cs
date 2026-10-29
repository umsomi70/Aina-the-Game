using UnityEngine;
using UnityEngine.InputSystem;

namespace Gidigbo.Interaction
{
    public class MapLevelSelector : InspectableObject
    {
        [System.Serializable]
        public struct LevelPin
        {
            public string levelName;
            public Transform pinTransform;
            [HideInInspector] public Vector3 originalScale;
            [HideInInspector] public MeshRenderer meshRenderer;
            [HideInInspector] public Material material;
            [HideInInspector] public Color originalColor;
        }

        [Header("Map & Level Pins")]
        [SerializeField] private LevelPin[] levelPins;
        [SerializeField] private float selectedScaleMultiplier = 1.3f;
        [SerializeField] private Color selectedPinColor = Color.green;

        private int _currentIndex = 0;
        private bool _isSelectorActive = false;

        protected override void Awake()
        {
            base.Awake();

            // Cache original scales and materials for each pin
            for (int i = 0; i < levelPins.Length; i++)
            {
                if (levelPins[i].pinTransform != null)
                {
                    levelPins[i].originalScale = levelPins[i].pinTransform.localScale;
                    levelPins[i].meshRenderer = levelPins[i].pinTransform.GetComponentInChildren<MeshRenderer>();
                    if (levelPins[i].meshRenderer != null)
                    {
                        levelPins[i].material = levelPins[i].meshRenderer.material;
                        if (levelPins[i].material.HasProperty("_Color"))
                            levelPins[i].originalColor = levelPins[i].material.color;
                        else if (levelPins[i].material.HasProperty("_BaseColor"))
                            levelPins[i].originalColor = levelPins[i].material.GetColor("_BaseColor");
                    }
                }
            }
        }

        public override void OnInspectEnter()
        {
            _isSelectorActive = true;
            _currentIndex = 0;
            UpdatePinVisuals();
        }

        public override void OnInspectExit()
        {
            _isSelectorActive = false;
            ResetPinVisuals();
        }

        public void HandleMapInput()
        {
            if (!_isSelectorActive || levelPins.Length == 0) return;

            int oldIndex = _currentIndex;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
                {
                    _currentIndex--;
                    if (_currentIndex < 0) _currentIndex = levelPins.Length - 1;
                }
                if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
                {
                    _currentIndex++;
                    if (_currentIndex >= levelPins.Length) _currentIndex = 0;
                }
            }

            if (oldIndex != _currentIndex)
            {
                UpdatePinVisuals();
                Debug.Log($"Selected Level: {levelPins[_currentIndex].levelName}");
            }
        }

        private void UpdatePinVisuals()
        {
            for (int i = 0; i < levelPins.Length; i++)
            {
                if (levelPins[i].pinTransform == null) continue;

                if (i == _currentIndex)
                {
                    // Scale up and tint active pin
                    levelPins[i].pinTransform.localScale = levelPins[i].originalScale * selectedScaleMultiplier;
                    if (levelPins[i].material != null)
                    {
                        if (levelPins[i].material.HasProperty("_Color")) levelPins[i].material.color = selectedPinColor;
                        if (levelPins[i].material.HasProperty("_BaseColor")) levelPins[i].material.SetColor("_BaseColor", selectedPinColor);
                    }
                }
                else
                {
                    // Revert unselected pins
                    levelPins[i].pinTransform.localScale = levelPins[i].originalScale;
                    if (levelPins[i].material != null)
                    {
                        if (levelPins[i].material.HasProperty("_Color")) levelPins[i].material.color = levelPins[i].originalColor;
                        if (levelPins[i].material.HasProperty("_BaseColor")) levelPins[i].material.SetColor("_BaseColor", levelPins[i].originalColor);
                    }
                }
            }
        }

        private void ResetPinVisuals()
        {
            for (int i = 0; i < levelPins.Length; i++)
            {
                if (levelPins[i].pinTransform == null) continue;
                levelPins[i].pinTransform.localScale = levelPins[i].originalScale;
                if (levelPins[i].material != null)
                {
                    if (levelPins[i].material.HasProperty("_Color")) levelPins[i].material.color = levelPins[i].originalColor;
                    if (levelPins[i].material.HasProperty("_BaseColor")) levelPins[i].material.SetColor("_BaseColor", levelPins[i].originalColor);
                }
            }
        }
    }
}