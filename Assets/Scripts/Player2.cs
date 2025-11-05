using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player2 : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    // 🧩 狀態
    [HideInInspector] public bool isSlowed = false;
    [HideInInspector] public bool isJumpReduced = false;

    // 🧩 預設參數記錄
    private float defaultMoveSpeed;
    private float defaultJumpForce;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
    }

    // ===== 接收 SlowZone / LowJumpZone 呼叫 =====
    public void ApplySpeedMultiplier(float multiplier)
    {
        moveSpeed = defaultMoveSpeed * multiplier;
        isSlowed = multiplier < 1f;
    }

    public void ApplyJumpMultiplier(float multiplier)
    {
        jumpForce = defaultJumpForce * multiplier;
        isJumpReduced = multiplier < 1f;
    }

    public void ResetSpeed()
    {
        moveSpeed = defaultMoveSpeed;
        isSlowed = false;
    }

    public void ResetJump()
    {
        jumpForce = defaultJumpForce;
        isJumpReduced = false;
    }

    // ===== 玩家輸入 =====
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && groundedPlayer)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravityValue);
        }
    }

    // ===== 更新移動 =====
    private void Update()
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && velocity.y < 0)
            velocity.y = 0f;

        // 🧭 只允許 X 軸移動
        Vector3 move = new Vector3(moveInput.x, 0, 0);
        controller.Move(move * Time.deltaTime * moveSpeed);

        // 🌀 重力
        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 🧭 面向左右移動方向
        if (Mathf.Abs(move.x) > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                720f * Time.deltaTime
            );
        }
    }
}