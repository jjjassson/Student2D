using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PrefabSelectorButton : MonoBehaviour, IPointerClickHandler
{
    public ObjectPlacer placer;          // 拖入 ObjectPlacer
    public GameObject prefabToPlace;     // 這個按鈕代表的物件

    // 滑鼠點擊事件
    public void OnPointerClick(PointerEventData eventData)
    {
        if (placer == null || prefabToPlace == null)
        {
            Debug.LogWarning("尚未指定 placer 或 prefabToPlace！");
            return;
        }

        // 🖱️ 右鍵選擇物件
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            placer.SelectObjectFromButton(prefabToPlace);
        }
    }
}
