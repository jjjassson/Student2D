using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ObjectPlacer : MonoBehaviour
{
    [Header("放置設定")]
    public Camera mainCamera;
    public float placeDepth = 10f;
    public float scrollSpeed = 2f;
    public float minDepth = 1f;
    public float maxDepth = 50f;

    [Header("放置物件")]
    public GameObject selectedObjectPrefab;
    public GameObject secondaryObjectPrefab;

    private float lastDepthDisplayTime;

    // 新增：放置限制控制
    private bool isPlacementAllowed = false; // 是否在放置時間內 (由 RoundManager 控制)
    private bool hasPlacedThisPhase = true; // 本輪放置階段是否已放置 (預設為 true，避免在放置階段開始前放置)

    private RoundManager roundManager;

    void Start()
    {
        // 取得 RoundManager 實例並註冊事件
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
            // 嘗試取得主攝影機
            mainCamera = Camera.main;
        }
    }

    void OnDestroy()
    {
        if (roundManager != null)
        {
            roundManager.OnPlacementStart -= SetPlacementCooldown;
            roundManager.OnPlacementAllowedChange -= SetPlacementAllowed;
        }
    }

    // 從 RoundManager 接收事件，重置單次放置限制
    // 這個方法在每輪放置開始時被呼叫
    private void SetPlacementCooldown(float placementTime)
    {
        hasPlacedThisPhase = false; // 新一輪放置開始時，重置放置標記 (允許放置)
    }

    // 設置是否允許放置
    // 這個方法在放置階段開始/結束/間隔冷卻時被呼叫
    private void SetPlacementAllowed(bool isAllowed)
    {
        isPlacementAllowed = isAllowed;

        if (!isAllowed)
        {
            // 確保當放置階段結束或進入冷卻時，立即禁用放置
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

        // ❌ 檢查是否處於允許放置階段 且 尚未在本階段放置 (單次放置限制)
        if (!isPlacementAllowed || hasPlacedThisPhase) return;

        // --- 執行放置 (滑鼠左鍵) ---
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (selectedObjectPrefab == null)
            {
                Debug.LogWarning("尚未選擇任何放置物件！");
                return;
            }

            // 座標計算
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, placeDepth)
            );

            // 執行放置
            GameObject mainObj = Instantiate(selectedObjectPrefab, worldPos, Quaternion.identity);

            // 放置副物件 (Z=0)
            if (secondaryObjectPrefab != null)
            {
                Vector3 secondaryPos = new Vector3(worldPos.x, worldPos.y, 0f);
                Instantiate(secondaryObjectPrefab, secondaryPos, Quaternion.identity);
            }

            // ✅ 放置完成後，設定本輪已放置 (進入冷卻直到下一輪放置階段開始)
            hasPlacedThisPhase = true;
            Debug.Log($"物件 {selectedObjectPrefab.name} 放置完成。本輪放置結束，需等待下一輪。");
        }
    }

    // --- 螢幕上顯示目前深度 & 狀態 ---
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 20;
        style.normal.textColor = Color.yellow;

        // 顯示深度
        if (Time.time - lastDepthDisplayTime < 2f)
        {
            GUI.Label(new Rect(10, 10, 300, 40), $"目前深度：{placeDepth:F2}", style);
        }

        // 顯示放置狀態
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
        // 使用 RoundManager 中公開的 IsRoundActive 屬性 (解決錯誤)
        else if (roundManager != null && roundManager.IsRoundActive)
        {
            statusStyle.normal.textColor = Color.gray;
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 50, 300, 40), "遊玩中/冷卻中...", statusStyle);
        }
    }

    // --- 物件選擇方法 (由 ItemSelector 呼叫) ---
    // 請確保你的 ItemSelector 腳本傳入正確的主物件和副物件
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