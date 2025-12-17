using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("階段時間設定")]
    public float placementTimePerPlayer = 10f;
    public float roundDuration = 30f;
    public float intermissionDelay = 3f;
    public float interPlacementDelay = 5f;

    private int roundNumber = 0;
    private int placementPhaseCount = 0;
    private bool isRoundActive = false;
    private bool isPlacementPhase = false;
    private List<PlayerScore> activePlayers = new List<PlayerScore>();

    public bool IsRoundActive => isRoundActive;

    // 事件系統
    public event System.Action<float> OnPlacementStart;
    public event System.Action OnPlacementEnd;
    public event System.Action<int> OnRoundStart;
    public event System.Action<int> OnRoundEnd;
    public event System.Action<float> OnCountdownTick;
    public event System.Action<bool> OnPlacementAllowedChange;

    private Coroutine roundCycleCoroutine;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        // 重新抓取場景上的玩家（確保有人加入）
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        activePlayers = playerObjects.Select(p => p.GetComponent<PlayerScore>())
                                     .Where(ps => ps != null).ToList();

        if (activePlayers.Count > 0) StartRound();
        else Debug.LogError("找不到 PlayerScore！請確認 Tag 與 Script。");
    }

    // 🔄 核心回合邏輯
    private IEnumerator RoundCycleSequence()
    {
        roundNumber++;
        placementPhaseCount = 0;
        isRoundActive = true;

        Debug.Log($"=== Round {roundNumber} 開始 ===");

        // 1️⃣ 回合開始：復活所有玩家
        foreach (var player in activePlayers)
        {
            if (player != null) player.Revive();
        }
        OnRoundStart?.Invoke(roundNumber);

        // 2️⃣ 進入放置循環 (Placement Loop)
        while (isRoundActive)
        {
            placementPhaseCount++;

            // --- 執行放置階段 ---
            yield return StartCoroutine(HandlePlacementPhase(placementPhaseCount));

            // --- 檢查點 A：放置階段是否全滅？ ---
            if (CheckIfAllDead())
            {
                Debug.Log("💀 放置階段全員死亡，跳過生存階段，直接結算");
                break; // 跳出 while 迴圈，直接前往 EndRound
            }

            // --- 單人模式檢查 ---
            if (activePlayers.Count == 1)
            {
                Debug.Log("單人模式：放置結束，進入生存挑戰！");
                break; // 跳出 while，進入下方的生存計時
            }

            // --- 放置間隔休息 ---
            Debug.Log($"放置間隔冷卻 {interPlacementDelay} 秒...");
            OnCountdownTick?.Invoke(0);
            OnPlacementAllowedChange?.Invoke(false);
            yield return new WaitForSeconds(interPlacementDelay);
        }

        // 3️⃣ 生存階段計時 (Survival Phase)
        // 只有在「還有活人」且「回合仍激活」時才執行
        if (isRoundActive && !CheckIfAllDead())
        {
            float timer = 0f;
            Debug.Log($"生存計時開始：限時 {roundDuration} 秒");

            while (isRoundActive)
            {
                // 檢查點 B：生存期間是否全滅？
                if (CheckIfAllDead())
                {
                    Debug.Log("💀 生存階段全員死亡，回合提早結束");
                    break;
                }

                if (roundDuration > 0)
                {
                    timer += Time.deltaTime;
                    float timeLeft = Mathf.Max(0, roundDuration - timer);
                    OnCountdownTick?.Invoke(timeLeft);

                    if (timer >= roundDuration)
                    {
                        Debug.Log("⏰ 時間到！強制結束回合");
                        break;
                    }
                }
                yield return null;
            }
        }

        // 4️⃣ 觸發回合結束
        // 無論是放置死、生存死、還是時間到，統一在這裡處理
        EndRound();
    }

    private bool CheckIfAllDead()
    {
        return activePlayers.All(p => p != null && !p.isAlive);
    }

    private IEnumerator HandlePlacementPhase(int phaseNum)
    {
        isPlacementPhase = true;
        Debug.Log($"進入放置階段：{placementTimePerPlayer} 秒");

        OnPlacementAllowedChange?.Invoke(true);
        OnPlacementStart?.Invoke(placementTimePerPlayer);

        float pTimer = placementTimePerPlayer;

        // 🔥 修正重點：在放置計時中，每一幀都檢查是否全滅
        while (pTimer > 0 && isPlacementPhase && isRoundActive)
        {
            // 如果這時候全滅，不需要等到時間跑完，直接中斷
            if (CheckIfAllDead())
            {
                Debug.Log("放置階段偵測到全滅，提早結束倒數");
                break;
            }

            OnCountdownTick?.Invoke(pTimer);
            pTimer -= Time.deltaTime;
            yield return null;
        }

        isPlacementPhase = false;
        OnPlacementEnd?.Invoke();
        OnPlacementAllowedChange?.Invoke(false);
    }

    public void StartRound()
    {
        if (roundCycleCoroutine != null) StopCoroutine(roundCycleCoroutine);
        roundCycleCoroutine = StartCoroutine(RoundCycleSequence());
    }

    public void EndRound()
    {
        // 這裡的邏輯是正確的：只有 active 的回合才需要執行結束流程
        if (!isRoundActive) return;

        isRoundActive = false; // 標記回合結束

        Debug.Log($"=== Round {roundNumber} 結束！準備重置 ===");
        OnRoundEnd?.Invoke(roundNumber);

        // 停止目前的循環，準備進入休息後重啟
        if (roundCycleCoroutine != null) StopCoroutine(roundCycleCoroutine);

        StartCoroutine(IntermissionSequence());
    }

    private IEnumerator IntermissionSequence()
    {
        OnCountdownTick?.Invoke(0);
        Debug.Log($"休息 {intermissionDelay} 秒...");
        yield return new WaitForSeconds(intermissionDelay);
        StartRound(); // 重新開始下一回合
    }

    public void NotifyPlayerDeath(PlayerScore player)
    {
        // 這裡雖然目前是空的，但透過上面的 CheckIfAllDead 輪詢已經足夠處理
        // 如果想要更優化效能，可以在這裡檢查 CheckIfAllDead()
        // 但目前的寫法已經很穩健
    }

    public void NotifyPlayerReachedGoal(PlayerScore player)
    {
        if (isRoundActive) EndRound();
    }
}