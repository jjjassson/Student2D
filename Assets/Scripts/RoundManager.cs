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

    [Tooltip("遊玩階段的時間限制 (例如 30 秒)。設為 0 表示無時間限制。")]
    public float roundDuration = 30f;

    [Tooltip("回合結束後等待時間 (顯示結算或等待重生的時間)")]
    public float intermissionDelay = 3f;

    [Tooltip("單次放置階段之間的冷卻休息時間 (僅在多輪放置時啟用)")]
    public float interPlacementDelay = 5f;

    private int roundNumber = 0;
    private int placementPhaseCount = 0;
    private bool isRoundActive = false;
    private bool isPlacementPhase = false;
    private List<PlayerScore> activePlayers = new List<PlayerScore>();

    public bool IsRoundActive
    {
        get { return isRoundActive; }
    }

    // 事件系統
    public event System.Action<float> OnPlacementStart;
    public event System.Action OnPlacementEnd;
    public event System.Action<int> OnRoundStart;
    public event System.Action<int> OnRoundEnd;
    public event System.Action<float> OnCountdownTick; // UI 倒數顯示用
    public event System.Action<bool> OnPlacementAllowedChange;

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
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        activePlayers = playerObjects.Select(p => p.GetComponent<PlayerScore>())
                                     .Where(ps => ps != null).ToList();

        if (activePlayers.Count > 0)
        {
            StartRound();
        }
        else
        {
            Debug.LogError("找不到 PlayerScore！請確認 Tag 與 Script。");
        }
    }

    // 🔄 核心回合邏輯
    private IEnumerator RoundCycleSequence()
    {
        roundNumber++;
        placementPhaseCount = 0;
        isRoundActive = true;
        Debug.Log($"=== Round {roundNumber} 開始 (玩家復活並回到重生點) ===");

        // 1️⃣ 回合開始：復活所有玩家 (這裡會讓玩家回到重生點)
        foreach (var player in activePlayers)
        {
            if (player != null)
            {
                // 請確保 PlayerScore.Revive() 裡面有 transform.position = spawnPoint 的邏輯
                player.Revive();
            }
        }
        OnRoundStart?.Invoke(roundNumber);

        // 2️⃣ 進入放置循環 (如果是單人模式，這裡只會跑一次)
        while (isRoundActive)
        {
            placementPhaseCount++;

            // --- 放置階段 ---
            yield return StartCoroutine(HandlePlacementPhase(placementPhaseCount));

            // --- 檢查是否已經全滅 ---
            if (CheckIfAllDead())
            {
                isRoundActive = false;
                break; // 直接跳出，進入結算
            }
            else if (activePlayers.Count == 1)
            {
                // 單人模式：放置一次後，直接進入「生存計時」
                Debug.Log("單人模式：放置結束，進入生存挑戰！");
                break;
            }

            // --- 多人模式：放置間隔休息 ---
            Debug.Log($"放置間隔冷卻 {interPlacementDelay} 秒...");
            OnCountdownTick?.Invoke(0);
            OnPlacementAllowedChange?.Invoke(false);
            yield return new WaitForSeconds(interPlacementDelay);
        }

        // 3️⃣ 生存階段計時 (這是你原本缺少的部分)
        // 如果跳出了放置循環，且回合還是 Active (代表還有人活著)，就開始倒數 30 秒
        if (isRoundActive)
        {
            float timer = 0f;
            Debug.Log($"生存計時開始：限時 {roundDuration} 秒");

            while (isRoundActive)
            {
                // A. 每一幀都檢查有沒有死光
                if (CheckIfAllDead())
                {
                    Debug.Log("💀 玩家死亡，回合提早結束");
                    break;
                }

                // B. 計時邏輯
                if (roundDuration > 0)
                {
                    timer += Time.deltaTime;
                    float timeLeft = Mathf.Max(0, roundDuration - timer);

                    // 更新 UI 倒數
                    OnCountdownTick?.Invoke(timeLeft);

                    // 時間到！
                    if (timer >= roundDuration)
                    {
                        Debug.Log("⏰ 時間到！強制結束回合 (視同死亡)");

                        // 這裡可以選擇是否要「殺死」玩家來播放死亡動畫
                        // foreach (var p in activePlayers) { if(p.isAlive) p.Die(); }

                        break; // 跳出迴圈，直接執行 EndRound
                    }
                }

                yield return null; // 等待下一幀
            }
        }

        // 4️⃣ 觸發回合結束
        EndRound();
    }

    // 輔助檢查方法
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
        while (pTimer > 0 && isPlacementPhase && isRoundActive)
        {
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
        if (!isRoundActive) return; // 避免重複呼叫

        isRoundActive = false;
        if (roundCycleCoroutine != null) StopCoroutine(roundCycleCoroutine);

        Debug.Log($"=== Round {roundNumber} 結束！準備重置 ===");
        OnRoundEnd?.Invoke(roundNumber);

        StartCoroutine(IntermissionSequence());
    }

    private IEnumerator IntermissionSequence()
    {
        // 顯示 0 秒給 UI
        OnCountdownTick?.Invoke(0);

        Debug.Log($"休息 {intermissionDelay} 秒...");
        yield return new WaitForSeconds(intermissionDelay);

        // 重新開始下一回合 (這會觸發 StartRound -> Revive -> 回到重生點)
        StartRound();
    }

    // 供外部 (PlayerScore) 呼叫
    public void NotifyPlayerDeath(PlayerScore player)
    {
        // 這裡不需要做太多事，因為 RoundCycleSequence 的 while 迴圈會自動檢測到死亡
        // 但如果想要「一死就立刻觸發」，可以保留這個方法來雙重確認
    }

    public void NotifyPlayerReachedGoal(PlayerScore player)
    {
        if (isRoundActive) EndRound();
    }
}