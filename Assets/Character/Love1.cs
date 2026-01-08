using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Love1 : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    [Header("位置限制")]
    [Tooltip("玩家 Z 軸能到達的最大值")]
    public float maxZPosition = -3f;

    // =========================
    // ❤️ 戀愛腦吸引能力設定
    // =========================
    [Header("戀愛腦能力（吸引）")]
    [Tooltip("是否啟用吸引能力")]
    [SerializeField] private bool enableLoveForce = true;

    [Tooltip("吸引半徑")]
    [SerializeField] private float attractRadius = 6f;

    [Tooltip("吸引強度（越大拉越快）")]
    [SerializeField] private float attractForce = 8f;

    [Tooltip("影響圖層（建議 Player）")]
    [SerializeField] private LayerMask targetLayer;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    // 狀態
    [HideInInspector] public bool isSlowed = false;
    [HideInInspector] public bool isJumpReduced = false;
    [HideInInspector] public bool reverseControl = false;

    // 預設參數
    private float defaultMoveSpeed;
    private float defaultJumpForce;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
    }

    // ===== 狀態接口 =====
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

    public void ResetSpeed() { moveSpeed = defaultMoveSpeed; }
    public void ResetJump() { jumpForce = defaultJumpForce; }
    public void SetReverse(bool active) { reverseControl = active; }

    // ===== Input =====
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (reverseControl) moveInput = -moveInput;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && groundedPlayer)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravityValue);
    }

    private void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && velocity.y < 0) velocity.y = 0f;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (move.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(move, Vector3.up),
                720f * Time.deltaTime);
        }

        if (transform.position.z > maxZPosition)
        {
            Vector3 pos = transform.position;
            pos.z = maxZPosition;
            transform.position = pos;
        }

        // ❤️ 吸引能力
        if (enableLoveForce)
            ApplyLoveForce();
    }

    private void ApplyLoveForce()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, attractRadius, targetLayer);

        foreach (Collider col in targets)
        {
            if (col.gameObject == gameObject) continue;

            CharacterController target = col.GetComponent<CharacterController>();
            if (target == null) continue;

            Vector3 dir = (transform.position - target.transform.position).normalized;
            target.Move(dir * attractForce * Time.deltaTime);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PlatformDisappear platform = hit.collider.GetComponent<PlatformDisappear>();
        if (platform != null) platform.OnStepped();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attractRadius);
    }
#endif
}
