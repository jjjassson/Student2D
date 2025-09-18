using UnityEngine;
using UnityEngine.UI;

public class PrefabSelectorButton : MonoBehaviour
{
    public ObjectPlacer placer;            // 拖 ObjectPlacer 的物件進來
    public GameObject prefabToPlace;       // 拖你想選擇的 Prefab 進來

    void Start()
    {
        // 設定按鈕點擊事件
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (placer != null && prefabToPlace != null)
        {
            placer.SetSelectedObject(prefabToPlace);
        }
        else
        {
            Debug.LogWarning("尚未指定 placer 或 prefabToPlace！");
        }
    }
}
