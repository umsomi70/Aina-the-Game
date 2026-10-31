using UnityEngine;
using TMPro;

public class Interactor : MonoBehaviour
{
    public float range = 3f;
    public KeyCode interactKey = KeyCode.E;
    public GameObject promptObject;
    public TextMeshProUGUI promptLabel;

    void Update()
    {
        IInteractable target = null;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range))
            target = hit.collider.GetComponentInParent<IInteractable>();

        if (target != null)
        {
            promptObject.SetActive(true);
            promptLabel.text = $"Press E to {target.PromptText}";

            if (Input.GetKeyDown(interactKey))
                target.Interact();
        }
        else
        {
            promptObject.SetActive(false);
        }
    }
}