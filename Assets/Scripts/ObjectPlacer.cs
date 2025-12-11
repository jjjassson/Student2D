using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ObjectPlacer : MonoBehaviour
{
    [Header("放置設定")]
    public Camera mainCamera;
    public float placeDepth = 10f; // 初始深度
    public float scrollSpeed = 2f;
    public float minDepth = 1f;
    public float maxDepth = 50f;

    [Header("Z軸深度限制 (世界座標)")]
    // 新增：世界座標 Z 軸的最小/最大限制
    public float worldMinZ = -18f;
    public float worldMaxZ = 0f;

    [Header("放置物件")]
    public GameObject selectedObjectPrefab;
    public GameObject secondaryObjectPrefab;

    private float lastDepthDisplayTime;
    private const float INITIAL_PLACE_DEPTH = 10f; // 用於放置後的深度歸零

    // 新增：放置限制控制
    private bool isPlacementAllowed = false; // 是否在放置時間內 (由 RoundManager 控制)
    private bool hasPlacedThisPhase = true; // 本輪放置階段是否已放置

    private RoundManager roundManager;

    void Start()
    {
        // ... (保持不變)

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

        // 初始化深度
        placeDepth = INITIAL_PLACE_DEPTH;
    }

    void OnDestroy()
    {
        // ... (保持不變)
    }

    // 從 RoundManager 接收事件，重置單次放置限制
    private void SetPlacementCooldown(float placementTime)
    {
        hasPlacedThisPhase = false; // 新一輪放置開始時，重置放置標記 (允許放置)
        // 新增：每次放置階段開始時，將深度重設為初始值
        placeDepth = INITIAL_PLACE_DEPTH;
        lastDepthDisplayTime = Time.time; // 顯示深度
        Debug.Log($"放置階段開始，深度已重設為 {placeDepth:F2}");
    }

    // 設置是否允許放置
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
            // 使用 Time.deltaTime 使滾輪速度與幀率無關
            placeDepth -= scrollValue * scrollSpeed * Time.deltaTime;
            placeDepth = Mathf.Clamp(placeDepth, minDepth, maxDepth);
            lastDepthDisplayTime = Time.time; // 更新顯示時間
        }

        // 檢查是否處於允許放置階段 且 尚未在本階段放置 (單次放置限制)
        if (!isPlacementAllowed || hasPlacedThisPhase) return;

        // --- 執行放置 (滑鼠左鍵) ---
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (selectedObjectPrefab == null)
            {
                Debug.LogWarning("尚未選擇任何放置物件！");
                return;
            }

            // 1. 座標計算 (從螢幕座標轉換到世界座標)
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, placeDepth)
            );

            // 2. 🎯 Z 軸限制邏輯 (應用您的要求：限制在 worldMinZ 到 worldMaxZ 之間)
            float clampedZ = Mathf.Clamp(worldPos.z, worldMinZ, worldMaxZ);
            worldPos.z = clampedZ;

            // 3. 執行放置
            GameObject mainObj = Instantiate(selectedObjectPrefab, worldPos, Quaternion.identity);

            // 4. 放置副物件 (Z=0, 但我們應該使用 worldMaxZ=0 來確保一致性)
            if (secondaryObjectPrefab != null)
            {
                Vector3 secondaryPos = new Vector3(worldPos.x, worldPos.y, worldMaxZ); // 使用 worldMaxZ (通常為 0)
                Instantiate(secondaryObjectPrefab, secondaryPos, Quaternion.identity);
            }

            // 5. ✅ 放置完成後，設定本輪已放置
            hasPlacedThisPhase = true;

            // 6. ❌ 放置完成後，將深度重設為初始值 (等待下一輪開始時才允許使用滾輪調整)
            placeDepth = INITIAL_PLACE_DEPTH;
            lastDepthDisplayTime = 0f; // 停止顯示深度，直到下一輪開始調整

            Debug.Log($"物件 {selectedObjectPrefab.name} 放置完成。世界座標 Z={worldPos.z:F2} (已被限制在 {worldMinZ} 到 {worldMaxZ} 之間)。本輪放置結束，需等待下一輪。");
        }
    }

    // --- 螢幕上顯示目前深度 & 狀態 ---
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 20;
        style.normal.textColor = Color.yellow;

        // 顯示深度 (僅在調整後顯示 2 秒)
        if (Time.time - lastDepthDisplayTime < 2f)
        {
            GUI.Label(new Rect(10, 10, 300, 40), $"目前深度：{placeDepth:F2}", style);

            // 額外顯示 Z 軸限制
            GUIStyle limitStyle = new GUIStyle(style);
            limitStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(10, 40, 400, 40), $"世界 Z 軸限制: {worldMinZ:F1} 到 {worldMaxZ:F1}", limitStyle);
        }

        // 顯示放置狀態
        // ... (保持不變)
        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.fontSize = 25;

        if (isPlacementAllowed)
        {
            if (hasPlacedThisPhase)
            {
                statusStyle.normal.textColor = Color.red;
                GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 50, 300, 40), "已放置。等待下一輪...", statusStyle);
            }
            else
            {
                statusStyle.normal.textColor = Color.green;
                GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 50, 300, 40), "建造中... (按滑鼠左鍵放置)", statusStyle);
            }
        }
        else if (roundManager != null && roundManager.IsRoundActive)
        {
            statusStyle.normal.textColor = Color.gray;
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 50, 300, 40), "遊玩中/冷卻中...", statusStyle);
        }
    }

    // ... (SelectObjectFromButton 和 DeselectObject 保持不變)
    public void SelectObjectFromButton(GameObject mainPrefab, GameObject secondaryPrefab = null)
    {
        if (mainPrefab == null) return;

        selectedObjectPrefab = mainPrefab;
        secondaryObjectPrefab = secondaryPrefab;

        Debug.Log($"選擇主物件：{mainPrefab.name}" +
                    (secondaryPrefab != null ? $", 副物件：{secondaryPrefab.name}" : ", 無副物件"));
    }

    public void DeselectObject()
    {
        selectedObjectPrefab = null;
        secondaryObjectPrefab = null;
        Debug.Log("取消選擇物件");
    }
}