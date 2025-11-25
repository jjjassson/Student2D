using UnityEngine;
using UnityEngine.UI; // 必須引用，才能控制 Image
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("UI 元件")]
    [Tooltip("顯示倒數數字")]
    public TextMeshProUGUI timerText;

    [Tooltip("顯示狀態圖片 (例如：建造階段圖示)")]
    public Image statusImage;

    [Header("狀態圖片資源 (請將對應 Sprite 拉入此處)")]
    public Sprite buildPhaseSprite;   // 建造階段圖片 (例如：槌子圖示)
    public Sprite readyPhaseSprite;   // 準備階段圖片 (例如：Ready字樣或旗幟)
    public Sprite goSprite;           // 開始圖片 (例如：GO!字樣)
    public Sprite roundEndSprite;     // 結束圖片 (例如：Time's Up)

    private void Start()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.OnCountdownTick += UpdateTimerDisplay;
            RoundManager.Instance.OnPlacementStart += HandlePlacementStart;
            RoundManager.Instance.OnPlacementEnd += HandlePlacementEnd;
            RoundManager.Instance.OnRoundStart += HandleRoundStart;
            RoundManager.Instance.OnRoundEnd += HandleRoundEnd;
        }
    }

    private void OnDestroy()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.OnCountdownTick -= UpdateTimerDisplay;
            RoundManager.Instance.OnPlacementStart -= HandlePlacementStart;
            RoundManager.Instance.OnPlacementEnd -= HandlePlacementEnd;
            RoundManager.Instance.OnRoundStart -= HandleRoundStart;
            RoundManager.Instance.OnRoundEnd -= HandleRoundEnd;
        }
    }

    // ===== 事件處理邏輯 =====

    private void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText != null)
        {
            if (timeRemaining <= 0)
                timerText.text = "";
            else
                timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }

    // 1. 建造階段
    private void HandlePlacementStart(float totalTime)
    {
        UpdateStatusImage(buildPhaseSprite); // 切換圖片
        if (timerText != null) timerText.color = Color.yellow;
        ShowUI(true);
    }

    // 2. 準備階段 (倒數前)
    private void HandlePlacementEnd()
    {
        UpdateStatusImage(readyPhaseSprite); // 切換圖片
        if (timerText != null) timerText.color = Color.red;
    }

    // 3. 回合開始
    private void HandleRoundStart(int roundNum)
    {
        UpdateStatusImage(goSprite); // 切換圖片
        if (timerText != null) timerText.text = ""; // GO 階段通常不用數字

        // 1秒後隱藏所有 UI，讓畫面乾淨
        Invoke(nameof(HideAllUI), 1f);
    }

    // 4. 回合結束
    private void HandleRoundEnd(int roundNum)
    {
        ShowUI(true);
        UpdateStatusImage(roundEndSprite); // 切換圖片
    }

    // 🛠️ 小工具：切換圖片並確保不為空
    private void UpdateStatusImage(Sprite newSprite)
    {
        if (statusImage != null && newSprite != null)
        {
            statusImage.sprite = newSprite;
            statusImage.gameObject.SetActive(true); // 確保圖片是開啟的

            // 可選：如果你希望圖片保持原始比例
            statusImage.preserveAspect = true;
        }
        else if (statusImage != null)
        {
            // 如果沒給圖片，就隱藏 Image 物件以免顯示白色方塊
            statusImage.gameObject.SetActive(false);
        }
    }

    private void ShowUI(bool show)
    {
        if (timerText != null) timerText.gameObject.SetActive(show);
        if (statusImage != null) statusImage.gameObject.SetActive(show);
    }

    private void HideAllUI()
    {
        ShowUI(false);
    }
}