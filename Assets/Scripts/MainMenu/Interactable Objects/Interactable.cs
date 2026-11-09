using UnityEngine;

namespace Gidigbo.Interaction
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void OnLookEnter();
        void OnLookExit();
        void Interact();
    }
}