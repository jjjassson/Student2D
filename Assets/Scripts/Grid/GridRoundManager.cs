using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridRoundManager : MonoBehaviour
{
    public static GridRoundManager Instance { get; private set; }

    [Header("物件資料夾")]
    public List<GameObject> objectFolder = new List<GameObject>();

    [Header("階段時間設定")]
    public float placementTime = 10f;
    public float survivalTime = 20f;
    public float intermissionDelay = 3f;

    // --- 內部資料結構 ---
    // 我們定義一個簡單的類別來綁定玩家的所有資訊
    private class PlayerData
    {
        public GridObjectPlacer placer; // 負責放置
        public PlayerScore score;       // 負責生命與分數
        public Vector3 spawnPoint;      // 出生點
        public Rigidbody rb;            // 物理 (用於重置位置時歸零速度)
    }

    private List<PlayerData> players = new List<PlayerData>();

    // --- 狀態變數 ---
    private int roundNumber = 0;
    private bool isRoundActive = false;

    // --- 事件系統 (保留舊版功能) ---
    public event System.Action<float> OnCountdownTick; // UI 倒數用
    public event System.Action<string> OnPhaseChange;  // UI 顯示階段文字用
    public event System.Action<int> OnRoundStart;      // 回合開始事件
    public event System.Action<int> OnRoundEnd;        // 回合結束事件

    private Coroutine roundCycleCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(StartGameRoutine());
    }

    // 初始化：抓取玩家並記錄資料
    IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        // 重新抓取場景上的玩家 (透過 Tag 或 Type 都可以)
        GridObjectPlacer[] foundPlacers = FindObjectsOfType<GridObjectPlacer>();

        players.Clear();
        foreach (var placer in foundPlacers)
        {
            PlayerData data = new PlayerData();
            data.placer = placer;
            data.score = placer.GetComponent<PlayerScore>(); // 抓取同一物件上的 Score 腳本
            data.spawnPoint = placer.transform.position;     // 記錄出生點
            data.rb = placer.GetComponent<Rigidbody>();

            players.Add(data);
        }

        if (players.Count > 0)
        {
            StartCoroutine(RoundCycleSequence());
        }
        else
        {
            Debug.LogError("錯誤：找不到玩家！請確認玩家身上有 GridObjectPlacer 和 PlayerScore。");
        }
    }

    // 🔥 核心回合流程 (融合版) 🔥
    private IEnumerator RoundCycleSequence()
    {
        roundNumber++;
        isRoundActive = true;
        Debug.Log($"=== Round {roundNumber} 開始 ===");
        OnRoundStart?.Invoke(roundNumber);

        // ==========================================
        // 1️⃣ 復活與發牌階段
        // ==========================================

        // A. 復活所有玩家並重置位置
        foreach (var p in players)
        {
            if (p.score != null) p.score.Revive(); // 呼叫舊有的復活
            ResetPlayerPosition(p);                // 回到出生點
        }

        // B. 發牌 (給予隨機物件)
        if (objectFolder.Count > 0)
        {
            GameObject selectedObj = objectFolder[Random.Range(0, objectFolder.Count)];
            foreach (var p in players)
            {
                p.placer.AssignNewObject(selectedObj);
            }
        }

        // ==========================================
        // 2️⃣ 放置階段 (10秒)
        // ==========================================
        OnPhaseChange?.Invoke("放置階段");

        // 開啟右搖桿控制
        SetAllPlayersPlacementMode(true);

        float pTimer = placementTime;
        while (pTimer > 0)
        {
            // 每一幀檢查全滅 (舊版功能)
            if (CheckIfAllDead())
            {
                Debug.Log("放置階段全滅，提早結束");
                break;
            }

            OnCountdownTick?.Invoke(pTimer);
            pTimer -= Time.deltaTime;
            yield return null;
        }

        // 時間到！強制放置 (如果有鬼影沒放的話)
        SetAllPlayersPlacementMode(false);

        // ==========================================
        // 3️⃣ 生存階段 (20秒)
        // ==========================================
        OnPhaseChange?.Invoke("生存挑戰");

        // 如果放置階段就全滅了，這裡會直接跳過，或者你可以讓它繼續跑
        // 這裡邏輯是：只要還有人活著，就跑生存計時
        if (!CheckIfAllDead())
        {
            float sTimer = survivalTime;
            while (sTimer > 0)
            {
                // 生存階段全滅檢查
                if (CheckIfAllDead())
                {
                    Debug.Log("生存階段全滅，提早結束");
                    break;
                }

                OnCountdownTick?.Invoke(sTimer);
                sTimer -= Time.deltaTime;
                yield return null;
            }
        }

        // ==========================================
        // 4️⃣ 回合結束：處死並重置
        // ==========================================
        OnPhaseChange?.Invoke("回合結束");
        OnRoundEnd?.Invoke(roundNumber); // 觸發舊版結束事件

        Debug.Log("時間到！重置回合...");

        // 雖然下回合開始會復活，但你原本邏輯是「時間到視為死亡並回到原點」
        // 所以這裡執行一次強制回歸
        foreach (var p in players)
        {
            ResetPlayerPosition(p);
        }

        OnCountdownTick?.Invoke(0);
        yield return new WaitForSeconds(intermissionDelay);

        // 重啟下一回合
        StartCoroutine(RoundCycleSequence());
    }

    // --- 輔助函式 ---

    private void SetAllPlayersPlacementMode(bool active)
    {
        foreach (var p in players)
        {
            if (p.placer != null) p.placer.SetPlacementMode(active);
        }
    }

    private void ResetPlayerPosition(PlayerData p)
    {
        if (p.rb != null)
        {
            p.rb.velocity = Vector3.zero;
            p.rb.angularVelocity = Vector3.zero;
        }

        // 將玩家傳送回一開始記錄的 spawnPoint
        if (p.placer != null)
        {
            p.placer.transform.position = p.spawnPoint;
        }
    }

    // 檢查是否所有玩家都死亡 (依賴 PlayerScore.isAlive)
    private bool CheckIfAllDead()
    {
        // 如果沒有玩家資料，視為全滅
        if (players.Count == 0) return true;

        // 只要有一個人是活的 (isAlive == true)，就回傳 false (還沒全滅)
        // 注意：這裡假設 PlayerScore 有 public bool isAlive
        foreach (var p in players)
        {
            if (p.score != null && p.score.isAlive)
            {
                return false;
            }
        }
        return true;
    }
}