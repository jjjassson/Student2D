using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPlacer : MonoBehaviour
{
    public GameObject selectedObjectPrefab;
    public Camera mainCamera;
    public float placeDepth = 10f;

    void Update()
    {
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

    // 讓 UI 按鈕呼叫，設定目前要放置的 prefab
    public void SetSelectedObject(GameObject prefab)
    {
        selectedObjectPrefab = prefab;
        Debug.Log("已選擇物件：" + prefab.name);
    }
}
