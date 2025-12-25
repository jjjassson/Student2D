using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 1. 定義資料結構 (直接放在這裡或獨立檔案皆可)
[System.Serializable]
public class GridItemPair
{
    public string itemName = "New Item";
    public GameObject mainPrefab;      // 主物件 (跟隨游標 X, Z)
    public GameObject secondaryPrefab; // 副物件 (跟隨 X, Z 永遠為 0)
}

public class GridRoundManager : MonoBehaviour
{
    public static GridRoundManager Instance { get; private set; }

    [Header("物件資料夾 (請在這裡設定成對物件)")]
    // 2. 修改：使用 ItemPair 取代原本的 GameObject List
    public List<GridItemPair> itemFolder = new List<GridItemPair>();

    [Header("階段時間設定")]
    public float placementTime = 10f;
    public float survivalTime = 20f;
    public float intermissionDelay = 3f;

    // --- 內部資料結構 ---
    private class PlayerData
    {
        public GridObjectPlacer placer;
        public PlayerScore score;
        public Vector3 spawnPoint;
        public Rigidbody rb;
    }

    private List<PlayerData> players = new List<PlayerData>();

    // --- 狀態變數 ---
    private int roundNumber = 0;
    private bool isRoundActive = false;

    // 事件
    public event System.Action<float> OnCountdownTick;
    public event System.Action<string> OnPhaseChange;
    public event System.Action<int> OnRoundStart;
    public event System.Action<int> OnRoundEnd;

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

    IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        GridObjectPlacer[] foundPlacers = FindObjectsOfType<GridObjectPlacer>();
        players.Clear();
        foreach (var placer in foundPlacers)
        {
            PlayerData data = new PlayerData();
            data.placer = placer;
            data.score = placer.GetComponent<PlayerScore>();
            data.spawnPoint = placer.transform.position;
            data.rb = placer.GetComponent<Rigidbody>();
            players.Add(data);
        }

        if (players.Count > 0) StartCoroutine(RoundCycleSequence());
        else Debug.LogError("錯誤：找不到玩家！");
    }

    private IEnumerator RoundCycleSequence()
    {
        roundNumber++;
        isRoundActive = true;
        Debug.Log($"=== Round {roundNumber} 開始 ===");
        OnRoundStart?.Invoke(roundNumber);

        // 1️⃣ 復活與發牌
        foreach (var p in players)
        {
            if (p.score != null) p.score.Revive();
            ResetPlayerPosition(p);
        }

        // B. 修改：發牌邏輯 (傳遞 Pair)
        if (itemFolder.Count > 0)
        {
            // 隨機選一組
            GridItemPair selectedPair = itemFolder[Random.Range(0, itemFolder.Count)];

            foreach (var p in players)
            {
                // 呼叫 Placer 的新方法，傳入主與副
                p.placer.AssignNewObjectPair(selectedPair.mainPrefab, selectedPair.secondaryPrefab);
            }
        }

        // 2️⃣ 放置階段
        OnPhaseChange?.Invoke("放置階段");
        SetAllPlayersPlacementMode(true);

        float pTimer = placementTime;
        while (pTimer > 0)
        {
            if (CheckIfAllDead()) break;
            OnCountdownTick?.Invoke(pTimer);
            pTimer -= Time.deltaTime;
            yield return null;
        }
        SetAllPlayersPlacementMode(false);

        // 3️⃣ 生存階段
        OnPhaseChange?.Invoke("生存挑戰");
        if (!CheckIfAllDead())
        {
            float sTimer = survivalTime;
            while (sTimer > 0)
            {
                if (CheckIfAllDead()) break;
                OnCountdownTick?.Invoke(sTimer);
                sTimer -= Time.deltaTime;
                yield return null;
            }
        }

        // 4️⃣ 回合結束
        OnPhaseChange?.Invoke("回合結束");
        OnRoundEnd?.Invoke(roundNumber);

        foreach (var p in players) ResetPlayerPosition(p);
        OnCountdownTick?.Invoke(0);
        yield return new WaitForSeconds(intermissionDelay);

        StartCoroutine(RoundCycleSequence());
    }

    // --- 輔助函式 ---
    private void SetAllPlayersPlacementMode(bool active)
    {
        foreach (var p in players) if (p.placer != null) p.placer.SetPlacementMode(active);
    }

    private void ResetPlayerPosition(PlayerData p)
    {
        if (p.rb != null) { p.rb.velocity = Vector3.zero; p.rb.angularVelocity = Vector3.zero; }
        if (p.placer != null) p.placer.transform.position = p.spawnPoint;
    }

    private bool CheckIfAllDead()
    {
        if (players.Count == 0) return true;
        foreach (var p in players)
        {
            if (p.score != null && p.score.isAlive) return false;
        }
        return true;
    }
}