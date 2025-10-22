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

            Instantiate(selectedObjectPrefab, worldPos, Quaternion.identity);
            Debug.Log("放置物件於：" + worldPos);
        }
    }

    // ==============================
    // 由 UI 按鈕呼叫：右鍵選擇物件
    // ==============================
    public void SelectObjectFromButton(GameObject prefab)
    {
        if (prefab == null) return;

        selectedObjectPrefab = prefab;
        Debug.Log("右鍵選擇物件：" + prefab.name);
    }

    // 可選：取消選擇
    public void DeselectObject()
    {
        selectedObjectPrefab = null;
        Debug.Log("取消選擇物件");
    }
}