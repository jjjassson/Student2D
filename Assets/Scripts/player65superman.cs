using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player65Superman : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    [Header("位置限制")]
    public float maxZPosition = -3f;

    [Header("🦸 Superman 能力（自動加速）")]
    public float speedBoostMultiplier = 1.5f;
    public float boostDuration = 6.5f;
    public float boostInterval = 15f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    // 🧩 狀態
    [HideInInspector] public bool isSlowed = false;
    [HideInInspector] public bool isJumpReduced = false;

    // 🦸 能力狀態
    private bool isSpeedBoostActive = false;
    private float boostTimer = 0f;
    private float intervalTimer = 0f;

    // 🧩 預設參數
    private float defaultMoveSpeed;
    private float defaultJumpForce;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
    }

    // ===== 區域效果（完全沿用 Player1）=====
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

    // ===== Input =====
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

    // ===== Update =====
    private void Update()
    {
        HandleSpeedBoost();

        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && velocity.y < 0)
            velocity.y = 0f;

        // 👉 最終速度（SlowZone × Superman）
        float finalSpeed = moveSpeed;
        if (isSpeedBoostActive)
            finalSpeed *= speedBoostMultiplier;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(move * finalSpeed * Time.deltaTime);

        // 重力
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

        // Z 軸限制
        if (transform.position.z > maxZPosition)
        {
            Vector3 pos = transform.position;
            pos.z = maxZPosition;
            transform.position = pos;
        }
    }

    // ===== Superman 能力邏輯 =====
    private void HandleSpeedBoost()
    {
        if (!isSpeedBoostActive)
        {
            intervalTimer += Time.deltaTime;
            if (intervalTimer >= boostInterval)
            {
                isSpeedBoostActive = true;
                boostTimer = 0f;
                intervalTimer = 0f;
            }
        }
        else
        {
            boostTimer += Time.deltaTime;
            if (boostTimer >= boostDuration)
            {
                isSpeedBoostActive = false;
            }
        }
    }

    // ===== PlatformDisappear 支援（保留 Player1 行為）=====
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null) return;

        PlatformDisappear platform = hit.collider.GetComponent<PlatformDisappear>();
        if (platform != null)
            platform.OnStepped();
    }
}
