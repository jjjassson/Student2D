using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPlacer : MonoBehaviour
{
    [Header("放置設定")]
    public Camera mainCamera;                 // 主攝影機
    public float placeDepth = 10f;            // 初始深度
    public float scrollSpeed = 2f;            // 滾輪調整速度
    public float minDepth = 1f;               // 最小深度
    public float maxDepth = 50f;              // 最大深度

    [Header("放置物件")]
    public GameObject selectedObjectPrefab;   // 當前選擇的物件

    [Header("副物件對應表")]
    public GameObject secondaryObjectPrefab;  // 選擇的物件對應 Z=0 的副物件

    void Update()
    {
        // ==============================
        // 滾輪調整放置深度
        // ==============================
        float scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue != 0)
        {
            placeDepth -= scrollValue * scrollSpeed * Time.deltaTime;
            placeDepth = Mathf.Clamp(placeDepth, minDepth, maxDepth);
            Debug.Log($"目前深度：{placeDepth:F2}");
        }

        // ==============================
        // 左鍵放置物件
        // ==============================
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (selectedObjectPrefab == null)
            {
                Debug.LogWarning("尚未選擇任何放置物件！");
                return;
            }

            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, placeDepth)
            );

            // 放置主物件
            GameObject mainObj = Instantiate(selectedObjectPrefab, worldPos, Quaternion.identity);
            Debug.Log("放置主物件於：" + worldPos);

            // 如果有對應副物件，放置副物件於 Z=0
            if (secondaryObjectPrefab != null)
            {
                Vector3 secondaryPos = new Vector3(worldPos.x, worldPos.y, 0f);
                Instantiate(secondaryObjectPrefab, secondaryPos, Quaternion.identity);
                Debug.Log("放置副物件於：" + secondaryPos);
            }
        }
    }

    // ==============================
    // 由 UI 按鈕呼叫：右鍵選擇物件
    // ==============================
    public void SelectObjectFromButton(GameObject prefab)
    {
        if (prefab == null) return;

        selectedObjectPrefab = prefab;

        // 嘗試自動找對應副物件（例如命名規則：A 對應 A-1）
        string secondaryName = prefab.name + "-1";
        GameObject secondary = Resources.Load<GameObject>(secondaryName); // 從 Resources 資料夾載入
        if (secondary != null)
        {
            secondaryObjectPrefab = secondary;
            Debug.Log($"找到副物件：{secondaryName}");
        }
        else
        {
            secondaryObjectPrefab = null;
            Debug.LogWarning($"未找到副物件：{secondaryName}");
        }

        Debug.Log("右鍵選擇物件：" + prefab.name);
    }

    // 可選：取消選擇
    public void DeselectObject()
    {
        selectedObjectPrefab = null;
        secondaryObjectPrefab = null;
        Debug.Log("取消選擇物件");
    }
}