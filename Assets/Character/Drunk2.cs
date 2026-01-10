using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class Drunk2 : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    [HideInInspector] public bool isSlowed = false;
    [HideInInspector] public bool isJumpReduced = false;
    [HideInInspector] public bool isInverted = false;

    private float defaultMoveSpeed;
    private float defaultJumpForce;

    // ===== 🆕 Possess / Cooldown / Control =====
    [Header("附身技能設定")]
    public float possessRange = 3f;
    public float possessDuration = 10f;

    [Header("被附身者閃爍設定")]
    public Color flashColor = Color.cyan;
    public float flashSpeed = 6f;

    [Header("施法者半透明設定")]
    [Range(0.1f, 1f)]
    public float ghostAlpha = 0.3f;

    [Header("技能冷卻時間")]
    public float cooldown = 30f;
    private float cooldownTimer = 0f;

    [Header("附身期間鎖定施法者移動")]
    public bool lockCasterMovement = true;

    private bool isPossessing = false;
    private GameObject possessedTarget;
    private CharacterController targetController;

    private Renderer[] myRenderers;
    private Color[] myOriginalColors;

    private Renderer[] targetRenderers;
    private Color[] targetOriginalColors;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;

        myRenderers = GetComponentsInChildren<Renderer>();
        myOriginalColors = new Color[myRenderers.Length];
        for (int i = 0; i < myRenderers.Length; i++)
            myOriginalColors[i] = myRenderers[i].material.color;
    }

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

    public void OnMove(InputAction.CallbackContext context)
    {
        if (lockCasterMovement && isPossessing) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (lockCasterMovement && isPossessing) return;
        if (context.performed && groundedPlayer)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravityValue);
        }
    }

    private void Update()
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && velocity.y < 0)
            velocity.y = 0f;

        // 自動附身：冷卻完成且未附身時
        if (!isPossessing && cooldownTimer <= 0f)
        {
            TryPossess();
        }

        // 冷卻計時
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // 移動
        Vector3 move = (lockCasterMovement && isPossessing) ? Vector3.zero : new Vector3(moveInput.x, 0, 0);

        if (isInverted)
            move *= -1f;

        controller.Move(move * Time.deltaTime * moveSpeed);

        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Mathf.Abs(move.x) > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                720f * Time.deltaTime
            );
        }

        // 控制被附身者
        if (isPossessing && possessedTarget != null && targetController != null)
        {
            Vector3 targetMove = new Vector3(moveInput.x, 0, 0);
            targetController.Move(targetMove * Time.deltaTime * moveSpeed);
            targetController.Move(Vector3.up * velocity.y * Time.deltaTime);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null) return;

        PlatformDisappear platform = hit.collider.GetComponent<PlatformDisappear>();
        if (platform != null)
            platform.OnStepped();
    }

    void TryPossess()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, possessRange);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (!hit.GetComponent<CharacterController>()) continue;

            possessedTarget = hit.gameObject;
            targetController = possessedTarget.GetComponent<CharacterController>();
            StartCoroutine(PossessCoroutine());
            break;
        }
    }

    IEnumerator PossessCoroutine()
    {
        isPossessing = true;
        cooldownTimer = cooldown;

        SetMyAlpha(ghostAlpha);

        targetRenderers = possessedTarget.GetComponentsInChildren<Renderer>();
        targetOriginalColors = new Color[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
            targetOriginalColors[i] = targetRenderers[i].material.color;

        float timer = 0f;
        while (timer < possessDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Abs(Mathf.Sin(Time.time * flashSpeed));
            for (int i = 0; i < targetRenderers.Length; i++)
                targetRenderers[i].material.color = Color.Lerp(targetOriginalColors[i], flashColor, t);

            yield return null;
        }

        EndPossess();
    }

    void EndPossess()
    {
        for (int i = 0; i < targetRenderers.Length; i++)
            targetRenderers[i].material.color = targetOriginalColors[i];

        SetMyAlpha(1f);
        possessedTarget = null;
        targetController = null;
        isPossessing = false;
    }

    void SetMyAlpha(float alpha)
    {
        for (int i = 0; i < myRenderers.Length; i++)
        {
            Color c = myOriginalColors[i];
            c.a = alpha;
            myRenderers[i].material.color = c;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, possessRange);
    }
}
