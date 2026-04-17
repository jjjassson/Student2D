using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Drunk2 : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    private bool inputLocked;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (inputLocked) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (inputLocked) return;

        if (context.performed && groundedPlayer)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravityValue);
    }

    private void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && velocity.y < 0) velocity.y = 0f;

        Vector3 move = new Vector3(moveInput.x, 0, 0);
        controller.Move(move * Time.deltaTime * moveSpeed);

        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ⭐⭐⭐ 關鍵：鎖死朝向攝影機 ⭐⭐⭐
        LockToCamera();
    }

    // ⭐⭐⭐ 核心（不再用 LookRotation）⭐⭐⭐
    void LockToCamera()
    {
        if (Camera.main == null) return;

        Vector3 camEuler = Camera.main.transform.eulerAngles;

        // 只跟著 Y 軸（水平）
        transform.rotation = Quaternion.Euler(0f, camEuler.y, 0f);
    }

    // ===== 被附身時接收 Input =====
    public void ReceivePossessMove(Vector2 input)
    {
        inputLocked = true;
        moveInput = input;
    }

    public void ReceivePossessJump()
    {
        if (groundedPlayer)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravityValue);
    }

    public void EndPossession()
    {
        inputLocked = false;
    }
}