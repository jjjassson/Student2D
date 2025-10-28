using UnityEngine;
using UnityEngine.UI;

public class ItemSelector : MonoBehaviour
{
    [Header("物件清單")]
    public GameObject[] items;

    [Header("UI 設置")]
    public Transform content;
    public GameObject buttonPrefab;

    // 💡 關鍵連線：引用場景中的 ObjectPlacer
    [Header("控制器")]
    public ObjectPlacer placer;

    void Start()
    {
        // 檢查 Placer 是否已連線
        if (placer == null)
        {
            // 嘗試自動尋找場景中的 ObjectPlacer 實例 (如果 Inspector 沒拖入的話)
            placer = FindObjectOfType<ObjectPlacer>();
            if (placer == null)
            {
                Debug.LogError("ItemSelector 錯誤：請在 Inspector 中或場景中提供 ObjectPlacer 腳本！");
                return;
            }
        }

        // 動態生成按鈕
        foreach (var item in items)
        {
            GameObject currentItem = item;

            // 實例化按鈕 Prefab
            var btn = Instantiate(buttonPrefab, content);

            // 設定按鈕文字 (假設按鈕的子物件有 Text 元件)
            Text buttonText = btn.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = currentItem.name;
            }

            // 取得 Button 元件並新增點擊監聽器
            Button buttonComponent = btn.GetComponent<Button>();
            if (buttonComponent != null)
            {
                // 當按鈕被點擊時，呼叫 ObjectPlacer 中的 SelectObjectFromButton 方法
                // 注意：這裡使用左鍵點擊來選擇物件
                buttonComponent.onClick.AddListener(() => placer.SelectObjectFromButton(currentItem));
            }
            else
            {
                Debug.LogError($"ItemSelector 錯誤：生成的 Button Prefab {buttonPrefab.name} 缺少 Button 元件！");
            }
        }
    }
}