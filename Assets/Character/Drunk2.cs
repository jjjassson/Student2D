using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Drunk2 : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    // 可以把目標 Z 軸位置開放出來，未來如果有需要切換圖層會比較方便
    public float fixedZPosition = 0f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    private bool inputLocked;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (inputLocked) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (inputLocked) return;

        if (context.performed && groundedPlayer)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravityValue);
    }

    private void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && velocity.y < 0) velocity.y = 0f;

        // X 軸移動
        Vector3 move = new Vector3(moveInput.x, 0, 0);
        controller.Move(move * Time.deltaTime * moveSpeed);

        // Y 軸重力與跳躍移動
        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ⭐⭐⭐ 關鍵：鎖死朝向攝影機 ⭐⭐⭐
        LockToCamera();

        // ⭐⭐⭐ 新增：鎖死 Z 軸，防止因為碰撞偏移 ⭐⭐⭐
        LockZPosition();
    }

    // ===== 鎖定 Z 軸 =====
    void LockZPosition()
    {
        Vector3 currentPos = transform.position;

        // 加上一點浮點數容差 (0.001f)，避免系統每幀都在做無意義的微小覆寫造成效能浪費或抖動
        if (Mathf.Abs(currentPos.z - fixedZPosition) > 0.001f)
        {
            currentPos.z = fixedZPosition;
            transform.position = currentPos;

            // 可選：如果你發現強制設定 position 後畫面會偶爾卡頓或碰撞異常，
            // 可以把下面這行取消註解，這會強制物理引擎立刻同步變更。
            // Physics.SyncTransforms(); 
        }
    }

    // ⭐⭐⭐ 核心（不再用 LookRotation）⭐⭐⭐
    void LockToCamera()
    {
        if (Camera.main == null) return;

        Vector3 camEuler = Camera.main.transform.eulerAngles;

        // 只跟著 Y 軸（水平）
        transform.rotation = Quaternion.Euler(0f, camEuler.y, 0f);
    }

    // ===== 被附身時接收 Input =====
    public void ReceivePossessMove(Vector2 input)
    {
        inputLocked = true;
        moveInput = input;
    }

    public void ReceivePossessJump()
    {
        if (groundedPlayer)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravityValue);
    }

    public void EndPossession()
    {
        inputLocked = false;
    }
}