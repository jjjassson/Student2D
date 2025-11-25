using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("階段時間設定")]
    [Tooltip("每個玩家提供的放置時間 (例如 4 人 x 5 秒 = 20 秒)")]
    public float placementTimePerPlayer = 5f;
    [Tooltip("回合正式開始後的遊玩時間限制 (例如 30 秒)")]
    public float roundDuration = 30f;
    [Tooltip("回合結束後等待時間")]
    public float intermissionDelay = 3f;
    [Tooltip("放置結束後，開始衝刺前的倒數時間")]
    public float startCountdownTime = 3f;

    private int roundNumber = 0;
    private bool isRoundActive = false;
    private List<PlayerScore> activePlayers = new List<PlayerScore>();

    // 📢 新增事件：通知 UI 或管理器目前是什麼階段
    public event System.Action<float> OnPlacementStart; // 參數傳入總放置時間
    public event System.Action OnPlacementEnd;
    public event System.Action<int> OnRoundStart;
    public event System.Action<int> OnRoundEnd;
    public event System.Action<float> OnCountdownTick; // 通用倒數顯示 (給放置倒數或開始倒數用)

    private Coroutine roundTimerCoroutine;

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
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        activePlayers = playerObjects.Select(p => p.GetComponent<PlayerScore>())
                                     .Where(ps => ps != null).ToList();

        if (activePlayers.Count > 0)
        {
            StartCoroutine(GameLoopSequence());
        }
        else
        {
            Debug.LogError("找不到 PlayerScore！請確認 Tag 與 Script。");
        }
    }

    // 🔄 主要遊戲流程控制 (放置 -> 倒數 -> 遊玩)
    private IEnumerator GameLoopSequence()
    {
        roundNumber++;
        isRoundActive = false;
        Debug.Log($"=== Round {roundNumber} 準備流程開始 ===");

        // ------------------------------------------------------------
        // 1️⃣ 放置階段 (Placement Phase)
        // ------------------------------------------------------------
        // 計算總時間：玩家人數 * 每人時間
        float totalPlacementTime = activePlayers.Count * placementTimePerPlayer;
        Debug.Log($"進入放置階段：共 {activePlayers.Count} 人，時間 {totalPlacementTime} 秒");

        OnPlacementStart?.Invoke(totalPlacementTime); // 通知外部顯示放置UI

        // 執行放置階段的倒數
        float pTimer = totalPlacementTime;
        while (pTimer > 0)
        {
            OnCountdownTick?.Invoke(pTimer); // 更新 UI 倒數
            yield return new WaitForSeconds(1f);
            pTimer -= 1f;
        }

        OnPlacementEnd?.Invoke(); // 通知外部關閉放置 UI / 禁止放置
        Debug.Log("放置階段結束！");

        // ------------------------------------------------------------
        // 2️⃣ 準備衝刺倒數 (Ready Set Go)
        // ------------------------------------------------------------
        float cTimer = startCountdownTime;
        while (cTimer > 0)
        {
            OnCountdownTick?.Invoke(cTimer);
            yield return new WaitForSeconds(1f);
            cTimer -= 1f;
        }
        OnCountdownTick?.Invoke(0);

        // ------------------------------------------------------------
        // 3️⃣ 正式開始回合 (Gameplay Phase)
        // ------------------------------------------------------------
        StartRound();
    }

    public void StartRound()
    {
        Debug.Log($"=== Round {roundNumber} 正式開始！限時 {roundDuration} 秒 ===");

        foreach (var player in activePlayers)
        {
            player.Revive();
        }

        isRoundActive = true;
        OnRoundStart?.Invoke(roundNumber);

        // ⏰ 啟動回合限時計時器
        if (roundTimerCoroutine != null) StopCoroutine(roundTimerCoroutine);
        roundTimerCoroutine = StartCoroutine(RoundTimer());
    }

    // ⏳ 回合限時邏輯
    private IEnumerator RoundTimer()
    {
        float timer = roundDuration;
        while (timer > 0 && isRoundActive)
        {
            // 這裡可以選擇是否要傳送剩餘時間給 UI
            // OnCountdownTick?.Invoke(timer); 
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        // 如果時間到了回合還在進行中，強制結束
        if (isRoundActive)
        {
            Debug.Log("時間到！回合強制結束。");
            EndRound();
        }
    }

    public void EndRound()
    {
        if (!isRoundActive) return;

        isRoundActive = false;
        if (roundTimerCoroutine != null) StopCoroutine(roundTimerCoroutine);

        Debug.Log($"=== Round {roundNumber} 結束！ ===");
        OnRoundEnd?.Invoke(roundNumber);

        StartCoroutine(IntermissionSequence());
    }

    private IEnumerator IntermissionSequence()
    {
        Debug.Log($"休息 {intermissionDelay} 秒...");
        yield return new WaitForSeconds(intermissionDelay);

        // 重新開始新的回合流程 (回到放置階段)
        StartCoroutine(GameLoopSequence());
    }

    public void NotifyPlayerDeath(PlayerScore player)
    {
        if (!isRoundActive) return;

        bool allDead = activePlayers.All(p => !p.isAlive);
        if (allDead)
        {
            EndRound();
        }
    }

    public void NotifyPlayerReachedGoal(PlayerScore player)
    {
        if (!isRoundActive) return;
        // 這裡看你的規則，如果是有人到終點就結束，或全部人都到才結束
        EndRound();
    }
}