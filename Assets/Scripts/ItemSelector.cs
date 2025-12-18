using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    public GameObject buttonPrefab; // 帶有 UIScaleHover.cs 的按鈕預製物

    [Header("控制器參考")]
    public ObjectPlacer placer;
    public PlayerCursorController playerCursor; // 拖入畫面上的 PlayerCursor 物件

    void Start()
    {
        // 自動防錯：如果沒拖入則嘗試尋找
        if (placer == null)
            placer = FindObjectOfType<ObjectPlacer>();

        if (playerCursor == null)
            playerCursor = FindObjectOfType<PlayerCursorController>();

        foreach (var item in items)
        {
            // 生成按鈕
            var btnObj = Instantiate(buttonPrefab, content);

            // --- 處理按鈕圖案與文字顯示 ---
            Image buttonImage = btnObj.GetComponent<Image>();
            if (item.uiSprite != null && buttonImage != null)
            {
                buttonImage.sprite = item.uiSprite;
                buttonImage.preserveAspect = true;

                // 如果有 Sprite，通常不需要顯示文字，刪除子物件中的 Text
                Text buttonText = btnObj.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    Destroy(buttonText.gameObject);
                }
            }
            else
            {
                // 沒有圖片時顯示 Prefab 名稱
                Text buttonText = btnObj.GetComponentInChildren<Text>();
                if (buttonText != null)
                    buttonText.text = item.mainPrefab.name;
            }

            // --- 設置按鈕組件 ---
            Button buttonComponent = btnObj.GetComponent<Button>();
            if (buttonComponent == null)
            {
                Debug.LogError($"ItemSelector 錯誤：{buttonPrefab.name} 缺少 Button 元件！");
                continue;
            }

            // --- [左鍵點擊邏輯] ---
            buttonComponent.onClick.AddListener(() =>
            {
                // 1. 設定要放置的物件
                placer.selectedObjectPrefab = item.mainPrefab;
                placer.secondaryObjectPrefab = null;

                // 2. 更新游標圖案
                if (playerCursor != null)
                {
                    playerCursor.ChangeCursorSprite(item.uiSprite);
                }

                Debug.Log($"選擇主物件：{item.mainPrefab.name}, 副物件：無 (左鍵)");
            });

            // --- [右鍵點擊邏輯 (使用 EventTrigger)] ---
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
                    // 1. 設定要放置的物件 (含副物件)
                    placer.selectedObjectPrefab = item.mainPrefab;
                    placer.secondaryObjectPrefab = item.secondaryPrefab;

                    // 2. 更新游標圖案
                    if (playerCursor != null)
                    {
                        playerCursor.ChangeCursorSprite(item.uiSprite);
                    }

                    Debug.Log($"選擇主物件：{item.mainPrefab.name}" +
                              (item.secondaryPrefab != null ? $", 副物件：{item.secondaryPrefab.name}" : ", 無副物件") + " (右鍵)");
                }
            });
            trigger.triggers.Add(rightClickEntry);
        }
    }
}