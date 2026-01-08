using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Love2 : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    [Header("戀愛腦能力（吸引）")]
    [SerializeField] private bool enableLoveForce = true;
    [SerializeField] private float attractRadius = 6f;
    [SerializeField] private float attractForce = 8f;
    [SerializeField] private LayerMask targetLayer;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    [HideInInspector] public bool reverseControl = false;

    private float defaultMoveSpeed;
    private float defaultJumpForce;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
    }

    public void SetReverse(bool active) { reverseControl = active; }

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

        Vector3 move = new Vector3(moveInput.x, 0, 0);
        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractRadius);
    }
#endif
}
