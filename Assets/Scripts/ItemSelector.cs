using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class ItemPair
{
    public GameObject mainPrefab;      // 主物件
    public GameObject secondaryPrefab; // 對應副物件（Z=0）
    public Sprite itemIcon;            // **[新增]：用於 UI 按鈕的圖片/图标**
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

            // --- [新增] 設定按鈕圖片 ---
            // 尝试获取按钮本身或其子物体中的 Image 组件，用于显示图标。
            // 如果您的图标 Image 是 Text 之外的第一个 Image 子物体，这通常是可行的。
            Image buttonIcon = btnObj.GetComponentInChildren<Image>();

            // 确保我们找到了 Image 组件，并且在 ItemPair 中有设置图片
            if (buttonIcon != null && item.itemIcon != null)
            {
                buttonIcon.sprite = item.itemIcon;
                // 确保图片不被按钮默认颜色覆盖
                buttonIcon.color = Color.white;
            }
            else if (buttonIcon == null)
            {
                Debug.LogWarning($"ItemSelector 警告：按钮 {buttonPrefab.name} 缺少 Image 组件！无法显示图片。");
            }
            else if (item.itemIcon == null)
            {
                Debug.LogWarning($"物品 {item.mainPrefab.name} 没有设置图标 (itemIcon)！");
            }

            // --- [保留] 設定按鈕文字 ---
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