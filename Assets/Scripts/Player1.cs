using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player1 : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    [Header("位置限制")] // 🆕 新增標題讓介面更好看
    [Tooltip("玩家 Z 軸能到達的最大值 (不能超過這條線)")]
    public float maxZPosition = -3f; // 🆕 這就是那個可以調整的變數

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

    // ... (中間的 ApplySpeedMultiplier, ResetSpeed, OnMove, OnJump 等等保持不變) ...
    // 為了節省篇幅，這裡省略中間沒變的函數，請保留你原本的程式碼
    public void ApplySpeedMultiplier(float multiplier) { moveSpeed = defaultMoveSpeed * multiplier; isSlowed = multiplier < 1f; }
    public void ApplyJumpMultiplier(float multiplier) { jumpForce = defaultJumpForce * multiplier; isJumpReduced = multiplier < 1f; }
    public void ResetSpeed() { moveSpeed = defaultMoveSpeed; isSlowed = false; }
    public void ResetJump() { jumpForce = defaultJumpForce; isJumpReduced = false; }
    public void OnMove(InputAction.CallbackContext context) { moveInput = context.ReadValue<Vector2>(); }
    public void OnJump(InputAction.CallbackContext context) { if (context.performed && groundedPlayer) { velocity.y = Mathf.Sqrt(jumpForce * -2f * gravityValue); } }

    // ===== 更新移動 =====
    private void Update()
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && velocity.y < 0)
            velocity.y = 0f;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(move * Time.deltaTime * moveSpeed);

        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 轉向
        if (move.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(move, Vector3.up),
                720f * Time.deltaTime
            );
        }

        // ---------------------------------------------------------
        // 🆕 修改後：使用變數 maxZPosition 來限制
        // ---------------------------------------------------------
        if (transform.position.z > maxZPosition)
        {
            // 直接修改 transform.position 將 Z 軸拉回設定的邊界
            Vector3 clampedPosition = transform.position;
            clampedPosition.z = maxZPosition;
            transform.position = clampedPosition;
        }
    }

    // ... (原本的 OnControllerColliderHit 保持不變) ...
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null) return;
        PlatformDisappear platform = hit.collider.GetComponent<PlatformDisappear>();
        if (platform != null) platform.OnStepped();
    }
}