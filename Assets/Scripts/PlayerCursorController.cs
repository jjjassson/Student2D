using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCursorController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerId = 0;


[Header("UI Cursor")]
    public RectTransform cursorUI;
    public float moveSpeed = 500f;

    [Header("Preview Settings")]
    public float fallbackDistance = 10f;

    private Transform previewObject;
    private Vector2 moveInput;
    private Vector2 mouseInput;
    private GameControls controls;
    private Camera cam;

    void Awake()
    {
        controls = new GameControls();
    }

    void Start()
    {
        cam = Camera.main;
    }

    void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => moveInput = Vector2.zero;

        if (playerId == 0)
            controls.Player.MousePoint.performed += ctx => mouseInput = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        UpdateCursorUI();
        UpdatePreviewPosition();
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    // --------------------------------------------------
    // UI 游標追蹤
    // --------------------------------------------------
    void UpdateCursorUI()
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

    // --------------------------------------------------
    // 玩家選擇物件 → 生成預覽物件
    // --------------------------------------------------
    public void StartPreview(Transform prefab)
    {
        ClearPreview();

        previewObject = Instantiate(prefab);
        previewObject.name = prefab.name + "_Preview";

        foreach (var col in previewObject.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    // --------------------------------------------------
    // 清除預覽物件
    // --------------------------------------------------
    public void ClearPreview()
    {
        if (previewObject != null)
            Destroy(previewObject.gameObject);

        previewObject = null;
    }

    // --------------------------------------------------
    // 射線 → 更新預覽位置（不貼地）
    // --------------------------------------------------
    void UpdatePreviewPosition()
    {
        if (previewObject == null || cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(cursorUI.position);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            previewObject.position = hit.point;
        }
        else
        {
            previewObject.position = ray.GetPoint(fallbackDistance);
        }
    }


}
