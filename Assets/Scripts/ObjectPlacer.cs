using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // 雖然這裡主要用 OnGUI，但保留引用沒關係

public class ObjectPlacer : MonoBehaviour
{
    [Header("放置設定")]
    public Camera mainCamera;
    public float placeDepth = 10f; // 初始深度
    public float scrollSpeed = 2f;
    public float minDepth = 1f;
    public float maxDepth = 50f;

    [Header("Z軸深度限制 (世界座標)")]
    public float worldMinZ = -18f;
    public float worldMaxZ = 0f;

    [Header("UI 顯示設定 (這裡調整字體大小)")]
    [Tooltip("左上角深度資訊的字體大小")]
    public int debugInfoFontSize = 40; // 預設改大到 40

    [Tooltip("螢幕下方狀態文字的字體大小")]
    public int statusTextFontSize = 60; // 預設改大到 60

    [Header("放置物件")]
    public GameObject selectedObjectPrefab;
    public GameObject secondaryObjectPrefab;

    private float lastDepthDisplayTime;
    private const float INITIAL_PLACE_DEPTH = 10f;

    private bool isPlacementAllowed = false;
    private bool hasPlacedThisPhase = true;

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
            Debug.LogError("ObjectPlacer 找不到 RoundManager 實例！請確保 RoundManager 在場景中！");
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        placeDepth = INITIAL_PLACE_DEPTH;
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
        placeDepth = INITIAL_PLACE_DEPTH;
        lastDepthDisplayTime = Time.time;
        Debug.Log($"放置階段開始，深度已重設為 {placeDepth:F2}");
    }

    private void SetPlacementAllowed(bool isAllowed)
    {
        isPlacementAllowed = isAllowed;
        if (!isAllowed)
        {
            hasPlacedThisPhase = true;
        }
    }

    void Update()
    {
        // --- 深度調整 (滾輪) ---
        float scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue != 0)
        {
            placeDepth -= scrollValue * scrollSpeed * Time.deltaTime;
            placeDepth = Mathf.Clamp(placeDepth, minDepth, maxDepth);
            lastDepthDisplayTime = Time.time;
        }

        if (!isPlacementAllowed || hasPlacedThisPhase) return;

        // --- 執行放置 (滑鼠左鍵) ---
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (selectedObjectPrefab == null)
            {
                Debug.LogWarning("尚未選擇任何放置物件！");
                return;
            }

            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, placeDepth)
            );

            float clampedZ = Mathf.Clamp(worldPos.z, worldMinZ, worldMaxZ);
            worldPos.z = clampedZ;

            Instantiate(selectedObjectPrefab, worldPos, Quaternion.identity);

            if (secondaryObjectPrefab != null)
            {
                Vector3 secondaryPos = new Vector3(worldPos.x, worldPos.y, worldMaxZ);
                Instantiate(secondaryObjectPrefab, secondaryPos, Quaternion.identity);
            }

            hasPlacedThisPhase = true;
            placeDepth = INITIAL_PLACE_DEPTH;
            lastDepthDisplayTime = 0f;

            Debug.Log($"物件 {selectedObjectPrefab.name} 放置完成。Z={worldPos.z:F2}");
        }
    }

    // --- 螢幕上顯示目前深度 & 狀態 (UI 修改處) ---
    void OnGUI()
    {
        // 1. 設定左上角資訊文字樣式
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = debugInfoFontSize; // 使用變數控制大小
        style.normal.textColor = Color.yellow;
        style.fontStyle = FontStyle.Bold; // 加粗比較明顯

        // 顯示深度 (僅在調整後顯示 2 秒)
        if (Time.time - lastDepthDisplayTime < 2f)
        {
            // 加大顯示範圍 (Rect)，避免字變大後被切掉
            GUI.Label(new Rect(20, 20, 500, 100), $"目前深度：{placeDepth:F2}", style);

            // 額外顯示 Z 軸限制
            GUIStyle limitStyle = new GUIStyle(style);
            limitStyle.normal.textColor = Color.cyan;
            // Y 軸位置也要往下移，避免跟上面重疊
            GUI.Label(new Rect(20, 20 + debugInfoFontSize + 10, 600, 100), $"世界 Z 軸限制: {worldMinZ:F1} 到 {worldMaxZ:F1}", limitStyle);
        }

        // 2. 設定下方狀態文字樣式
        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.fontSize = statusTextFontSize; // 使用變數控制大小
        statusStyle.alignment = TextAnchor.MiddleCenter; // 設定置中對齊
        statusStyle.fontStyle = FontStyle.Bold; // 加粗

        // 計算螢幕下方的位置 (Rect)
        float labelWidth = 800f; // 寬度加大
        float labelHeight = 150f; // 高度加大
        float xPos = (Screen.width - labelWidth) / 2;
        float yPos = Screen.height - labelHeight - 20; // 距離底部 20px

        Rect statusRect = new Rect(xPos, yPos, labelWidth, labelHeight);

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
                GUI.Label(statusRect, "建造中... (按滑鼠左鍵放置)", statusStyle);
            }
        }
        else if (roundManager != null && roundManager.IsRoundActive)
        {
            statusStyle.normal.textColor = Color.gray;
            GUI.Label(statusRect, "遊玩中/冷卻中...", statusStyle);
        }
    }

    public void SelectObjectFromButton(GameObject mainPrefab, GameObject secondaryPrefab = null)
    {
        if (mainPrefab == null) return;
        selectedObjectPrefab = mainPrefab;
        secondaryObjectPrefab = secondaryPrefab;
        Debug.Log($"選擇主物件：{mainPrefab.name}");
    }

    public void DeselectObject()
    {
        selectedObjectPrefab = null;
        secondaryObjectPrefab = null;
        Debug.Log("取消選擇物件");
    }
}