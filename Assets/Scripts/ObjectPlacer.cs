using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPlacer : MonoBehaviour
{
    [Header("放置設定")]
    public Camera mainCamera;

    [Header("UI 顯示設定")]
    public int debugInfoFontSize = 40;
    public int statusTextFontSize = 60;

    [Header("放置物件")]
    public GameObject selectedObjectPrefab;
    public GameObject secondaryObjectPrefab;

    // 狀態變數
    private bool isPlacementAllowed = false;
    private bool hasPlacedThisPhase = true;

    // 用來顯示 UI 的座標
    private Vector3 currentMousePos;

    private RoundManager roundManager;

    void Start()
    {
        roundManager = RoundManager.Instance;
        if (roundManager != null)
        {
            roundManager.OnPlacementStart += SetPlacementCooldown;
            roundManager.OnPlacementAllowedChange += SetPlacementAllowed;
        }
        else
        {
            Debug.LogError("ObjectPlacer 找不到 RoundManager 實例！");
        }

        if (mainCamera == null) mainCamera = Camera.main;
    }

    void OnDestroy()
    {
        if (roundManager != null)
        {
            roundManager.OnPlacementStart -= SetPlacementCooldown;
            roundManager.OnPlacementAllowedChange -= SetPlacementAllowed;
        }
    }

    private void SetPlacementCooldown(float placementTime)
    {
        hasPlacedThisPhase = false;
        Debug.Log("放置階段開始");
    }

    private void SetPlacementAllowed(bool isAllowed)
    {
        isPlacementAllowed = isAllowed;
        if (!isAllowed) hasPlacedThisPhase = true;
    }

    void Update()
    {
        if (!isPlacementAllowed || hasPlacedThisPhase) return;

        // 1. 建立一個位於 Y=0 的數學平面
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // 2. 射線檢測：找出滑鼠點在地板上的哪裡
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            // --- 核心邏輯修改 ---

            // 物件 1 (主物件): X, Z 隨滑鼠，Y 固定 0
            Vector3 mainObjPos = new Vector3(hitPoint.x, 0, hitPoint.z);

            // 物件 2 (副物件): X 隨滑鼠，Y 固定 0，Z 強制固定 0 (根據你的要求)
            Vector3 secondaryObjPos = new Vector3(hitPoint.x, 0, 0);

            // 更新 UI 顯示用的座標
            currentMousePos = mainObjPos;

            // 3. 按下滑鼠左鍵放置
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (selectedObjectPrefab == null) return;

                // 放置主物件
                Instantiate(selectedObjectPrefab, mainObjPos, Quaternion.identity);

                // 放置副物件 (如果有設定的話)
                if (secondaryObjectPrefab != null)
                {
                    Instantiate(secondaryObjectPrefab, secondaryObjPos, Quaternion.identity);
                }

                hasPlacedThisPhase = true;
                Debug.Log($"放置完成: 主物件於 {mainObjPos}, 副物件於 {secondaryObjPos}");
            }
        }
    }

    void OnGUI()
    {
        // 左上角座標顯示
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = debugInfoFontSize;
        style.normal.textColor = Color.yellow;
        style.fontStyle = FontStyle.Bold;

        if (isPlacementAllowed && !hasPlacedThisPhase)
        {
            // 顯示目前滑鼠在地板上的座標
            GUI.Label(new Rect(20, 20, 600, 100),
                $"放置位置: X={currentMousePos.x:F1}, Y=0, Z={currentMousePos.z:F1}", style);
        }

        // 下方狀態文字
        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.fontSize = statusTextFontSize;
        statusStyle.alignment = TextAnchor.MiddleCenter;
        statusStyle.fontStyle = FontStyle.Bold;

        float labelWidth = 800f;
        float labelHeight = 150f;
        Rect statusRect = new Rect((Screen.width - labelWidth) / 2, Screen.height - labelHeight - 20, labelWidth, labelHeight);

        if (isPlacementAllowed)
        {
            if (hasPlacedThisPhase)
            {
                statusStyle.normal.textColor = Color.red;
                GUI.Label(statusRect, "已放置", statusStyle);
            }
            else
            {
                statusStyle.normal.textColor = Color.green;
                GUI.Label(statusRect, "請點擊地板放置", statusStyle);
            }
        }
        else if (roundManager != null && roundManager.IsRoundActive)
        {
            statusStyle.normal.textColor = Color.gray;
            GUI.Label(statusRect, "遊戲進行中", statusStyle);
        }
    }

    public void SelectObjectFromButton(GameObject mainPrefab, GameObject secondaryPrefab = null)
    {
        selectedObjectPrefab = mainPrefab;
        secondaryObjectPrefab = secondaryPrefab;
    }

    public void DeselectObject()
    {
        selectedObjectPrefab = null;
        secondaryObjectPrefab = null;
    }
}