using UnityEngine;
using UnityEngine.InputSystem;

public class GridObjectPlacer : MonoBehaviour
{
    [Header("網格設定")]
    public float gridSize = 1.0f;
    public Vector3 startPosition = Vector3.zero;

    [Header("移動手感設定")]
    public float moveInterval = 0.2f;
    public float inputThreshold = 0.5f;
    public Vector2 xRange = new Vector2(-18, 18);
    public Vector2 zRange = new Vector2(-10, 10);

    // --- 內部變數 ---
    private GameObject currentMainPrefab;
    private GameObject currentSecondaryPrefab; // 新增：副物件 Prefab

    private GameObject mainGhostObject;      // 修改：主鬼影
    private GameObject secondaryGhostObject; // 新增：副鬼影

    private Vector3 currentGridPos; // 這是主物件的座標
    private Vector2 currentInput;
    private float nextMoveTime = 0f;

    private bool isPlacementMode = false;

    void Update()
    {
        if (isPlacementMode)
        {
            HandleMovement();
            UpdateGhostPosition();
        }
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
            moveDir.x = Mathf.Sign(currentInput.x);
        else
            moveDir.z = Mathf.Sign(currentInput.y);

        currentGridPos += moveDir * gridSize;
        currentGridPos.x = Mathf.Clamp(currentGridPos.x, xRange.x, xRange.y);
        currentGridPos.z = Mathf.Clamp(currentGridPos.z, zRange.x, zRange.y);
        currentGridPos.y = 0;

        nextMoveTime = Time.time + moveInterval;
    }

    // --- 核心邏輯：更新鬼影位置 ---
    private void UpdateGhostPosition()
    {
        // 1. 更新主物件 (跟隨游標)
        if (mainGhostObject != null)
        {
            mainGhostObject.transform.position = currentGridPos;
        }

        // 2. 更新副物件 (X 跟隨主物件, Z 強制歸零)
        if (secondaryGhostObject != null)
        {
            // 🔥 這裡實作你的需求：Z 都在 0 位置
            Vector3 secondaryPos = new Vector3(currentGridPos.x, 0, 0);
            secondaryGhostObject.transform.position = secondaryPos;
        }
    }

    // --- 被 Manager 呼叫：發牌 (接收兩個物件) ---
    public void AssignNewObjectPair(GameObject main, GameObject secondary)
    {
        currentMainPrefab = main;
        currentSecondaryPrefab = secondary;

        // 清除舊鬼影
        if (mainGhostObject != null) Destroy(mainGhostObject);
        if (secondaryGhostObject != null) Destroy(secondaryGhostObject);

        // 重置游標到初始點
        float snappedX = Mathf.Round(startPosition.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(startPosition.z / gridSize) * gridSize;
        currentGridPos = new Vector3(snappedX, 0, snappedZ);

        // 生成主鬼影
        if (currentMainPrefab != null)
        {
            mainGhostObject = Instantiate(currentMainPrefab, currentGridPos, Quaternion.identity);
            DisableColliders(mainGhostObject);
        }

        // 生成副鬼影 (如果有的話)
        if (currentSecondaryPrefab != null)
        {
            Vector3 secondaryPos = new Vector3(currentGridPos.x, 0, 0); // 初始位置也要 Z=0
            secondaryGhostObject = Instantiate(currentSecondaryPrefab, secondaryPos, Quaternion.identity);
            DisableColliders(secondaryGhostObject);
        }
    }

    // 輔助：關閉鬼影的 Collider
    private void DisableColliders(GameObject obj)
    {
        foreach (var c in obj.GetComponentsInChildren<Collider>()) c.enabled = false;
    }

    // --- 被 Manager 呼叫：開關放置模式 ---
    public void SetPlacementMode(bool active)
    {
        isPlacementMode = active;

        if (!active)
        {
            // 時間到，強制放置
            if (mainGhostObject != null)
            {
                PlaceObject();
            }
        }
    }

    public void OnMoveCursor(InputAction.CallbackContext context)
    {
        currentInput = context.ReadValue<Vector2>();
    }

    public void OnPlaceObject(InputAction.CallbackContext context)
    {
        if (context.performed && isPlacementMode)
        {
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        // 至少要有主物件才能放
        if (currentMainPrefab == null || mainGhostObject == null) return;

        // 1. 生成主實體
        Instantiate(currentMainPrefab, currentGridPos, Quaternion.identity);

        // 2. 生成副實體 (如果有)
        if (currentSecondaryPrefab != null)
        {
            // 🔥 確認放置時 Z 也是 0
            Vector3 placePos = new Vector3(currentGridPos.x, 0, 0);
            Instantiate(currentSecondaryPrefab, placePos, Quaternion.identity);
        }

        // 3. 銷毀鬼影
        if (mainGhostObject != null) { Destroy(mainGhostObject); mainGhostObject = null; }
        if (secondaryGhostObject != null) { Destroy(secondaryGhostObject); secondaryGhostObject = null; }

        isPlacementMode = false;
    }
}