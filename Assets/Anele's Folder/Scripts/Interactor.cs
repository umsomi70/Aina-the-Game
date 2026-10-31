using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Interactor : MonoBehaviour
{
    public float range = 2.5f;
    public GameObject promptObject;
    public TextMeshProUGUI promptLabel;

    void Update()
    {
        IInteractable best = null;
        float bestDist = float.MaxValue;

        var cols = Physics.OverlapSphere(transform.position, range, ~0, QueryTriggerInteraction.Collide);
        foreach (var col in cols)
        {
            var i = col.GetComponentInParent<IInteractable>();
            if (i == null) continue;

            float d = Vector3.Distance(transform.position, col.ClosestPoint(transform.position));
            if (d < bestDist) { bestDist = d; best = i; }
        }

        bool show = best != null && !CameraController.InputBlocked;
        if (promptObject != null) promptObject.SetActive(show);

        if (show)
        {
            if (promptLabel != null) promptLabel.text = $"Press E to {best.PromptText}";
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                best.Interact();
        }
    }
}