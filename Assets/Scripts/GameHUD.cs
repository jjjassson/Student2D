using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("UI 元件")]
    [Tooltip("顯示倒數數字")]
    public TextMeshProUGUI timerText;

    [Tooltip("顯示狀態圖片 (例如：建造階段圖示)")]
    public Image statusImage;

    [Header("狀態圖片資源 (請將對應 Sprite 拉入此處)")]
    public Sprite buildPhaseSprite;      // 建造階段圖片 (槌子)
    public Sprite cooldownPhaseSprite;   // 新增：冷卻/休息階段圖片 (例如：沙漏或暫停圖示)
    public Sprite goSprite;             // 回合開始圖片 (GO!)
    public Sprite roundEndSprite;        // 結束圖片 (Time's Up)

    private RoundManager roundManager;

    private void Start()
    {
        roundManager = RoundManager.Instance;
        if (roundManager != null)
        {
            // 訂閱事件
            roundManager.OnCountdownTick += UpdateTimerDisplay;
            roundManager.OnPlacementStart += HandlePlacementStart;
            roundManager.OnPlacementEnd += HandlePlacementEnd;
            roundManager.OnRoundStart += HandleRoundStart;
            roundManager.OnRoundEnd += HandleRoundEnd;
        }
    }

    private void OnDestroy()
    {
        if (roundManager != null)
        {
            // 取消訂閱
            roundManager.OnCountdownTick -= UpdateTimerDisplay;
            roundManager.OnPlacementStart -= HandlePlacementStart;
            roundManager.OnPlacementEnd -= HandlePlacementEnd;
            roundManager.OnRoundStart -= HandleRoundStart;
            roundManager.OnRoundEnd -= HandleRoundEnd;
        }
    }

    // ===== 事件處理邏輯 =====

    private void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText != null)
        {
            // 處理負數或極小數值，避免顯示 -1 或 -0
            if (timeRemaining <= 0)
                timerText.text = "";
            else
                timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }

    // 1. 建造階段
    private void HandlePlacementStart(float totalTime)
    {
        // 如果有正在運行的 Invoke，先取消，避免在錯誤時間切換狀態
        CancelInvoke();
        UpdateStatusImage(buildPhaseSprite); // 切換到建造圖示
        if (timerText != null) timerText.color = Color.yellow;
        ShowUI(true);
    }

    // 2. 放置階段結束
    private void HandlePlacementEnd()
    {
        // 由於多人模式下，放置結束會接著 interPlacementDelay 冷卻，
        // 我們讓 UI 顯示冷卻狀態，並在冷卻結束時讓狀態圖示消失 (等待下一輪放置或死亡)

        UpdateStatusImage(cooldownPhaseSprite); // 切換到冷卻圖示
        if (timerText != null) timerText.text = ""; // 冷卻期間不顯示數字

        // 1秒後隱藏所有 UI，讓畫面乾淨 (因為冷卻完之後就是遊玩階段)
        // 使用 Invoke + delay 確保它不會在 RoundStart 之前就被覆蓋
        Invoke(nameof(HideAllUI), roundManager.interPlacementDelay);
    }

    // 3. 回合開始
    private void HandleRoundStart(int roundNum)
    {
        // 清除 HandlePlacementEnd 可能設定的 Invoke
        CancelInvoke();

        UpdateStatusImage(goSprite); // 切換到 GO! 圖示
        if (timerText != null) timerText.text = ""; // GO 階段通常不用數字

        // 立即或短暫延遲後隱藏 UI，讓玩家可以專心遊玩
        Invoke(nameof(HideAllUI), 1f);
    }

    // 4. 回合結束
    private void HandleRoundEnd(int roundNum)
    {
        CancelInvoke(); // 清除所有延遲操作
        ShowUI(true);
        UpdateStatusImage(roundEndSprite); // 切換到結束圖示
    }

    // 🛠️ 小工具：切換圖片並確保不為空
    private void UpdateStatusImage(Sprite newSprite)
    {
        if (statusImage != null)
        {
            if (newSprite != null)
            {
                statusImage.sprite = newSprite;
                statusImage.gameObject.SetActive(true); // 確保圖片是開啟的
                statusImage.preserveAspect = true;
            }
            else
            {
                // 如果沒有提供圖片，則隱藏 Image 物件
                statusImage.gameObject.SetActive(false);
            }
        }
    }

    private void ShowUI(bool show)
    {
        if (timerText != null) timerText.gameObject.SetActive(show);
        // statusImage 的 active 狀態由 UpdateStatusImage 處理
        if (show == false)
        {
            if (statusImage != null) statusImage.gameObject.SetActive(false);
        }
    }

    private void HideAllUI()
    {
        ShowUI(false);
    }
}