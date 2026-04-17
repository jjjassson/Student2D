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

    [Header("玩家圖片設定")]
    public Sprite playerIcon;
    public float iconHeightOffset = 2.0f; // 圖片在物件上方的高度
    public float iconScale = 1.0f;        // 圖片大小縮放比例 (預設 1)

    // --- 內部變數 ---
    private GameObject currentMainPrefab;
    private GameObject currentSecondaryPrefab;

    private GameObject mainGhostObject;
    private GameObject secondaryGhostObject;

    private GameObject currentIconObj; // 用來追蹤當前的圖片物件

    private Vector3 currentGridPos;
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

    private void UpdateGhostPosition()
    {
        if (mainGhostObject != null)
        {
            mainGhostObject.transform.position = currentGridPos;

            // 讓圖片永遠面向攝影機，避免變扁平
            if (currentIconObj != null && Camera.main != null)
            {
                currentIconObj.transform.forward = Camera.main.transform.forward;
            }
        }

        if (secondaryGhostObject != null)
        {
            Vector3 secondaryPos = new Vector3(currentGridPos.x, 0, 0);
            secondaryGhostObject.transform.position = secondaryPos;
        }
    }

    public void AssignNewObjectPair(GameObject main, GameObject secondary)
    {
        currentMainPrefab = main;
        currentSecondaryPrefab = secondary;

        // 清除舊鬼影與圖片
        if (mainGhostObject != null) Destroy(mainGhostObject);
        if (secondaryGhostObject != null) Destroy(secondaryGhostObject);
        if (currentIconObj != null) Destroy(currentIconObj);

        float snappedX = Mathf.Round(startPosition.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(startPosition.z / gridSize) * gridSize;
        currentGridPos = new Vector3(snappedX, 0, snappedZ);

        if (currentMainPrefab != null)
        {
            mainGhostObject = Instantiate(currentMainPrefab, currentGridPos, Quaternion.identity);
            DisableColliders(mainGhostObject);

            // 生成玩家圖片在主鬼影上
            CreatePlayerIcon(mainGhostObject.transform);
        }

        if (currentSecondaryPrefab != null)
        {
            Vector3 secondaryPos = new Vector3(currentGridPos.x, 0, 0);
            secondaryGhostObject = Instantiate(currentSecondaryPrefab, secondaryPos, Quaternion.identity);
            DisableColliders(secondaryGhostObject);
        }
    }

    // 輔助方法：生成玩家圖片並設定大小
    private void CreatePlayerIcon(Transform parentTransform)
    {
        if (playerIcon == null) return;

        currentIconObj = new GameObject("PlayerIcon");
        currentIconObj.transform.SetParent(parentTransform);
        currentIconObj.transform.localPosition = new Vector3(0, iconHeightOffset, 0);

        SpriteRenderer sr = currentIconObj.AddComponent<SpriteRenderer>();
        sr.sprite = playerIcon;
        sr.sortingOrder = 10;

        // 🔥 新增：動態掛載「防拉伸」小幫手腳本，鎖死絕對大小
        IconAntiStretch antiStretch = currentIconObj.AddComponent<IconAntiStretch>();
        antiStretch.targetScale = iconScale;
    }

    private void DisableColliders(GameObject obj)
    {
        foreach (var c in obj.GetComponentsInChildren<Collider>()) c.enabled = false;
    }

    public void SetPlacementMode(bool active)
    {
        isPlacementMode = active;

        if (!active) // 當 !active 代表放置時間結束，準備進入闖關時間
        {
            if (mainGhostObject != null)
            {
                PlaceObject(); // 時間到，強制放置
            }

            // 闖關時間開始，強制銷毀頭頂的玩家圖片
            if (currentIconObj != null)
            {
                Destroy(currentIconObj);
                currentIconObj = null;
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
        if (currentMainPrefab == null || mainGhostObject == null) return;

        // 1. 生成主實體
        GameObject placedMain = Instantiate(currentMainPrefab, currentGridPos, Quaternion.identity);

        // 2. 如果玩家提早放置，把圖片從「鬼影」轉移到「真實物件」上，直到闖關時間才消失
        if (currentIconObj != null)
        {
            currentIconObj.transform.SetParent(placedMain.transform);
            currentIconObj.transform.localPosition = new Vector3(0, iconHeightOffset, 0);
        }

        // 3. 生成副實體 (如果有)
        if (currentSecondaryPrefab != null)
        {
            Vector3 placePos = new Vector3(currentGridPos.x, 0, 0);
            Instantiate(currentSecondaryPrefab, placePos, Quaternion.identity);
        }

        // 4. 銷毀鬼影
        if (mainGhostObject != null) { Destroy(mainGhostObject); mainGhostObject = null; }
        if (secondaryGhostObject != null) { Destroy(secondaryGhostObject); secondaryGhostObject = null; }

        isPlacementMode = false; // 該玩家提早結束放置模式
    }
}

// 🔥 新增：放在同一個檔案裡的獨立類別，專門用來防止圖片跟隨父物件變形
public class IconAntiStretch : MonoBehaviour
{
    public float targetScale = 1.0f;

    void LateUpdate()
    {
        // 如果有父物件，就根據父物件的變形程度，進行「反向縮小/放大」
        if (transform.parent != null)
        {
            Vector3 parentScale = transform.parent.lossyScale;

            // 避免除以 0 產生錯誤
            if (parentScale.x == 0 || parentScale.y == 0 || parentScale.z == 0) return;

            transform.localScale = new Vector3(
                targetScale / parentScale.x,
                targetScale / parentScale.y,
                targetScale / parentScale.z
            );
        }
        else
        {
            // 如果沒有父物件，就保持正常的 Target Scale
            transform.localScale = new Vector3(targetScale, targetScale, targetScale);
        }
    }
}