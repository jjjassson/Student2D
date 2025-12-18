using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // 必須引用 UI

public class PlayerCursorController : MonoBehaviour
{
    public int playerId = 0;
    public RectTransform cursorUI;
    public float moveSpeed = 500f;

    private Image cursorImage; // 儲存 Image 元件參考
    private Sprite defaultSprite; // 初始的游標圖案

    private Vector2 moveInput;
    private Vector2 mouseInput;
    private GameControls controls;

    void Awake()
    {
        controls = new GameControls();
        cursorImage = cursorUI.GetComponent<Image>();
        if (cursorImage != null) defaultSprite = cursorImage.sprite;
    }

    // 新增：供外部呼叫更換圖片的方法
    public void ChangeCursorSprite(Sprite newSprite)
    {
        if (cursorImage == null) return;

        if (newSprite != null)
            cursorImage.sprite = newSprite;
        else
            cursorImage.sprite = defaultSprite; // 如果傳入 null 就換回預設
    }

    // ... 保持原本的 OnEnable, Update, OnDisable 邏輯不變 ...
    void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => moveInput = Vector2.zero;

        if (playerId == 0)
        {
            controls.Player.MousePoint.performed += ctx => mouseInput = ctx.ReadValue<Vector2>();
        }
    }

    void Update()
    {
        if (playerId == 0)
        {
            cursorUI.position = mouseInput;
        }
        else
        {
            Vector3 pos = cursorUI.position;
            pos += (Vector3)(moveInput * moveSpeed * Time.deltaTime);
            pos.x = Mathf.Clamp(pos.x, 0, Screen.width);
            pos.y = Mathf.Clamp(pos.y, 0, Screen.height);
            cursorUI.position = pos;
        }
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }
}