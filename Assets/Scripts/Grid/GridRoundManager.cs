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

    // --- 🔥 新增：UI 顯示用的變數 ---
    private string uiCurrentPhaseText = ""; // 目前階段文字
    private float uiTimeLeft = 0f;          // 剩餘時間

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
        SetPhaseStatus("放置階段", placementColor); // 🔥 更新 UI 狀態
        SetAllPlayersPlacementMode(true);

        float pTimer = placementTime;
        while (pTimer > 0)
        {
            if (CheckIfAllDead()) break;

            uiTimeLeft = pTimer; // 🔥 更新倒數時間給 UI 顯示
            OnCountdownTick?.Invoke(pTimer);

            pTimer -= Time.deltaTime;
            yield return null;
        }
        SetAllPlayersPlacementMode(false);

        // 3️⃣ 生存階段
        SetPhaseStatus("生存挑戰", survivalColor); // 🔥 更新 UI 狀態

        if (!CheckIfAllDead())
        {
            float sTimer = survivalTime;
            while (sTimer > 0)
            {
                if (CheckIfAllDead()) break;

                uiTimeLeft = sTimer; // 🔥 更新倒數時間給 UI 顯示
                OnCountdownTick?.Invoke(sTimer);

                sTimer -= Time.deltaTime;
                yield return null;
            }
        }

        // 4️⃣ 回合結束
        SetPhaseStatus("回合結束", defaultColor); // 🔥 更新 UI 狀態
        uiTimeLeft = 0;

        OnRoundEnd?.Invoke(roundNumber);
        IsRoundActive = false;

        foreach (var p in players) ResetPlayerPosition(p);
        OnCountdownTick?.Invoke(0);

        yield return new WaitForSeconds(intermissionDelay);

        StartCoroutine(RoundCycleSequence());
    }

    // --- 輔助函式：統一設定階段文字與事件 ---
    private void SetPhaseStatus(string text, Color color)
    {
        uiCurrentPhaseText = text;      // 給 OnGUI 用
        OnPhaseChange?.Invoke(text);    // 給外部事件用

        // 為了讓 OnGUI 知道現在要用什麼顏色，我們可以把 color 存到一個臨時變數，
        // 或者簡單一點，直接在 OnGUI 裡判斷文字內容。
        // 這裡為了方便，我新增一個變數存顏色：
        uiCurrentColor = color;
    }

    // 用來存當前文字顏色
    private Color uiCurrentColor = Color.white;

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

    // --- 🔥 新增：OnGUI 顯示邏輯 ---
    void OnGUI()
    {
        // 如果還沒開始或找不到玩家，就不顯示
        if (players.Count == 0) return;

        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.fontSize = uiFontSize;
        statusStyle.alignment = TextAnchor.MiddleCenter;
        statusStyle.fontStyle = FontStyle.Bold;
        statusStyle.normal.textColor = uiCurrentColor; // 使用當前階段的顏色

        float labelWidth = 800f;
        float labelHeight = 150f;
        // 顯示在螢幕下方
        Rect statusRect = new Rect((Screen.width - labelWidth) / 2, Screen.height - labelHeight - 20, labelWidth, labelHeight);

        // 組合顯示文字： "階段名稱 : 9"
        // Mathf.CeilToInt 會讓時間顯示整數 (例如 9.1s 顯示 10)
        string displayText = $"{uiCurrentPhaseText}";

        // 如果時間大於 0，才顯示倒數數字
        if (uiTimeLeft > 0)
        {
            displayText += $" : {Mathf.CeilToInt(uiTimeLeft)}";
        }

        // 繪製文字
        GUI.Label(statusRect, displayText, statusStyle);
    }
}