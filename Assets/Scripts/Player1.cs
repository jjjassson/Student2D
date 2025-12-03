using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player1 : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    [Header("位置限制")]
    [Tooltip("玩家 Z 軸能到達的最大值 (不能超過這條線)")]
    public float maxZPosition = -3f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    // 🧩 狀態
    [HideInInspector] public bool isSlowed = false;
    [HideInInspector] public bool isJumpReduced = false;

    // 🧩 全方向反轉狀態（🆕 新增）
    [HideInInspector] public bool isInverted = false;

    // 🧩 預設參數記錄
    private float defaultMoveSpeed;
    private float defaultJumpForce;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
    }

    // ============================================================
    // 原本保留的功能（完全未動）
    // ============================================================
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

    // ============================================================
    // 🆕 新增功能：移動方向反轉
    // ============================================================
    public void InvertMovement()
    {
        isInverted = true;
    }

    public void ResetInverted()
    {
        isInverted = false;
    }

    // ============================================================
    // 更新移動（原本的程式 + 反向邏輯）
    // ============================================================
    private void Update()
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && velocity.y < 0)
            velocity.y = 0f;

        // --- 產生移動向量（原本） ---
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        // --- 🆕 若啟動反向，反向處理 ---
        if (isInverted)
        {
            move *= -1f;
        }

        // --- 控制器移動（原本） ---
        controller.Move(move * Time.deltaTime * moveSpeed);

        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- 角色朝向（原本） ---
        if (move.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(move, Vector3.up),
                720f * Time.deltaTime
            );
        }

        // ---------------------------------------------------------
        // Z 軸限制（原本）
        // ---------------------------------------------------------
        if (transform.position.z > maxZPosition)
        {
            Vector3 clampedPosition = transform.position;
            clampedPosition.z = maxZPosition;
            transform.position = clampedPosition;
        }
    }

    // 原本的碰撞（未更動）
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null) return;
        PlatformDisappear platform = hit.collider.GetComponent<PlatformDisappear>();
        if (platform != null) platform.OnStepped();
    }
}
