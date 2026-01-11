using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class Drunk1 : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    [Header("位置限制")]
    public float maxZPosition = -3f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    [HideInInspector] public bool isSlowed;
    [HideInInspector] public bool isJumpReduced;
    [HideInInspector] public bool reverseControl;

    private float defaultMoveSpeed;
    private float defaultJumpForce;

    // ================= 附身技能 =================
    [Header("附身技能設定")]
    public float possessRange = 3f;
    public float possessDuration = 10f;
    public float cooldown = 30f;

    [Header("被附身者閃爍")]
    public Color flashColor = Color.cyan;
    public float flashSpeed = 6f;

    [Header("施法者半透明")]
    [Range(0.1f, 1f)]
    public float ghostAlpha = 0.3f;

    private float cooldownTimer;
    private bool isPossessing;

    private Drunk1 possessedDrunk1;
    private Drunk2 possessedDrunk2;

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

    // ================= Input =================
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (reverseControl) moveInput = -moveInput;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && groundedPlayer)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravityValue);

            if (isPossessing)
                ForwardJump();
        }
    }

    // ================= Update =================
    private void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && velocity.y < 0) velocity.y = 0f;

        if (!isPossessing && cooldownTimer <= 0f)
            TryPossess();

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(move * Time.deltaTime * moveSpeed);

        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (move.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(move, Vector3.up),
                720f * Time.deltaTime
            );
        }

        if (transform.position.z > maxZPosition)
        {
            Vector3 p = transform.position;
            p.z = maxZPosition;
            transform.position = p;
        }

        if (isPossessing)
            ForwardMove(moveInput);
    }

    // ================= 附身邏輯 =================
    void TryPossess()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, possessRange);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            possessedDrunk1 = hit.GetComponent<Drunk1>();
            possessedDrunk2 = hit.GetComponent<Drunk2>();

            if (possessedDrunk1 || possessedDrunk2)
            {
                StartCoroutine(PossessCoroutine(hit.gameObject));
                break;
            }
        }
    }

    IEnumerator PossessCoroutine(GameObject target)
    {
        isPossessing = true;
        cooldownTimer = cooldown;

        SetMyAlpha(ghostAlpha);

        targetRenderers = target.GetComponentsInChildren<Renderer>();
        targetOriginalColors = new Color[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
            targetOriginalColors[i] = targetRenderers[i].material.color;

        float t = 0f;
        while (t < possessDuration)
        {
            t += Time.deltaTime;
            float f = Mathf.Abs(Mathf.Sin(Time.time * flashSpeed));
            for (int i = 0; i < targetRenderers.Length; i++)
                targetRenderers[i].material.color =
                    Color.Lerp(targetOriginalColors[i], flashColor, f);
            yield return null;
        }

        EndPossess();
    }

    void EndPossess()
    {
        SetMyAlpha(1f);
        isPossessing = false;
        possessedDrunk1 = null;
        possessedDrunk2 = null;
    }

    // ================= Input 轉交 =================
    void ForwardMove(Vector2 input)
    {
        if (possessedDrunk1)
            possessedDrunk1.ReceivePossessMove(input);
        if (possessedDrunk2)
            possessedDrunk2.ReceivePossessMove(input);
    }

    void ForwardJump()
    {
        if (possessedDrunk1)
            possessedDrunk1.ReceivePossessJump();
        if (possessedDrunk2)
            possessedDrunk2.ReceivePossessJump();
    }

    public void ReceivePossessMove(Vector2 input) { }
    public void ReceivePossessJump() { }

    void SetMyAlpha(float a)
    {
        for (int i = 0; i < myRenderers.Length; i++)
        {
            Color c = myOriginalColors[i];
            c.a = a;
            myRenderers[i].material.color = c;
        }
    }
}
