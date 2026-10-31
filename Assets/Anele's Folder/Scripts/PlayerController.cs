using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public CameraController cameraController;  

    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float acceleration = 14f;
    public float turnSpeed = 12f;
    
    

    CharacterController controller;
    Vector3 currentVelocity;
    float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        Vector3 input = Vector3.zero;
        if (!CameraController.InputBlocked)
        {
            float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
            input = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);
        }

        
        Quaternion camYaw = Quaternion.Euler(0f, cameraController.Yaw, 0f);
        Vector3 moveDir = camYaw * input;

        float speed = kb.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;
        currentVelocity = Vector3.Lerp(currentVelocity, moveDir * speed, acceleration * Time.deltaTime);

        
        Quaternion targetRot = transform.rotation;
        if (cameraController.IsFirstPerson)
            targetRot = camYaw;
        else if (moveDir.sqrMagnitude > 0.01f)
            targetRot = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

        
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += -9.81f * Time.deltaTime;

        controller.Move((currentVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
    }
}