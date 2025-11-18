using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("回合設定")]
    public float intermissionDelay = 3f;     // 回合結束後等待時間 (顯示結果等)
    public float startCountdownTime = 3f;  // 回合開始前的倒數計時時間

    private int roundNumber = 0;
    private bool isRoundActive = false;
    // 儲存所有已生成且帶有 PlayerScore 腳本的玩家實體
    private List<PlayerScore> activePlayers = new List<PlayerScore>();

    // 可以在 UI 上顯示的回合狀態事件 (可選)
    public event System.Action<int> OnRoundStart;
    public event System.Action<int> OnRoundEnd;
    public event System.Action<float> OnCountdownTick;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 假設 RoundManager 是跨場景持續存在的單例
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 確保遊戲開始時啟動回合流程
        // (此方法應在 InitializeLevel.cs 設置完玩家後被呼叫)
        StartGame();
    }

    public void StartGame()
    {
        // 找到所有玩家（假設它們已經被 InitializeLevel.cs 生成）
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        activePlayers = playerObjects.Select(p => p.GetComponent<PlayerScore>())
                                     .Where(ps => ps != null).ToList();

        if (activePlayers.Count > 0)
        {
            // 啟動第一回合的序列 (包含倒數計時)
            StartCoroutine(StartRoundSequence());
        }
        else
        {
            Debug.LogError("場景中找不到任何帶有 PlayerScore 的 Player 物件！請確認 Player Prefab 上有 'PlayerScore' 腳本並有正確 Tag。");
        }
    }

    // 🔴 負責處理「回合開始前倒數計時」的協程
    private IEnumerator StartRoundSequence()
    {
        roundNumber++;
        isRoundActive = false;
        Debug.Log($"=== Round {roundNumber} 準備開始！ ===");

        // 步驟 1: 執行倒數計時
        float timer = startCountdownTime;
        while (timer > 0)
        {
            int displayTime = Mathf.CeilToInt(timer);
            OnCountdownTick?.Invoke(displayTime);
            Debug.Log($"倒數: {displayTime}...");
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }
        OnCountdownTick?.Invoke(0); // 倒數結束

        // 步驟 2: 正式開始回合
        StartRound();
    }

    // 這是實際開始回合，並讓玩家復活的方法 (Round Start Logic)
    public void StartRound()
    {
        Debug.Log($"=== Round {roundNumber} 正式開始！ ===");

        // 💡 滿足「只清理角色」需求：只處理玩家重置
        foreach (var player in activePlayers)
        {
            // 呼叫玩家的 Revive() 方法，它會將玩家傳送到出生點並啟用碰撞體
            // (假設 PlayerScore 內有 ReviveSequence 協程確保安全重生)
            player.Revive();
        }

        isRoundActive = true;
        OnRoundStart?.Invoke(roundNumber);
    }

    public void EndRound()
    {
        if (!isRoundActive) return;

        isRoundActive = false;
        Debug.Log($"=== Round {roundNumber} 結束！ ===");
        OnRoundEnd?.Invoke(roundNumber);

        // 步驟 3: 進入回合間隔，然後啟動下一回合序列
        StartCoroutine(IntermissionSequence());
    }

    // 進入回合間隔時間的協程
    private IEnumerator IntermissionSequence()
    {
        Debug.Log($"回合間隔中，等待 {intermissionDelay} 秒...");
        // 等待間隔時間，讓玩家看到結果
        yield return new WaitForSeconds(intermissionDelay);

        // 啟動下一回合倒數
        StartCoroutine(StartRoundSequence());
    }


    public void NotifyPlayerDeath(PlayerScore player)
    {
        if (!isRoundActive) return;

        // 確保玩家已經被正確標記為死亡，且不是重複呼叫
        // 由於我們在 PlayerCollision 中檢查了 isAlive，這裡只需判斷全部是否死亡

        // 檢查是否所有玩家都死亡
        bool allDead = activePlayers.All(p => !p.isAlive);

        if (allDead)
        {
            Debug.Log("所有玩家死亡，回合結束。");
            EndRound();
        }
    }

    public void NotifyPlayerReachedGoal(PlayerScore player)
    {
        if (!isRoundActive) return;

        // 這裡您可以加入您的勝利條件判斷 (例如：所有人都抵達終點)

        EndRound();
    }
}