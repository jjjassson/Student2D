using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoundManager : MonoBehaviour
{
    // ... (前面的變數保持不變) ...
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
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        activePlayers = playerObjects.Select(p => p.GetComponent<PlayerScore>())
                                     .Where(ps => ps != null).ToList();

        if (activePlayers.Count > 0) StartRound();
        else Debug.LogError("找不到 PlayerScore！請確認 Tag 與 Script。");
    }

    // 🔄 核心回合邏輯 (修改重點在這裡)
    private IEnumerator RoundCycleSequence()
    {
        roundNumber++;
        placementPhaseCount = 0;
        isRoundActive = true;
        Debug.Log($"=== Round {roundNumber} 開始 (玩家復活並回到重生點) ===");

        // 1️⃣ 回合開始：復活所有玩家
        foreach (var player in activePlayers)
        {
            if (player != null) player.Revive();
        }
        OnRoundStart?.Invoke(roundNumber);

        // 2️⃣ 進入放置循環
        while (isRoundActive)
        {
            placementPhaseCount++;

            // --- 放置階段 ---
            yield return StartCoroutine(HandlePlacementPhase(placementPhaseCount));

            // --- 檢查是否已經全滅 (修正處) ---
            if (CheckIfAllDead())
            {
                Debug.Log("💀 放置階段全員死亡，跳過剩餘階段");
                // ❌ 移除這行： isRoundActive = false; 
                // 原因：如果這裡設為 false，下面的 EndRound() 會因為判斷 !isRoundActive 而直接 return，導致無法重生。
                break;
            }
            else if (activePlayers.Count == 1)
            {
                Debug.Log("單人模式：放置結束，進入生存挑戰！");
                break;
            }

            // --- 多人模式：放置間隔休息 ---
            Debug.Log($"放置間隔冷卻 {interPlacementDelay} 秒...");
            OnCountdownTick?.Invoke(0);
            OnPlacementAllowedChange?.Invoke(false);
            yield return new WaitForSeconds(interPlacementDelay);
        }

        // 3️⃣ 生存階段計時
        // 如果在放置階段全滅，程式會跑到這裡。
        // 因為我們剛剛移除了 isRoundActive = false，所以這裡的 if (isRoundActive) 會是 true，得以進入檢查。
        if (isRoundActive)
        {
            // 如果已經全滅，這裡的第一個檢查就會攔截並跳出，不會真的跑 30 秒
            if (CheckIfAllDead())
            {
                // 全員已死，什麼都不做，直接讓它往下跑到 EndRound
            }
            else
            {
                // 只有還有人活著才會開始倒數
                float timer = 0f;
                Debug.Log($"生存計時開始：限時 {roundDuration} 秒");

                while (isRoundActive)
                {
                    if (CheckIfAllDead())
                    {
                        Debug.Log("💀 玩家死亡，回合提早結束");
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
        }

        // 4️⃣ 觸發回合結束 (現在無論何時死，都能正確執行這裡了)
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
        // ⚠️ 這裡原本的邏輯擋住了放置階段死亡的重置
        // 因為剛剛我們移除了 loop 裡的 isRoundActive = false，所以現在即使放置階段死掉，isRoundActive 仍為 true，這裡就能通過。
        if (!isRoundActive) return;

        isRoundActive = false; // 在這裡統一關閉
        if (roundCycleCoroutine != null) StopCoroutine(roundCycleCoroutine);

        Debug.Log($"=== Round {roundNumber} 結束！準備重置 ===");
        OnRoundEnd?.Invoke(roundNumber);

        StartCoroutine(IntermissionSequence());
    }

    private IEnumerator IntermissionSequence()
    {
        OnCountdownTick?.Invoke(0);
        Debug.Log($"休息 {intermissionDelay} 秒...");
        yield return new WaitForSeconds(intermissionDelay);
        StartRound();
    }

    // ... (Notify 方法保持不變) ...
    public void NotifyPlayerDeath(PlayerScore player) { }
    public void NotifyPlayerReachedGoal(PlayerScore player) { if (isRoundActive) EndRound(); }
}