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
    private GameObject currentPrefab;
    private GameObject ghostObject;
    private Vector3 currentGridPos;
    private Vector2 currentInput;
    private float nextMoveTime = 0f;

    // 雖然變數叫 Allowed，但我們只用它來開關「游標移動」，不鎖角色移動
    private bool isPlacementMode = false;

    void Update()
    {
        // 只有在放置模式下才計算游標移動
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
        if (ghostObject != null)
        {
            ghostObject.transform.position = currentGridPos;
        }
    }

    // --- 被 Manager 呼叫：發牌 ---
    public void AssignNewObject(GameObject prefab)
    {
        currentPrefab = prefab;
        if (ghostObject != null) Destroy(ghostObject);

        // 重置游標到初始點 (0,0,0)
        float snappedX = Mathf.Round(startPosition.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(startPosition.z / gridSize) * gridSize;
        currentGridPos = new Vector3(snappedX, 0, snappedZ);

        if (currentPrefab != null)
        {
            ghostObject = Instantiate(currentPrefab, currentGridPos, Quaternion.identity);
            foreach (var c in ghostObject.GetComponentsInChildren<Collider>()) c.enabled = false;
        }

        // 注意：這裡不主動開 isPlacementMode，由 Manager 統一控制
    }

    // --- 被 Manager 呼叫：開關放置模式 ---
    public void SetPlacementMode(bool active)
    {
        isPlacementMode = active;

        if (!active)
        {
            // 🔥 重點修改：當時間到被關閉時，如果鬼影還在，就強制放置！
            if (ghostObject != null)
            {
                PlaceObject();
            }
        }
    }

    // --- Input System 綁定 ---
    public void OnMoveCursor(InputAction.CallbackContext context)
    {
        // 這裡只讀取數值，完全不干擾你的角色移動腳本 (CharacterController)
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
        if (currentPrefab == null || ghostObject == null) return;

        // 生成實體
        Instantiate(currentPrefab, currentGridPos, Quaternion.identity);

        // 銷毀鬼影
        Destroy(ghostObject);
        ghostObject = null; // 確保不會被重複放置

        // 關閉放置模式 (這回合完成了)
        isPlacementMode = false;
    }
}