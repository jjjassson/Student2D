using UnityEngine;
using UnityEngine.InputSystem;

public class GridObjectPlacer : MonoBehaviour
{
    [Header("網格設定")]
    public float gridSize = 1.0f; // 每一格的大小

    [Header("初始狀態設定")]
    [Tooltip("遊戲開始時，游標預設出現的位置")]
    public Vector3 startPosition = Vector3.zero; // <--- 新增這個變數

    [Header("移動手感設定")]
    [Tooltip("按住搖桿時，每隔幾秒移動一格")]
    public float moveInterval = 0.2f;
    [Tooltip("搖桿推超過多少才算有輸入")]
    public float inputThreshold = 0.5f;

    [Header("活動範圍")]
    public Vector2 xRange = new Vector2(-18, 18);
    public Vector2 zRange = new Vector2(-10, 10);

    // --- 內部變數 ---
    private BuildingData currentData;
    private GameObject ghostObject;
    private Vector3 currentGridPos;

    private Vector2 currentInput;
    private float nextMoveTime = 0f;
    private bool isPlacementAllowed = true;

    void Start()
    {
        // --- 修改處：使用你設定的初始位置 ---

        // 1. 先讀取你設定的數值
        Vector3 initPos = startPosition;

        // 2. 強制對齊網格 (避免你手動輸入 1.2 這種小數點，導致後面全部歪掉)
        float snappedX = Mathf.Round(initPos.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(initPos.z / gridSize) * gridSize;

        // 3. 設定初始座標 (強制 Y=0)
        currentGridPos = new Vector3(snappedX, 0, snappedZ);

        // 4. 確保初始位置在活動範圍內 (避免一開始就在邊界外)
        currentGridPos.x = Mathf.Clamp(currentGridPos.x, xRange.x, xRange.y);
        currentGridPos.z = Mathf.Clamp(currentGridPos.z, zRange.x, zRange.y);
    }

    void Update()
    {
        if (!isPlacementAllowed) return;

        HandleMovement();
        UpdateGhostPosition();
    }

    private void HandleMovement()
    {
        if (Time.time < nextMoveTime || currentInput.magnitude < inputThreshold)
        {
            if (currentInput.magnitude < 0.1f) nextMoveTime = 0f;
            return;
        }

        Vector3 moveDir = Vector3.zero;

        if (Mathf.Abs(currentInput.x) > Mathf.Abs(currentInput.y))
        {
            moveDir.x = Mathf.Sign(currentInput.x);
        }
        else
        {
            moveDir.z = Mathf.Sign(currentInput.y);
        }

        currentGridPos += moveDir * gridSize;

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

    public void UpdateBuildingData(BuildingData newData)
    {
        currentData = newData;

        if (ghostObject != null) Destroy(ghostObject);

        if (currentData != null && currentData.prefab != null)
        {
            ghostObject = Instantiate(currentData.prefab, currentGridPos, Quaternion.identity);
            foreach (var c in ghostObject.GetComponentsInChildren<Collider>()) c.enabled = false;
        }
    }

    public void OnMoveCursor(InputAction.CallbackContext context)
    {
        currentInput = context.ReadValue<Vector2>();
    }

    public void OnPlaceObject(InputAction.CallbackContext context)
    {
        if (context.performed && isPlacementAllowed)
        {
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        if (currentData == null || currentData.prefab == null) return;
        Instantiate(currentData.prefab, currentGridPos, Quaternion.identity);
        Debug.Log($"放置於格子: {currentGridPos}");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((xRange.x + xRange.y) / 2, 0, (zRange.x + zRange.y) / 2);
        Vector3 size = new Vector3(xRange.y - xRange.x, 0.1f, zRange.y - zRange.x);
        Gizmos.DrawWireCube(center, size);

        // 畫出初始位置 (紅色球)，方便你在編輯器看
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(startPosition, 0.3f);
    }
}