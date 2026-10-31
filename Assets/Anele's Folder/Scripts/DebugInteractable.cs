using UnityEngine;

public class DebugInteractable : MonoBehaviour, IInteractable
{
    public string promptText = "inspect";
    public string PromptText => promptText;

    public void Interact()
    {
        Debug.Log($"Interacted with {gameObject.name}");
    }
}