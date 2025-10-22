using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPlacer : MonoBehaviour
{
    [Header("放置設定")]
    public GameObject selectedObjectPrefab;
    public Camera mainCamera;
    public float placeDepth = 10f;         // 初始放置深度
    public float scrollSpeed = 2f;         // 滾輪調整速度
    public float minDepth = 1f;            // 最小深度
    public float maxDepth = 50f;           // 最大深度

    void Update()
    {
        // 🔄 用滑鼠滾輪調整放置深度
        float scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue != 0)
        {
            placeDepth -= scrollValue * scrollSpeed * Time.deltaTime; // 向上滾→往遠處放
            placeDepth = Mathf.Clamp(placeDepth, minDepth, maxDepth); // 限制深度範圍
            Debug.Log($"目前深度：{placeDepth:F2}");
        }

        // 🖱️ 點擊左鍵放置物件
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

    // 🧭 讓 UI 按鈕呼叫，設定目前要放置的 prefab
    public void SetSelectedObject(GameObject prefab)
    {
        selectedObjectPrefab = prefab;
        Debug.Log("已選擇物件：" + prefab.name);
    }
}