using UnityEngine;
using UnityEngine.InputSystem;

public class GridObjectPlacer : MonoBehaviour
{
    [Header("網格設定")]
    public float gridSize = 1.0f; // 每一格的大小 (例如 1x1)
    public float moveSpeed = 10f; // 游標移動速度

    [Header("活動範圍")]
    public Vector2 xRange = new Vector2(-18, 18);
    public Vector2 zRange = new Vector2(-10, 10);

    [Header("物件參照")]
    public GameObject selectedObjectPrefab;
    public GameObject secondaryObjectPrefab;

    // 這是「預覽物件」，半透明顯示玩家目前選到的位置
    private GameObject ghostObject;

    // 內部變數
    private Vector2 currentInput; // 搖桿輸入值
    private Vector3 cursorPosition; // 游標的「平滑」位置
    private Vector3 snappedPosition; // 吸附網格後的「實際」位置

    private bool isPlacementAllowed = true; // 可搭配 RoundManager 控制

    private void Start()
    {
        // 初始位置設為範圍中心
        cursorPosition = new Vector3(0, 0, 0);
        UpdateGhostObject();
    }

    private void Update()
    {
        if (!isPlacementAllowed) return;

        // 1. 處理移動 (根據搖桿輸入更新座標)
        // Time.deltaTime 確保移動平滑
        cursorPosition.x += currentInput.x * moveSpeed * Time.deltaTime;
        cursorPosition.z += currentInput.y * moveSpeed * Time.deltaTime;

        // 2. 限制範圍 (Clamp)
        cursorPosition.x = Mathf.Clamp(cursorPosition.x, xRange.x, xRange.y);
        cursorPosition.z = Mathf.Clamp(cursorPosition.z, zRange.x, zRange.y);

        // 3. 計算網格吸附 (Snap to Grid)
        // 原理：除以網格大小 -> 四捨五入 -> 乘回網格大小
        float snappedX = Mathf.Round(cursorPosition.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(cursorPosition.z / gridSize) * gridSize;

        // 確保高度 Y=0
        snappedPosition = new Vector3(snappedX, 0, snappedZ);

        // 4. 更新預覽物件 (Ghost) 的位置
        if (ghostObject != null)
        {
            ghostObject.transform.position = snappedPosition;
        }
        else
        {
            // 如果還沒有 ghost，生成一個
            UpdateGhostObject();
        }
    }

    // --- Input System 事件綁定 ---
    // 請在 PlayerInput 的 Events -> Invoke Unity Events 中，將對應事件拉給這兩個函式

    // 對應 Right Stick
    public void OnMoveCursor(InputAction.CallbackContext context)
    {
        currentInput = context.ReadValue<Vector2>();
    }

    // 對應 Right Trigger
    public void OnPlaceObject(InputAction.CallbackContext context)
    {
        // 只在按下瞬間觸發
        if (context.performed && isPlacementAllowed)
        {
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        if (selectedObjectPrefab == null) return;

        // 1. 放置主物件 (跟隨網格位置)
        Instantiate(selectedObjectPrefab, snappedPosition, Quaternion.identity);

        // 2. 放置副物件 (依照你的規則：Z 固定為 0)
        if (secondaryObjectPrefab != null)
        {
            // 副物件的 X 跟隨主物件，但 Z 強制歸零
            Vector3 secondaryPos = new Vector3(snappedPosition.x, 0, 0);
            Instantiate(secondaryObjectPrefab, secondaryPos, Quaternion.identity);
        }

        Debug.Log($"玩家放置於: {snappedPosition}");

        // 可選：放置後是否要在原處冷卻，或重設位置? 
        // 既然是搖桿，通常保留在原處比較好操作，所以這裡不重設 cursorPosition
    }

    // --- 視覺優化：建立半透明預覽 ---
    private void UpdateGhostObject()
    {
        if (selectedObjectPrefab == null) return;

        // 如果舊的有，先刪掉
        if (ghostObject != null) Destroy(ghostObject);

        // 生成新的預覽物件
        ghostObject = Instantiate(selectedObjectPrefab, cursorPosition, Quaternion.identity);

        // 移除所有碰撞體 (Collider)，避免預覽物件擋路或觸發物理
        Collider[] colliders = ghostObject.GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;

        // (進階) 如果想變半透明，需要修改材質，這裡先簡單略過，
        // 你可以先讓它維持原樣，或者掛一個簡單的半透明材質上去
    }

    // 用於 Gizmos 輔助 (在 Scene 視窗看範圍)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(0, 0, 0), new Vector3(xRange.y - xRange.x, 0.1f, zRange.y - zRange.x));
    }
}