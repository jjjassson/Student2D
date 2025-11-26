using UnityEngine;
using UnityEngine.InputSystem;

public class InPutHandler : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public GameObject cursorVisual; // 用來顯示自定義游標

    [Header("Cursor Settings")]
    public float cursorSpeed = 1000f;

    private GameControls controls;
    private Vector2 cursorInput;
    private Vector2 mousePosition;
    private bool useMouse = false;

    private void Awake()
    {
        controls = new GameControls();

        // 搖桿控制游標（例如 P1～P4）
        controls.Player.Move.performed += ctx => cursorInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => cursorInput = Vector2.zero;

        // 滑鼠控制點擊與位置（只給測試者用）
        controls.Player.MousePoint.performed += ctx =>
        {
            mousePosition = ctx.ReadValue<Vector2>();
            useMouse = true;
        };

        controls.Player.MouseClick.performed += ctx =>
        {
            Debug.Log("Mouse Clicked!");
            // 可以呼叫選擇函式，例如：
            // SelectObjectAtPosition(mousePosition);
        };

        // 搖桿按鍵（如 A 鍵放置）
        controls.Player.Confirm.performed += ctx =>
        {
            Debug.Log("Confirm (Gamepad A) Pressed!");
        };
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        if (useMouse)
        {
            // 滑鼠控制模式
            cursorVisual.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
        }
        else
        {
            // 搖桿控制模式（加速移動）
            Vector3 moveDelta = new Vector3(cursorInput.x, cursorInput.y, 0f) * cursorSpeed * Time.deltaTime;
            cursorVisual.transform.position += moveDelta;
        }
    }
}
