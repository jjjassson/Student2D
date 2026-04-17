using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Lung2 : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    // ⭐⭐⭐ 新增：可以設定要鎖死的 Z 軸位置
    [Header("鎖定位置")]
    public float fixedZPosition = 0f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    // 🧩 狀態
    [HideInInspector] public bool isSlowed = false;
    [HideInInspector] public bool isJumpReduced = false;

    // 🧩 🆕 全方向反轉狀態（與 Player1 一致）
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
    // SlowZone / LowJumpZone
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

    // ============================================================
    // 操作反轉
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
    // 玩家輸入
    // ============================================================
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
    // 更新
    // ============================================================
    private void Update()
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && velocity.y < 0)
            velocity.y = 0f;

        Vector3 move = new Vector3(moveInput.x, 0, 0);

        if (isInverted)
            move *= -1f;

        controller.Move(move * Time.deltaTime * moveSpeed);

        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

<<<<<<< Updated upstream
        // ==========================================
        // 🆕 核心修改：固定面向 Z 軸負方向 (面向玩家)
        // ==========================================
        transform.rotation = Quaternion.Euler(0, 180f, 0);

        // ⭐⭐⭐ 新增：鎖死 Z 軸，防止因為物理碰撞偏移 ⭐⭐⭐
        LockZPosition();
    }

    // ⭐⭐⭐ 新增：鎖定 Z 軸的方法 ⭐⭐⭐
    void LockZPosition()
    {
        Vector3 currentPos = transform.position;

        // 加上浮點數容差，避免無意義的微小覆寫
        if (Mathf.Abs(currentPos.z - fixedZPosition) > 0.001f)
        {
            currentPos.z = fixedZPosition;
            transform.position = currentPos;
        }
=======
        // ⭐⭐⭐ 改這裡：永遠面向攝影機 ⭐⭐⭐
        LockToCamera();
    }

    // ⭐⭐⭐ 新增 ⭐⭐⭐
    void LockToCamera()
    {
        if (Camera.main == null) return;

        Vector3 camEuler = Camera.main.transform.eulerAngles;

        // 只跟著 Y 軸（水平）
        transform.rotation = Quaternion.Euler(0f, camEuler.y, 0f);
>>>>>>> Stashed changes
    }

    // ============================================================
    // PlatformDisappear
    // ============================================================
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null) return;

        PlatformDisappear platform = hit.collider.GetComponent<PlatformDisappear>();
        if (platform != null)
            platform.OnStepped();
    }
}