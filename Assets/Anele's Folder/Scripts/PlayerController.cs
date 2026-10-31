using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 5.5f;
    public float acceleration = 12f;     // higher = snappier

    [Header("Look")]
    public Transform cameraTransform;    // drag Main Camera here
    public float lookSensitivity = 0.08f;
    public float maxLookAngle = 85f;

    [Header("Zoom (field of view)")]
    public float defaultFOV = 60f;
    public float minFOV = 30f;
    public float maxFOV = 70f;
    public float zoomStep = 0.02f;
    public float zoomSmoothing = 10f;

    CharacterController controller;
    Camera cam;
    Vector3 currentVelocity;
    float pitch;
    float verticalVelocity;
    float targetFOV;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = cameraTransform.GetComponent<Camera>();
        targetFOV = defaultFOV;
        cam.fieldOfView = defaultFOV;
        SetCursorLocked(true);
    }

    void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        // Cursor lock: Esc frees it, click locks it again
        if (kb.escapeKey.wasPressedThisFrame) SetCursorLocked(false);
        if (mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            SetCursorLocked(true);

        bool locked = Cursor.lockState == CursorLockMode.Locked;

        // Look (no deltaTime: mouse delta is already per-frame movement)
        if (locked)
        {
            Vector2 look = mouse.delta.ReadValue() * lookSensitivity;
            transform.Rotate(0f, look.x, 0f);
            pitch = Mathf.Clamp(pitch - look.y, -maxLookAngle, maxLookAngle);
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            // Zoom target
            float scroll = mouse.scroll.ReadValue().y;
            targetFOV = Mathf.Clamp(targetFOV - scroll * zoomStep, minFOV, maxFOV);
        }
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, zoomSmoothing * Time.deltaTime);

        // Move
        float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        Vector3 input = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);

        float speed = kb.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;
        Vector3 targetVelocity = transform.TransformDirection(input) * speed;

        // Ease toward the target speed instead of snapping
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);

        // Gravity
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += -9.81f * Time.deltaTime;

        Vector3 finalMove = currentVelocity + Vector3.up * verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);
    }

    void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void OnApplicationFocus(bool hasFocus)
{
    if (!hasFocus)
        SetCursorLocked(false);
}
}