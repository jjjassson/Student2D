using UnityEngine;
using UnityEngine.InputSystem;

public class InPutHandler : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerId = 0;               // 玩家ID，0 = 滑鼠

    [Header("Cursor Settings")]
    public float moveSpeed = 500f;         // 搖桿游標速度
    public float cursorDepth = 10f;        // 預覽物件距離相機的深度
    public Transform cursorVisual;         // 游標可視化物件

    [Header("Camera")]
    public Camera cam;

    private Vector2 moveInput;
    private Vector2 mouseInput;
    private bool useMouse = true;

    private Transform previewObject;

    private GameControls controls;

    void Awake()
    {
        controls = new GameControls();
    }

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        if (cursorVisual == null)
        {
            // 沒有游標模型就生成一個小球
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.localScale = Vector3.one * 0.3f;
            Destroy(go.GetComponent<Collider>());
            cursorVisual = go.transform;
        }
    }

    void OnEnable()
    {
        controls.Player.Enable();

        // 搖桿移動
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // 滑鼠位置
        controls.Player.MousePoint.performed += ctx =>
        {
            mouseInput = ctx.ReadValue<Vector2>();
            useMouse = true;
        };
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    void Update()
    {
        UpdateCursorPosition();
        UpdatePreviewPosition();
    }

    // -------------------------------
    // 更新游標/預覽物件位置
    // -------------------------------
    private void UpdateCursorPosition()
    {
        if (cursorVisual == null || cam == null) return;

        Vector3 newPos;

        if (useMouse && playerId == 0)
        {
            // 滑鼠模式
            newPos = cam.ScreenToWorldPoint(new Vector3(mouseInput.x, mouseInput.y, cursorDepth));
        }
        else
        {
            // 搖桿模式
            Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0f) * moveSpeed * Time.deltaTime;
            newPos = cursorVisual.position + delta;
        }

        cursorVisual.position = newPos;
    }

    // -------------------------------
    // 更新預覽物件位置
    // -------------------------------
    private void UpdatePreviewPosition()
    {
        if (previewObject == null || cursorVisual == null) return;

        previewObject.position = cursorVisual.position;
    }

    // -------------------------------
    // 按鈕選擇物件時呼叫 → 生成/更新預覽物件
    // -------------------------------
    public void StartPreview(Transform prefab)
    {
        ClearPreview();

        if (prefab == null) return;

        previewObject = Instantiate(prefab);
        previewObject.name = prefab.name + "_Preview";

        foreach (var col in previewObject.GetComponentsInChildren<Collider>())
            col.enabled = false;

        foreach (var rb in previewObject.GetComponentsInChildren<Rigidbody>())
            Destroy(rb);
    }

    // -------------------------------
    // 清除預覽物件
    // -------------------------------
    public void ClearPreview()
    {
        if (previewObject != null)
            Destroy(previewObject.gameObject);

        previewObject = null;
    }
}
