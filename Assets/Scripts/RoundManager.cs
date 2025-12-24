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

    [Header("隨機物品設定 (新功能)")]
    [Tooltip("請把做好的 BuildingData (房子、樹...) 全部拖進來")]
    public BuildingData[] itemPool;

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

    private IEnumerator RoundCycleSequence()
    {
        roundNumber++;
        placementPhaseCount = 0;
        isRoundActive = true;

        Debug.Log($"=== Round {roundNumber} 開始 ===");

        // 復活玩家
        foreach (var player in activePlayers)
        {
            if (player != null) player.Revive();
        }
        OnRoundStart?.Invoke(roundNumber);

        // 放置循環
        while (isRoundActive)
        {
            placementPhaseCount++;

            // --- 執行放置階段 ---
            yield return StartCoroutine(HandlePlacementPhase(placementPhaseCount));

            // 檢查全滅
            if (CheckIfAllDead())
            {
                Debug.Log("💀 放置階段全員死亡，直接結算");
                break;
            }

            // 單人模式檢查
            if (activePlayers.Count == 1)
            {
                break;
            }

            // 放置間隔休息
            Debug.Log($"放置間隔冷卻 {interPlacementDelay} 秒...");
            OnCountdownTick?.Invoke(0);
            OnPlacementAllowedChange?.Invoke(false);
            yield return new WaitForSeconds(interPlacementDelay);
        }

        // 生存階段
        if (isRoundActive && !CheckIfAllDead())
        {
            float timer = 0f;
            Debug.Log($"生存計時開始：限時 {roundDuration} 秒");

            while (isRoundActive)
            {
                if (CheckIfAllDead()) break;

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

        // ★★★ 新增：發牌給所有玩家 ★★★
        DistributeRandomItems();

        // ★★★ 新增：允許所有玩家移動游標 ★★★
        SetAllPlayersPlacement(true);

        OnPlacementAllowedChange?.Invoke(true);
        OnPlacementStart?.Invoke(placementTimePerPlayer);

        float pTimer = placementTimePerPlayer;

        while (pTimer > 0 && isPlacementPhase && isRoundActive)
        {
            if (CheckIfAllDead()) break;

            OnCountdownTick?.Invoke(pTimer);
            pTimer -= Time.deltaTime;
            yield return null;
        }

        isPlacementPhase = false;

        // ★★★ 新增：時間到，禁止移動 ★★★
        SetAllPlayersPlacement(false);

        OnPlacementEnd?.Invoke();
        OnPlacementAllowedChange?.Invoke(false);
    }

    // --- 新增的功能函式 ---

    // 隨機發牌
    private void DistributeRandomItems()
    {
        // 抓取所有有 PlayerInventory 的玩家
        PlayerInventory[] players = FindObjectsOfType<PlayerInventory>();

        if (itemPool.Length == 0)
        {
            Debug.LogError("RoundManager 卡池是空的！請在 Inspector 拖入 BuildingData。");
            return;
        }

        foreach (var player in players)
        {
            // 隨機抽一張
            int randomIndex = Random.Range(0, itemPool.Length);
            player.EquipItem(itemPool[randomIndex]);
        }
        Debug.Log("已隨機發放物品");
    }

    // 控制所有玩家能不能放置/移動
    private void SetAllPlayersPlacement(bool allowed)
    {
        PlayerInventory[] players = FindObjectsOfType<PlayerInventory>();
        foreach (var p in players)
        {
            p.SetPlacementState(allowed);
        }
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
        OnRoundEnd?.Invoke(roundNumber);

        if (roundCycleCoroutine != null) StopCoroutine(roundCycleCoroutine);
        StartCoroutine(IntermissionSequence());
    }

    private IEnumerator IntermissionSequence()
    {
        OnCountdownTick?.Invoke(0);
        Debug.Log($"休息 {intermissionDelay} 秒...");
        yield return new WaitForSeconds(intermissionDelay);
        StartRound();
    }

    public void NotifyPlayerDeath(PlayerScore player) { }
    public void NotifyPlayerReachedGoal(PlayerScore player) { if (isRoundActive) EndRound(); }
}