using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("階段時間設定")]
    [Tooltip("單次放置階段的持續時間 (例如 10 秒)")]
    public float placementTimePerPlayer = 10f;

    [Tooltip("回合開始後的總遊玩時間限制 (設為 0 表示無時間限制，直到全員死亡)")]
    public float roundDuration = 0f;

    [Tooltip("回合結束後等待時間")]
    public float intermissionDelay = 3f;

    [Tooltip("單次放置階段之間的冷卻休息時間 (僅在多輪放置時啟用)")]
    public float interPlacementDelay = 5f;

    private int roundNumber = 0;
    private int placementPhaseCount = 0; // 追蹤本回合內放置階段的次數
    private bool isRoundActive = false; // 保持 private
    private bool isPlacementPhase = false; // 標記是否處於放置時間內
    private List<PlayerScore> activePlayers = new List<PlayerScore>();

    // ✅ 新增 Public Property 讓外部可以讀取 IsRoundActive 狀態 (解決 CS0122 錯誤)
    public bool IsRoundActive
    {
        get { return isRoundActive; }
    }

    // 📢 事件：通知 UI 或管理器目前是什麼階段
    public event System.Action<float> OnPlacementStart; // 參數傳入單次放置時間
    public event System.Action OnPlacementEnd;
    public event System.Action<int> OnRoundStart;
    public event System.Action<int> OnRoundEnd;
    public event System.Action<float> OnCountdownTick; // 通用倒數顯示
    public event System.Action<bool> OnPlacementAllowedChange; // 控制 ObjectPlacer 放置權限

    private Coroutine roundCycleCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        // 假設 PlayerScore 腳本附加在 Tag 為 "Player" 的物件上
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        activePlayers = playerObjects.Select(p => p.GetComponent<PlayerScore>())
                                     .Where(ps => ps != null).ToList();

        if (activePlayers.Count > 0)
        {
            StartRound(); // 遊戲從第一回合開始
        }
        else
        {
            Debug.LogError("找不到 PlayerScore！請確認 Tag 與 Script。");
        }
    }

    // 🔄 主要回合循環控制 (放置 -> 遊玩/死亡檢查 -> 下一輪放置 或 回合結束)
    private IEnumerator RoundCycleSequence()
    {
        roundNumber++;
        placementPhaseCount = 0; // 重置放置次數
        isRoundActive = true;
        Debug.Log($"=== Round {roundNumber} 開始 (玩家已可自由行動) ===");

        // 1️⃣ 回合開始 (玩家復活、可移動)
        foreach (var player in activePlayers)
        {
            // 假設 PlayerScore 有 Revive 方法
            if (player != null) player.Revive();
        }
        OnRoundStart?.Invoke(roundNumber);

        // 2️⃣ 進入放置/遊玩循環
        while (isRoundActive)
        {
            placementPhaseCount++;

            // ------------------------------------------------------------
            // 放置階段 (Placement Phase)
            // ------------------------------------------------------------
            yield return StartCoroutine(HandlePlacementPhase(placementPhaseCount));

            // ------------------------------------------------------------
            // 死亡檢查與退出條件
            // ------------------------------------------------------------
            bool allDead = activePlayers.All(p => p != null && !p.isAlive);
            if (allDead)
            {
                // 所有人死亡，退出本回合循環，將在 EndRound 中處理下一回合
                isRoundActive = false;
                break;
            }
            else if (activePlayers.Count == 1)
            {
                // 單人模式，放置一次後即退出放置循環 (等待玩家死亡)
                Debug.Log("單人模式：放置階段結束，等待玩家死亡。");
                break;
            }

            // ------------------------------------------------------------
            // 多人模式：放置間隔冷卻 (Inter-Placement Delay)
            // ------------------------------------------------------------
            Debug.Log($"放置間隔冷卻 {interPlacementDelay} 秒...");
            OnCountdownTick?.Invoke(0); // 清空倒數顯示

            // 禁用放置 (確保 ObjectPlacer 無法操作)
            OnPlacementAllowedChange?.Invoke(false);
            yield return new WaitForSeconds(interPlacementDelay);

            // 如果有多人，且沒有全死，則繼續下一輪放置
        }

        // 退出放置循環後，如果 Round Active 仍為 True (單人模式，放置完畢但未死亡)
        if (isRoundActive)
        {
            // 等待直到所有玩家死亡
            yield return new WaitUntil(() => activePlayers.All(p => p != null && !p.isAlive) || !isRoundActive);
        }

        // 3️⃣ 回合結束處理
        EndRound();
    }

    // 放置階段的細節邏輯
    private IEnumerator HandlePlacementPhase(int phaseNum)
    {
        isPlacementPhase = true;
        Debug.Log($"進入放置階段 (第 {phaseNum} 輪)：時間 {placementTimePerPlayer} 秒");

        OnPlacementAllowedChange?.Invoke(true); // 啟用放置並重置 ObjectPlacer 的放置狀態
        OnPlacementStart?.Invoke(placementTimePerPlayer);

        float pTimer = placementTimePerPlayer;
        while (pTimer > 0 && isPlacementPhase && isRoundActive)
        {
            // 倒數計時
            OnCountdownTick?.Invoke(pTimer);
            pTimer -= Time.deltaTime;
            yield return null;
        }

        isPlacementPhase = false;
        OnPlacementEnd?.Invoke(); // 通知外部 UI 關閉
        OnPlacementAllowedChange?.Invoke(false); // 禁用放置
        Debug.Log($"第 {phaseNum} 輪放置階段結束！");
    }

    public void StartRound()
    {
        if (roundCycleCoroutine != null) StopCoroutine(roundCycleCoroutine);
        roundCycleCoroutine = StartCoroutine(RoundCycleSequence());
    }

    public void EndRound()
    {
        if (!isRoundActive) return;

        isRoundActive = false;
        if (roundCycleCoroutine != null) StopCoroutine(roundCycleCoroutine);

        Debug.Log($"=== Round {roundNumber} 結束！ ===");
        OnRoundEnd?.Invoke(roundNumber);

        StartCoroutine(IntermissionSequence());
    }

    private IEnumerator IntermissionSequence()
    {
        Debug.Log($"休息 {intermissionDelay} 秒，準備回到 Round {roundNumber + 1} 的玩家生成位置...");

        yield return new WaitForSeconds(intermissionDelay);

        // 進入新的回合流程 
        StartRound();
    }

    public void NotifyPlayerDeath(PlayerScore player)
    {
        if (!isRoundActive) return;

        bool allDead = activePlayers.All(p => p != null && !p.isAlive);
        if (allDead)
        {
            EndRound(); // 觸發 EndRound 進入 IntermissionSequence
        }
    }

    public void NotifyPlayerReachedGoal(PlayerScore player)
    {
        if (!isRoundActive) return;
        // 依據您的規則，如果有人到達終點也結束回合
        EndRound();
    }
}