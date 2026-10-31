using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
   
    public static bool InputBlocked;

    public Transform target;           
    public GameObject playerModel;      
    public Vector3 pivotOffset = new Vector3(0f, 0.7f, 0f);

    [Header("Zoom")]
    public bool allowFirstPerson = true;      
    public float minThirdPersonDistance = 2f;   
    public float startDistance = 6f;
    public float maxDistance = 12f;
    public float firstPersonThreshold = 0.8f;
    public float zoomStep = 0.01f;
    public float zoomSmoothing = 10f;

    [Header("Rotation")]
    public float rotateSensitivity = 0.15f;
    public float minPitch = -70f;
    public float maxPitch = 80f;

    [Header("Wall collision")]
    public float minWallDistance = 1f;          

    float yaw, pitch = 15f;
    float targetDistance, currentDistance;

    public bool IsFirstPerson { get; private set; }
    public float Yaw => yaw;

    void Start()
    {
        targetDistance = currentDistance = startDistance;
        yaw = target.eulerAngles.y;
    }

    void LateUpdate()
    {
        var mouse = Mouse.current;
        if (mouse == null || target == null) return;

        bool focused = Application.isFocused && !InputBlocked;
        bool rotating = focused && (mouse.rightButton.isPressed || IsFirstPerson);

        
        Cursor.lockState = rotating ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !rotating;

        if (rotating)
        {
            Vector2 delta = mouse.delta.ReadValue() * rotateSensitivity;
            yaw += delta.x;
            pitch = Mathf.Clamp(pitch - delta.y, minPitch, maxPitch);
        }

        float minDist = allowFirstPerson ? 0f : minThirdPersonDistance;

        if (focused)
        {
            float scroll = mouse.scroll.ReadValue().y;
            targetDistance = Mathf.Clamp(targetDistance - scroll * zoomStep, minDist, maxDistance);
        }

      
        targetDistance = Mathf.Clamp(targetDistance, minDist, maxDistance);

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomSmoothing * Time.deltaTime);
        IsFirstPerson = allowFirstPerson && currentDistance < firstPersonThreshold;
        if (playerModel != null) playerModel.SetActive(!IsFirstPerson);

        Vector3 pivot = target.position + pivotOffset;
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 back = rotation * Vector3.back;

        
        float dist = IsFirstPerson ? 0f : GetClearDistance(pivot, back, currentDistance);

        transform.position = pivot + back * dist;
        transform.rotation = rotation;
    }

    float GetClearDistance(Vector3 pivot, Vector3 dir, float desired)
    {
        float result = desired;
        var hits = Physics.SphereCastAll(pivot, 0.2f, dir, desired, ~0, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (h.transform.IsChildOf(target)) continue;
            if (h.distance < result) result = h.distance;
        }
        return Mathf.Max(result - 0.1f, minWallDistance);
    }
}