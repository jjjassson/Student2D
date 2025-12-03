using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class ItemPair
{
    public GameObject mainPrefab;      // 主物件
    public GameObject secondaryPrefab; // 對應副物件（Z=0）
}

public class ItemSelector : MonoBehaviour
{
    [Header("物件清單")]
    public ItemPair[] items;

    [Header("UI 設置")]
    public Transform content;
    public GameObject buttonPrefab;

    [Header("控制器")]
    public ObjectPlacer placer;

    void Start()
    {
        if (placer == null)
            placer = FindObjectOfType<ObjectPlacer>();

        foreach (var item in items)
        {
            var btnObj = Instantiate(buttonPrefab, content);

            // 設定按鈕文字
            Text buttonText = btnObj.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = item.mainPrefab.name;

            // 確保按鈕存在
            Button buttonComponent = btnObj.GetComponent<Button>();
            if (buttonComponent == null)
            {
                Debug.LogError($"ItemSelector 錯誤：{buttonPrefab.name} 缺少 Button 元件！");
                continue;
            }

            buttonComponent.onClick.RemoveAllListeners();

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
                    // 手動指定主物件 + 對應副物件
                    placer.selectedObjectPrefab = item.mainPrefab;
                    placer.secondaryObjectPrefab = item.secondaryPrefab;

                    Debug.Log($"選擇主物件：{item.mainPrefab.name}" +
                              (item.secondaryPrefab != null ? $", 副物件：{item.secondaryPrefab.name}" : ", 無副物件"));
                }
            });

            trigger.triggers.Add(rightClickEntry);
        }
    }
}