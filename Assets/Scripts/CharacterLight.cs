using UnityEngine;

// 此腳本專門用於在角色下方投射一個可配置的圓形光斑（Blob Shadow）
public class CharacterLight : MonoBehaviour
{
    [Header("投射設定")]
    [Tooltip("射線從角色向下檢測的最大距離。")]
    public float maxDistance = 20f;
    [Tooltip("射線起點相對於角色中心（transform.position）的垂直偏移量，應設置在角色腳部上方。")]
    public Vector3 raycastOffset = Vector3.up * 0.5f;
    [Tooltip("Raycast 應該檢測的 Layer Mask。請確保排除光斑自身的 Layer。")]
    public LayerMask hitMask = ~0; // 預設為 ~0 (檢測所有 Layer)

    [Header("地面光斑設定")]
    [Tooltip("用於顯示光斑的預製體（建議使用 Quad 並搭配圓形透明紋理）。")]
    public GameObject groundLightPrefab;
    [Tooltip("光斑的顏色（您要求為黑色，但可以調整）。")]
    public Color shadowColor = Color.black;

    // --- 修改處 1：將半徑 (Radius) 改為 尺寸 (Size) 以支援長寬獨立調整 ---
    [Tooltip("光斑的尺寸 (X: 寬度, Y: 長度/高度)")]
    public Vector2 shadowSize = new Vector2(1f, 1f);

    [Tooltip("光斑與地面分離的距離，防止 Z-fighting。")]
    public float surfaceOffset = 0.05f;

    [Header("平滑追隨設定 (解決卡頓)")]
    [Tooltip("光斑追隨目標位置的速度。值越高，追隨越快，值低則越平滑。建議 10-20。")]
    public float smoothingSpeed = 15f;

    // 私有變數
    private GameObject groundLightInstance;
    private Renderer groundLightRenderer;
    private Vector3 targetPosition; // 存儲 Raycast 擊中的目標位置

    private void Awake()
    {
        // --- 1. 檢查與初始化 ---

        if (groundLightPrefab == null)
        {
            Debug.LogError("🔴 錯誤：請在 Inspector 中指定 'Ground Light Prefab' 欄位！");
            return;
        }

        // 實例化光斑物件，位置在 LateUpdate 中單獨控制
        groundLightInstance = Instantiate(groundLightPrefab);
        groundLightInstance.transform.parent = null;

        // 獲取 Renderer 元件
        groundLightRenderer = groundLightInstance.GetComponent<Renderer>();
        if (groundLightRenderer == null)
        {
            Debug.LogError("🔴 錯誤：光斑預製體上找不到 Renderer 元件！請確認 Prefab 設定。");
        }

        // 為光斑材質創建實例並初始化顏色
        if (groundLightRenderer != null)
        {
            // 創建材質實例，避免影響共用材質的其他物件
            groundLightRenderer.material = new Material(groundLightRenderer.sharedMaterial);
            groundLightRenderer.material.SetColor("_Color", shadowColor);
        }
    }

    // 使用 LateUpdate 確保在所有角色的移動（FixedUpdate/Update）完成後才更新光斑位置
    private void LateUpdate()
    {
        // 確保核心元件存在
        if (groundLightInstance == null || groundLightRenderer == null) return;

        // --- 2. 動態同步屬性 ---

        // --- 修改處 2：分別應用 X 和 Y 的縮放 ---
        // 這裡設定 scale.z 為 1，因為 Quad 通常是平面的，Z 軸縮放不影響視覺
        groundLightInstance.transform.localScale = new Vector3(shadowSize.x, shadowSize.y, 1f);

        // 同步顏色
        groundLightRenderer.material.SetColor("_Color", shadowColor);

        // --- 3. 射線檢測與定位 ---

        Vector3 startPos = transform.position + raycastOffset;
        bool hitGround = false;

        // 進行 Raycast 檢測
        if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, maxDistance, hitMask))
        {
            hitGround = true;

            // 計算 Raycast 擊中的目標位置
            targetPosition = hit.point + hit.normal * surfaceOffset;

            // 旋轉：讓光斑平面貼合地面的角度（處理斜坡）
            groundLightInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // 額外旋轉修正：如果你發現拉長後方向不對（例如想要順著角色方向拉長），
            // 可以取消註解下面這行來讓光斑跟隨角色的面向：
            // groundLightInstance.transform.rotation *= transform.rotation; 
        }

        if (hitGround)
        {
            // 顯示光斑
            if (!groundLightInstance.activeSelf) groundLightInstance.SetActive(true);

            // 核心平滑邏輯：使用 Lerp 讓光斑位置平滑地趨近目標位置
            groundLightInstance.transform.position = Vector3.Lerp(
                groundLightInstance.transform.position,     // 起點：光斑當前位置
                targetPosition,                             // 終點：Raycast 擊中的目標位置
                Time.deltaTime * smoothingSpeed             // 插值量，實現流暢的跟隨
            );
        }
        else
        {
            // 射線未擊中物體，隱藏光斑
            if (groundLightInstance.activeSelf) groundLightInstance.SetActive(false);
        }
    }

    // 當腳本被禁用時隱藏光斑
    private void OnDisable()
    {
        if (groundLightInstance != null)
        {
            groundLightInstance.SetActive(false);
        }
    }

    // 當腳本被啟用時顯示光斑
    private void OnEnable()
    {
        if (groundLightInstance != null)
        {
            groundLightInstance.SetActive(true);
        }
    }

    // 銷毀物件時清理光斑實例
    private void OnDestroy()
    {
        if (groundLightInstance != null)
        {
            Destroy(groundLightInstance);
        }
    }
}