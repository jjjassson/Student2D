using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPlacer : MonoBehaviour
{
    [Header("放置設定")]
    public Camera mainCamera;

    [Header("高度設定")]
    [Tooltip("在此設定物件放置的固定高度 (例如 0 或 1)")]
    public float placementHeight = 0f; // 這裡控制 Y 軸高度

    [Header("邊界限制 (X/Z軸)")]
    public bool enableBoundaries = false;
    public Vector2 xRange = new Vector2(-20, 20);
    public Vector2 zRange = new Vector2(-20, 20);

    [Header("UI 顯示設定")]
    public int debugInfoFontSize = 40;
    public int statusTextFontSize = 60;

    [Header("放置物件")]
    public GameObject selectedObjectPrefab;
    public GameObject secondaryObjectPrefab;

    private bool isPlacementAllowed = false;
    private bool hasPlacedThisPhase = true;
    private Vector3 currentHoverPos;

    private RoundManager roundManager;

    void Start()
    {
        roundManager = RoundManager.Instance;
        if (roundManager != null)
        {
            roundManager.OnPlacementStart += SetPlacementCooldown;
            roundManager.OnPlacementAllowedChange += SetPlacementAllowed;
        }
        else
        {
            Debug.LogError("ObjectPlacer 找不到 RoundManager 實例！");
        }

        if (mainCamera == null) mainCamera = Camera.main;
    }

    void OnDestroy()
    {
        if (roundManager != null)
        {
            roundManager.OnPlacementStart -= SetPlacementCooldown;
            roundManager.OnPlacementAllowedChange -= SetPlacementAllowed;
        }
    }

    private void SetPlacementCooldown(float placementTime)
    {
        hasPlacedThisPhase = false;
        Debug.Log($"放置階段開始，本次固定高度為 Y={placementHeight}");
    }

    private void SetPlacementAllowed(bool isAllowed)
    {
        isPlacementAllowed = isAllowed;
        if (!isAllowed) hasPlacedThisPhase = true;
    }

    void Update()
    {
        if (!isPlacementAllowed || hasPlacedThisPhase) return;

        // 1. 建立射線與地板平面 (用來抓 X 和 Z)
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        // 這裡平面設為 0 是為了準確抓取滑鼠在地板上的投影位置
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        // 2. 檢測滑鼠位置
        if (groundPlane.Raycast(ray, out float enter))
        {
            // 取得射線在地板上的點
            Vector3 hitPoint = ray.GetPoint(enter);

            // 【關鍵修改】強制將 Y 軸替換為你設定的高度
            hitPoint.y = placementHeight;

            // 邊界限制
            if (enableBoundaries)
            {
                hitPoint.x = Mathf.Clamp(hitPoint.x, xRange.x, xRange.y);
                hitPoint.z = Mathf.Clamp(hitPoint.z, zRange.x, zRange.y);
            }

            // 更新 UI 顯示用座標
            currentHoverPos = hitPoint;

            // 3. 執行放置
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (selectedObjectPrefab == null)
                {
                    Debug.LogWarning("尚未選擇任何放置物件！");
                    return;
                }

                Instantiate(selectedObjectPrefab, hitPoint, Quaternion.identity);

                if (secondaryObjectPrefab != null)
                {
                    // 若有副物件，位置也相同 (或依你的需求調整)
                    Instantiate(secondaryObjectPrefab, hitPoint, Quaternion.identity);
                }

                hasPlacedThisPhase = true;
                Debug.Log($"物件放置完成：{hitPoint}");
            }
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = debugInfoFontSize;
        style.normal.textColor = Color.yellow;
        style.fontStyle = FontStyle.Bold;

        if (isPlacementAllowed && !hasPlacedThisPhase)
        {
            GUI.Label(new Rect(20, 20, 600, 100), $"預覽位置: {currentHoverPos}", style);

            // 顯示當前設定的高度
            GUIStyle hintStyle = new GUIStyle(style);
            hintStyle.fontSize = (int)(debugInfoFontSize * 0.8f);
            hintStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(20, 20 + debugInfoFontSize + 10, 600, 100), $"目前固定高度 Y = {placementHeight}", hintStyle);
        }

        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.fontSize = statusTextFontSize;
        statusStyle.alignment = TextAnchor.MiddleCenter;
        statusStyle.fontStyle = FontStyle.Bold;

        float labelWidth = 800f;
        float labelHeight = 150f;
        Rect statusRect = new Rect((Screen.width - labelWidth) / 2, Screen.height - labelHeight - 20, labelWidth, labelHeight);

        if (isPlacementAllowed)
        {
            if (hasPlacedThisPhase)
            {
                statusStyle.normal.textColor = Color.red;
                GUI.Label(statusRect, "已放置。等待下一輪...", statusStyle);
            }
            else
            {
                statusStyle.normal.textColor = Color.green;
                GUI.Label(statusRect, "點擊放置物件", statusStyle);
            }
        }
        else if (roundManager != null && roundManager.IsRoundActive)
        {
            statusStyle.normal.textColor = Color.gray;
            GUI.Label(statusRect, "遊玩中...", statusStyle);
        }
    }

    public void SelectObjectFromButton(GameObject mainPrefab, GameObject secondaryPrefab = null)
    {
        if (mainPrefab == null) return;
        selectedObjectPrefab = mainPrefab;
        secondaryObjectPrefab = secondaryPrefab;
    }

    public void DeselectObject()
    {
        selectedObjectPrefab = null;
        secondaryObjectPrefab = null;
    }
}