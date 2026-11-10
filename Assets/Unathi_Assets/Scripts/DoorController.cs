
using UnityEngine;

namespace Gidigbo.Interaction
{
    public class DoorController : MonoBehaviour
    {
        [Header("Door Positions")]
        [SerializeField] private Transform inPosition;
        [SerializeField] private Transform outPosition;

        public void Interact(Transform player)
        {
            if (player == null || inPosition == null || outPosition == null)
            {
                Debug.LogWarning(
                    $"DoorController on {gameObject.name}: Missing player or door positions."
                );
                return;
            }

            // Compare the player's distance to each side of the door.
            float distanceToIn =
                Vector3.Distance(player.position, inPosition.position);

            float distanceToOut =
                Vector3.Distance(player.position, outPosition.position);

            // The player should travel to the opposite side.
            Transform destination = distanceToIn < distanceToOut
                ? outPosition
                : inPosition;

            CharacterController characterController =
                player.GetComponent<CharacterController>();

            if (characterController != null)
                characterController.enabled = false;

            // Change position only. Preserve the player's rotation.
            player.position = destination.position;

            if (characterController != null)
                characterController.enabled = true;
        }
    }
}
