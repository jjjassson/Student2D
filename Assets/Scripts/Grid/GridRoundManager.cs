using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 1. 定義資料結構
[System.Serializable]
public class GridItemPair
{
    public string itemName = "New Item";
    public GameObject mainPrefab;
    public GameObject secondaryPrefab;
}

public class GridRoundManager : MonoBehaviour
{
    public static GridRoundManager Instance { get; private set; }

    [Header("物件資料夾")]
    public List<GridItemPair> itemFolder = new List<GridItemPair>();

    [Header("階段時間設定")]
    public float placementTime = 10f;
    public float survivalTime = 20f;
    public float intermissionDelay = 3f;

    [Header("UI 設定")]
    public int uiFontSize = 60;
    public int uiRoundFontSize = 80;    // 🔥 新增：回合文字的大小
    public Color roundTextColor = Color.white; // 🔥 新增：回合文字的顏色
    public Color placementColor = Color.green;
    public Color survivalColor = Color.red;
    public Color defaultColor = Color.yellow;

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
    public bool IsRoundActive { get; private set; } = false;

    // UI 顯示用的變數
    private string uiCurrentPhaseText = ""; // 目前階段文字
    private float uiTimeLeft = 0f;          // 剩餘時間
    private Color uiCurrentColor = Color.white; // 目前階段文字顏色

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
        IsRoundActive = true;
        Debug.Log($"=== Round {roundNumber} 開始 ===");
        OnRoundStart?.Invoke(roundNumber);

        // 1️⃣ 復活與發牌
        foreach (var p in players)
        {
            if (p.score != null) p.score.Revive();
            ResetPlayerPosition(p);
        }

        if (itemFolder.Count > 0)
        {
            GridItemPair selectedPair = itemFolder[Random.Range(0, itemFolder.Count)];
            foreach (var p in players)
            {
                p.placer.AssignNewObjectPair(selectedPair.mainPrefab, selectedPair.secondaryPrefab);
            }
        }

        // 2️⃣ 放置階段
        SetPhaseStatus("放置階段", placementColor);
        SetAllPlayersPlacementMode(true);

        float pTimer = placementTime;
        while (pTimer > 0)
        {
            if (CheckIfAllDead()) break;

            uiTimeLeft = pTimer;
            OnCountdownTick?.Invoke(pTimer);

            pTimer -= Time.deltaTime;
            yield return null;
        }
        SetAllPlayersPlacementMode(false);

        // 3️⃣ 生存階段
        SetPhaseStatus("生存挑戰", survivalColor);

        if (!CheckIfAllDead())
        {
            float sTimer = survivalTime;
            while (sTimer > 0)
            {
                if (CheckIfAllDead()) break;

                uiTimeLeft = sTimer;
                OnCountdownTick?.Invoke(sTimer);

                sTimer -= Time.deltaTime;
                yield return null;
            }
        }

        // 4️⃣ 回合結束
        SetPhaseStatus("回合結束", defaultColor);
        uiTimeLeft = 0;

        OnRoundEnd?.Invoke(roundNumber);
        IsRoundActive = false;

        foreach (var p in players) ResetPlayerPosition(p);
        OnCountdownTick?.Invoke(0);

        yield return new WaitForSeconds(intermissionDelay);

        StartCoroutine(RoundCycleSequence());
    }

    private void SetPhaseStatus(string text, Color color)
    {
        uiCurrentPhaseText = text;
        OnPhaseChange?.Invoke(text);
        uiCurrentColor = color;
    }

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

    // --- 🔥 修改後的 OnGUI 顯示邏輯 ---
    void OnGUI()
    {
        // 如果還沒開始或找不到玩家，就不顯示
        if (players.Count == 0) return;

        // ----------------------------------------------------
        // 1. 顯示【下方】的階段狀態與倒數 (原有的)
        // ----------------------------------------------------
        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.fontSize = uiFontSize;
        statusStyle.alignment = TextAnchor.MiddleCenter;
        statusStyle.fontStyle = FontStyle.Bold;
        statusStyle.normal.textColor = uiCurrentColor;

        float labelWidth = 800f;
        float labelHeight = 150f;
        Rect statusRect = new Rect((Screen.width - labelWidth) / 2, Screen.height - labelHeight - 20, labelWidth, labelHeight);

        string displayText = $"{uiCurrentPhaseText}";
        if (uiTimeLeft > 0)
        {
            displayText += $" : {Mathf.CeilToInt(uiTimeLeft)}";
        }
        GUI.Label(statusRect, displayText, statusStyle);


        // ----------------------------------------------------
        // 2. 🔥 新增：顯示【上方】的 Round 數字
        // ----------------------------------------------------
        GUIStyle roundStyle = new GUIStyle(GUI.skin.label);
        roundStyle.fontSize = uiRoundFontSize;  // 使用獨立的字體大小
        roundStyle.alignment = TextAnchor.UpperCenter; // 設定為上方置中
        roundStyle.fontStyle = FontStyle.Bold;
        roundStyle.normal.textColor = roundTextColor; // 設定顏色

        // 顯示在螢幕上方，距離頂部 30 pixel
        Rect roundRect = new Rect(0, 30, Screen.width, 150);

        // 顯示文字：Round X
        GUI.Label(roundRect, $"Round {roundNumber}", roundStyle);
    }
}