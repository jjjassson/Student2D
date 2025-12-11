using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic; // 確保有這個 using

[System.Serializable]
public class ItemPair
{
    public GameObject mainPrefab;       // 主物件
    public GameObject secondaryPrefab;  // 對應副物件（Z=0）
    public Sprite uiSprite;             // UI 上要顯示的 1:1 圖像
}

public class ItemSelector : MonoBehaviour
{
    [Header("物件清單")]
    public ItemPair[] items;

    [Header("UI 設置")]
    public Transform content;
    public GameObject buttonPrefab; // 這個 Prefab 現在應該帶有 UIScaleHover.cs

    [Header("控制器")]
    public ObjectPlacer placer;

    // **移除：** 與 ButtonAnimator 相關的屬性/列表
    // **移除：** 與 Smoothing Time 相關的屬性

    void Start()
    {
        if (placer == null)
            placer = FindObjectOfType<ObjectPlacer>();

        foreach (var item in items)
        {
            var btnObj = Instantiate(buttonPrefab, content);

            // --- 圖像/文字設定邏輯 (保持不變) ---
            Image buttonImage = btnObj.GetComponent<Image>();
            if (item.uiSprite != null && buttonImage != null)
            {
                buttonImage.sprite = item.uiSprite;
                buttonImage.preserveAspect = true;

                Text buttonText = btnObj.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    Destroy(buttonText.gameObject);
                }
            }
            else
            {
                Text buttonText = btnObj.GetComponentInChildren<Text>();
                if (buttonText != null)
                    buttonText.text = item.mainPrefab.name;
            }
            // ------------------------------------

            Button buttonComponent = btnObj.GetComponent<Button>();
            if (buttonComponent == null)
            {
                Debug.LogError($"ItemSelector 錯誤：{buttonPrefab.name} 缺少 Button 元件！");
                continue;
            }

            // 左鍵點擊：設置為主物件和空副物件
            buttonComponent.onClick.AddListener(() =>
            {
                placer.selectedObjectPrefab = item.mainPrefab;
                placer.secondaryObjectPrefab = null;
                Debug.Log($"選擇主物件：{item.mainPrefab.name}, 副物件：無 (左鍵)");
            });


            // 設置 EventTrigger 處理右鍵點擊
            EventTrigger trigger = btnObj.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = btnObj.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry rightClickEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            rightClickEntry.callback.AddListener((data) =>
            {
                PointerEventData ped = (PointerEventData)data;
                if (ped.button == PointerEventData.InputButton.Right)
                {
                    placer.selectedObjectPrefab = item.mainPrefab;
                    placer.secondaryObjectPrefab = item.secondaryPrefab;

                    Debug.Log($"選擇主物件：{item.mainPrefab.name}" +
                              (item.secondaryPrefab != null ? $", 副物件：{item.secondaryPrefab.name}" : ", 無副物件") + " (右鍵)");
                }
            });
            trigger.triggers.Add(rightClickEntry);

            // **移除：** PointerEnter 和 PointerExit 的 EventTrigger 註冊，
            // **因為 UIScaleHover.cs 已經使用內建接口 IPointerEnterHandler/IPointerExitHandler 處理了。**
        }
    }
}