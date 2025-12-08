using UnityEngine;

// 專門用於控制煙霧 UI 顯示/隱藏的腳本
// 掛載到煙霧 UI Image/Panel 物件上
public class SmokeToggle : MonoBehaviour
{
    private GameObject smokeUI;

    void Awake()
    {
        // 確保引用到掛載這個腳本的物件
        smokeUI = gameObject;
        // 確保一開始是關閉的
        smokeUI.SetActive(false);
    }

    /// <summary>
    /// 啟用煙霧效果 (30 秒開啟)
    /// </summary>
    public void EnableSmoke()
    {
        if (!smokeUI.activeSelf)
        {
            smokeUI.SetActive(true);
            Debug.Log(smokeUI.name + " 煙霧已開啟。");
        }
    }

    /// <summary>
    /// 關閉煙霧效果 (30 秒關閉)
    /// </summary>
    public void DisableSmoke()
    {
        if (smokeUI.activeSelf)
        {
            smokeUI.SetActive(false);
            Debug.Log(smokeUI.name + " 煙霧已關閉。");
        }
    }
}