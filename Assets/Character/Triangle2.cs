using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Triangle2 : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    [Header("Triangle 能力設定")]
    [SerializeField] private float skillCooldown = 6.5f;
    [SerializeField] private float boostDuration = 1.5f;
    [SerializeField] private float speedMultiplier = 1.5f;

    [Header("加速視覺效果")]
    [SerializeField] private Color boostColor = Color.blue;
    [SerializeField] private float blinkSpeed = 8f;

    private CharacterController controller;
    private Renderer[] renderers;
    private Color[] originalColors;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    private float defaultMoveSpeed;
    private float timer;
    private bool boosting;

    [HideInInspector] public bool isSlowed = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        renderers = GetComponentsInChildren<Renderer>();

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material.color;

        defaultMoveSpeed = moveSpeed;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && groundedPlayer)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravityValue);
    }

    private void Update()
    {
        HandleSkillTimer();

        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && velocity.y < 0)
            velocity.y = 0f;

        Vector3 move = new Vector3(moveInput.x, 0, 0);
        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (boosting)
            BlinkColor();
    }

    private void HandleSkillTimer()
    {
        timer += Time.deltaTime;

        if (!boosting && timer >= skillCooldown)
        {
            boosting = true;
            timer = 0f;

            if (!isSlowed)
                moveSpeed = defaultMoveSpeed * speedMultiplier;
        }
        else if (boosting && timer >= boostDuration)
        {
            boosting = false;
            timer = 0f;
            moveSpeed = defaultMoveSpeed;
            ResetColor();
        }
    }

    private void BlinkColor()
    {
        float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        Color c = Color.Lerp(originalColors[0], boostColor, t);

        foreach (Renderer r in renderers)
            r.material.color = c;
    }

    private void ResetColor()
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.color = originalColors[i];
    }

    // ===== SlowZone 介面 =====
    public void ApplySpeedMultiplier(float multiplier)
    {
        isSlowed = multiplier < 1f;
        moveSpeed = defaultMoveSpeed * multiplier;
    }

    public void ResetSpeed()
    {
        isSlowed = false;
        moveSpeed = defaultMoveSpeed;
    }
}
