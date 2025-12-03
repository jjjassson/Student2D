using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class ItemPair
{
    public GameObject mainPrefab;      // 主物件
    public GameObject secondaryPrefab; // 對應副物件（Z=0）
    public Sprite uiSprite;            // 新增：UI 上要顯示的 1:1 圖像
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

            // **修改：設定按鈕圖像 (Image)**
            Image buttonImage = btnObj.GetComponent<Image>(); // 假設 Image 元件在按鈕根物件上

            // 檢查 item.uiSprite 是否存在
            if (item.uiSprite != null)
            {
                if (buttonImage != null)
                {
                    buttonImage.sprite = item.uiSprite;
                    // 確保圖像能以 1:1 比例顯示
                    buttonImage.preserveAspect = true;

                    // 移除文字組件（如果有的話，以確保只顯示圖像）
                    Text buttonText = btnObj.GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        Destroy(buttonText.gameObject);
                    }
                }
                else
                {
                    Debug.LogWarning($"ItemSelector 警告：{buttonPrefab.name} 缺少 Image 元件，無法顯示圖像！將改用物件名稱。");
                    // 作為備用，如果沒有 Image，則顯示文字
                    Text buttonText = btnObj.GetComponentInChildren<Text>();
                    if (buttonText != null)
                        buttonText.text = item.mainPrefab.name;
                }
            }
            else
            {
                // 如果 uiSprite 為空，則顯示物件名稱作為備用
                Text buttonText = btnObj.GetComponentInChildren<Text>();
                if (buttonText != null)
                    buttonText.text = item.mainPrefab.name;
            }
            // ------------------------------------

            // 確保按鈕存在
            Button buttonComponent = btnObj.GetComponent<Button>();
            if (buttonComponent == null)
            {
                Debug.LogError($"ItemSelector 錯誤：{buttonPrefab.name} 缺少 Button 元件！");
                continue;
            }

            // 這裡不需要移除左鍵監聽器，因為你只處理右鍵點擊
            // buttonComponent.onClick.RemoveAllListeners(); 

            // **左鍵點擊（如果需要）**：設置為主物件和空副物件
            buttonComponent.onClick.AddListener(() =>
            {
                placer.selectedObjectPrefab = item.mainPrefab;
                placer.secondaryObjectPrefab = null; // 左鍵只選主物件

                Debug.Log($"選擇主物件：{item.mainPrefab.name}, 副物件：無 (左鍵)");
            });


            // **右鍵點擊（保留原邏輯）**：設置為主物件和副物件
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
                              (item.secondaryPrefab != null ? $", 副物件：{item.secondaryPrefab.name}" : ", 無副物件") + " (右鍵)");
                }
            });

            trigger.triggers.Add(rightClickEntry);
        }
    }
}