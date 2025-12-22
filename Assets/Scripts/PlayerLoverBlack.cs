using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerLoverBlack : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    [Header("吸引能力設定")]
    public float attractRadius = 6f;
    public float attractForce = 6f;
    public float minAttractForce = 1.5f;
    public LayerMask playerLayer;
    public bool attractEnabled = true;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    // 🧩 狀態
    [HideInInspector] public bool isSlowed = false;
    [HideInInspector] public bool isJumpReduced = false;
    [HideInInspector] public bool isInverted = false;

    // 🧩 預設參數
    private float defaultMoveSpeed;
    private float defaultJumpForce;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
    }

    // =========================================================
    // Zone / 物件影響（與 Player2 / Player1 對齊）
    // =========================================================
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

    public void InvertMovement()
    {
        isInverted = true;
    }

    public void ResetInverted()
    {
        isInverted = false;
    }

    // =========================================================
    // Input
    // =========================================================
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

    // =========================================================
    // Update
    // =========================================================
    private void Update()
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && velocity.y < 0)
            velocity.y = 0f;

        // -------------------------------------------------
        // 🧭 僅允許 X 軸移動（Player2 核心規則）
        // -------------------------------------------------
        Vector3 move = new Vector3(moveInput.x, 0, 0);

        if (isInverted)
            move.x *= -1f;

        controller.Move(move * Time.deltaTime * moveSpeed);

        // 重力
        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 面向左右
        if (Mathf.Abs(move.x) > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                720f * Time.deltaTime
            );
        }

        // 🧲 吸引能力
        if (attractEnabled)
            AttractNearbyPlayers();
    }

    // =========================================================
    // 🧲 吸引邏輯（X 軸平面版本）
    // =========================================================
    private void AttractNearbyPlayers()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            attractRadius,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            CharacterController otherController =
                hit.GetComponent<CharacterController>();

            if (otherController == null)
                continue;

            Vector3 direction =
                transform.position - hit.transform.position;

            // ❗ 只拉 X 軸
            direction.y = 0f;
            direction.z = 0f;

            float distance = Mathf.Abs(direction.x);
            if (distance < 0.05f)
                continue;

            float t = 1f - Mathf.Clamp01(distance / attractRadius);
            float force = Mathf.Lerp(minAttractForce, attractForce, t);

            Vector3 pull =
                direction.normalized * force * Time.deltaTime;

            otherController.Move(pull);
        }
    }

    // =========================================================
    // PlatformDisappear 支援（與 Player1 同步）
    // =========================================================
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null) return;

        PlatformDisappear platform =
            hit.collider.GetComponent<PlatformDisappear>();

        if (platform != null)
            platform.OnStepped();
    }

    // =========================================================
    // Gizmo 顯示吸引範圍
    // =========================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attractRadius);
    }
}
