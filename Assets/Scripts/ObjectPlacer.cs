using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ObjectPlacer : MonoBehaviour
{
    [Header("放置設定")]
    public Camera mainCamera;
    public float placeDepth = 10f;
    public float scrollSpeed = 2f;
    public float minDepth = 1f;
    public float maxDepth = 50f;

    [Header("放置物件")]
    public GameObject selectedObjectPrefab;
    public GameObject secondaryObjectPrefab;

    private float lastDepthDisplayTime;

    void Update()
    {
        float scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue != 0)
        {
            placeDepth -= scrollValue * scrollSpeed * Time.deltaTime;
            placeDepth = Mathf.Clamp(placeDepth, minDepth, maxDepth);
            lastDepthDisplayTime = Time.time; // 更新顯示時間
        }

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

            GameObject mainObj = Instantiate(selectedObjectPrefab, worldPos, Quaternion.identity);

            if (secondaryObjectPrefab != null)
            {
                Vector3 secondaryPos = new Vector3(worldPos.x, worldPos.y, 0f);
                Instantiate(secondaryObjectPrefab, secondaryPos, Quaternion.identity);
            }
        }
    }

    // --- 螢幕上顯示目前深度 ---
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 20;
        style.normal.textColor = Color.yellow;

        // 只有在最近滾輪操作後的 2 秒內顯示
        if (Time.time - lastDepthDisplayTime < 2f)
        {
            GUI.Label(new Rect(10, 10, 300, 40), $"目前深度：{placeDepth:F2}", style);
        }
    }

    public void SelectObjectFromButton(GameObject prefab)
    {
        if (prefab == null) return;

        selectedObjectPrefab = prefab;
        string secondaryName = prefab.name + "-1";
        GameObject secondary = Resources.Load<GameObject>(secondaryName);
        if (secondary != null)
        {
            secondaryObjectPrefab = secondary;
            Debug.Log($"找到副物件：{secondaryName}");
        }
        else
        {
            secondaryObjectPrefab = null;
        }

        Debug.Log("右鍵選擇物件：" + prefab.name);
    }

    public void DeselectObject()
    {
        selectedObjectPrefab = null;
        secondaryObjectPrefab = null;
        Debug.Log("取消選擇物件");
    }
}