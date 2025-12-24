using UnityEngine;
using UnityEngine.InputSystem;

public class GridObjectPlacer : MonoBehaviour
{
    [Header("網格設定")]
    public float gridSize = 1.0f;
    public Vector3 startPosition = Vector3.zero; // 初始位置

    [Header("手感設定")]
    public float moveInterval = 0.2f; // 移動冷卻時間
    public float inputThreshold = 0.5f;

    [Header("範圍限制")]
    public Vector2 xRange = new Vector2(-18, 18);
    public Vector2 zRange = new Vector2(-10, 10);

    // --- 內部變數 ---
    private BuildingData currentData;
    private GameObject ghostObject;
    private Vector3 currentGridPos;

    private Vector2 currentInput;
    private float nextMoveTime = 0f;
    private bool isPlacementAllowed = false; // 預設為 false，等 RoundManager 開啟

    void Start()
    {
        // 初始化位置並對齊網格
        currentGridPos = new Vector3(
            Mathf.Round(startPosition.x / gridSize) * gridSize,
            0,
            Mathf.Round(startPosition.z / gridSize) * gridSize
        );
    }

    void Update()
    {
        if (!isPlacementAllowed) return;

        HandleMovement();
        UpdateGhostPosition();
    }

    // 搖桿移動邏輯
    private void HandleMovement()
    {
        if (Time.time < nextMoveTime || currentInput.magnitude < inputThreshold)
        {
            if (currentInput.magnitude < 0.1f) nextMoveTime = 0f;
            return;
        }

        Vector3 moveDir = Vector3.zero;
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
        if (ghostObject != null) ghostObject.transform.position = currentGridPos;
    }

    // --- 供外部 (Inventory) 呼叫 ---
    public void SetBuildingData(BuildingData newData)
    {
        currentData = newData;

        if (ghostObject != null) Destroy(ghostObject);

        if (currentData != null && currentData.mainPrefab != null)
        {
            ghostObject = Instantiate(currentData.mainPrefab, currentGridPos, Quaternion.identity);
            // 移除 Ghost 的碰撞體，只留視覺
            foreach (var c in ghostObject.GetComponentsInChildren<Collider>()) c.enabled = false;
        }
    }

    // --- Input System 事件 ---
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
        if (currentData == null || currentData.mainPrefab == null) return;

        // 1. 放置主物件
        Instantiate(currentData.mainPrefab, currentGridPos, Quaternion.identity);

        // 2. 放置副物件 (固定 Z=0)
        if (currentData.secondaryPrefab != null)
        {
            Vector3 secondaryPos = new Vector3(currentGridPos.x, 0, 0);
            Instantiate(currentData.secondaryPrefab, secondaryPos, Quaternion.identity);
        }

        Debug.Log($"放置於: {currentGridPos}");
        // 這裡可以加入音效或特效
    }

    // --- 給 RoundManager 控制開關 ---
    public void SetPlacementAllowed(bool allowed)
    {
        isPlacementAllowed = allowed;
        if (ghostObject != null) ghostObject.SetActive(allowed);
    }
}