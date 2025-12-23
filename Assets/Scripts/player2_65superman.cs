using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player2_65Superman : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

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

    // ===== 區域影響（沿用 Player2）=====
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

        // 👉 只能 X 軸左右移動（完全保留 Player2 特性）
        float finalSpeed = moveSpeed;
        if (isSpeedBoostActive)
            finalSpeed *= speedBoostMultiplier;

        Vector3 move = new Vector3(moveInput.x, 0, 0);
        controller.Move(move * finalSpeed * Time.deltaTime);

        // 重力
        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 面向左右方向
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
}
