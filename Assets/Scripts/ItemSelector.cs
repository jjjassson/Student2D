using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSelector : MonoBehaviour
{
    [Header("物件清單")]
    public GameObject[] items;

    [Header("UI 設置")]
    public Transform content;
    public GameObject buttonPrefab;

    [Header("控制器")]
    public ObjectPlacer placer;

    void Start()
    {
        // 自動尋找 ObjectPlacer
        if (placer == null)
        {
            placer = FindObjectOfType<ObjectPlacer>();
            if (placer == null)
            {
                Debug.LogError("ItemSelector 錯誤：請在 Inspector 中或場景中提供 ObjectPlacer 腳本！");
                return;
            }
        }

        // 為每個物件生成按鈕
        foreach (var item in items)
        {
            GameObject currentItem = item;

            var btnObj = Instantiate(buttonPrefab, content);

            // 設定按鈕文字
            Text buttonText = btnObj.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = currentItem.name;

            // 確保按鈕存在
            Button buttonComponent = btnObj.GetComponent<Button>();
            if (buttonComponent == null)
            {
                Debug.LogError($"ItemSelector 錯誤：{buttonPrefab.name} 缺少 Button 元件！");
                continue;
            }

            // 移除預設的左鍵事件（可選）
            buttonComponent.onClick.RemoveAllListeners();

            // 使用 EventTrigger 監聽右鍵
            EventTrigger trigger = btnObj.AddComponent<EventTrigger>();

            EventTrigger.Entry rightClickEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            rightClickEntry.callback.AddListener((data) =>
            {
                PointerEventData ped = (PointerEventData)data;
                if (ped.button == PointerEventData.InputButton.Right)
                {
                    placer.SelectObjectFromButton(currentItem);
                    Debug.Log($"右鍵選擇物件：{currentItem.name}");
                }
            });

            trigger.triggers.Add(rightClickEntry);
        }
    }
}