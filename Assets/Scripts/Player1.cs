using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player1 : MonoBehaviour, IMover
{
    [SerializeField] private float playerSpeed = 2f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float gravityValue = -9.81f;

    private CharacterController controller;
    private Vector2 movementInput = Vector2.zero;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    // ⬇️ 新增：速度倍率
    private float speedMultiplier = 1f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // ✅ 提供 SlowZone 呼叫的接口
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    // ✅ IMover 實作
    public void SetInputVector(Vector2 direction)
    {
        movementInput = direction;
    }

    // ✅ PlayerInput 直接呼叫 Jump
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        }
    }

    // 如果你想直接綁 Movement Action 也可以保留 OnMove
    public void OnMove(InputAction.CallbackContext context)
    {
        SetInputVector(context.ReadValue<Vector2>());
    }

    private void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0f)
            playerVelocity.y = 0f;

        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);

        if (move != Vector3.zero)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(move, Vector3.up), 720f * Time.deltaTime);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;

        // ⬇️ 這裡套用 speedMultiplier
        Vector3 finalMove = move * playerSpeed * speedMultiplier + Vector3.up * playerVelocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }
}
