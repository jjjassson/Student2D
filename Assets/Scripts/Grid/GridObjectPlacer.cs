using UnityEngine;
using UnityEngine.InputSystem;

public class GridObjectPlacer : MonoBehaviour
{
    [Header("網格設定")]
    public float gridSize = 1.0f;

    [Header("初始狀態設定")]
    // 依照你的要求，這裡預設為 (0,0,0)
    public Vector3 startPosition = Vector3.zero;

    [Header("移動手感設定")]
    public float moveInterval = 0.2f;
    public float inputThreshold = 0.5f;

    [Header("活動範圍")]
    public Vector2 xRange = new Vector2(-18, 18);
    public Vector2 zRange = new Vector2(-10, 10);

    // --- 內部變數 ---
    private GameObject currentPrefab; // 修改：直接儲存 Prefab
    private GameObject ghostObject;
    private Vector3 currentGridPos;

    private Vector2 currentInput; // 這裡將會接收右搖桿的訊號
    private float nextMoveTime = 0f;
    private bool isPlacementAllowed = false; // 預設關閉，等 RoundManager 發牌才開啟

    void Update()
    {
        if (!isPlacementAllowed) return;

        HandleMovement();
        UpdateGhostPosition();
    }

    // --- 你的核心移動邏輯 (保持不變) ---
    private void HandleMovement()
    {
        if (Time.time < nextMoveTime || currentInput.magnitude < inputThreshold)
        {
            if (currentInput.magnitude < 0.1f) nextMoveTime = 0f;
            return;
        }

        Vector3 moveDir = Vector3.zero;

        // 判斷移動方向
        if (Mathf.Abs(currentInput.x) > Mathf.Abs(currentInput.y))
            moveDir.x = Mathf.Sign(currentInput.x);
        else
            moveDir.z = Mathf.Sign(currentInput.y);

        currentGridPos += moveDir * gridSize;

        // 限制範圍 & 強制 Y=0
        currentGridPos.x = Mathf.Clamp(currentGridPos.x, xRange.x, xRange.y);
        currentGridPos.z = Mathf.Clamp(currentGridPos.z, zRange.x, zRange.y);
        currentGridPos.y = 0;

        nextMoveTime = Time.time + moveInterval;
    }

    private void UpdateGhostPosition()
    {
        if (ghostObject != null)
        {
            ghostObject.transform.position = currentGridPos;
        }
    }

    // --- 被 RoundManager 呼叫的函式 ---
    public void AssignNewObject(GameObject prefab)
    {
        currentPrefab = prefab;

        // 1. 清理舊的
        if (ghostObject != null) Destroy(ghostObject);

        // 2. 重置位置到你指定的 StartPosition (0,0,0)
        // 並進行網格對齊計算
        float snappedX = Mathf.Round(startPosition.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(startPosition.z / gridSize) * gridSize;
        currentGridPos = new Vector3(snappedX, 0, snappedZ);

        // 3. 生成預覽物件 (Ghost)
        if (currentPrefab != null)
        {
            ghostObject = Instantiate(currentPrefab, currentGridPos, Quaternion.identity);
            // 移除碰撞體只留視覺
            foreach (var c in ghostObject.GetComponentsInChildren<Collider>()) c.enabled = false;
        }

        isPlacementAllowed = true;
    }

    // --- Input System 綁定區 ---

    // 請在 PlayerInput 的 Events 裡綁定 "RightStick" 到這裡
    public void OnMoveCursor(InputAction.CallbackContext context)
    {
        currentInput = context.ReadValue<Vector2>();
    }

    // 請在 PlayerInput 的 Events 裡綁定 "RightStickPress" 到這裡
    public void OnPlaceObject(InputAction.CallbackContext context)
    {
        if (context.performed && isPlacementAllowed)
        {
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        if (currentPrefab == null) return;

        Instantiate(currentPrefab, currentGridPos, Quaternion.identity);
        Debug.Log($"放置於格子: {currentGridPos}");

        // 放置後看你要不要關閉功能，或銷毀 Ghost
        Destroy(ghostObject);
        isPlacementAllowed = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((xRange.x + xRange.y) / 2, 0, (zRange.x + zRange.y) / 2);
        Vector3 size = new Vector3(xRange.y - xRange.x, 0.1f, zRange.y - zRange.x);
        Gizmos.DrawWireCube(center, size);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(startPosition, 0.3f);
    }
}