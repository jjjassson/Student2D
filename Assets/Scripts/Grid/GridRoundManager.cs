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
    public int uiRoundFontSize = 80;    // 回合文字的大小
    public Color roundTextColor = Color.white; // 回合文字的顏色
    public Color placementColor = Color.green;
    public Color survivalColor = Color.red;
    public Color defaultColor = Color.yellow;

    [Header("攝影機錄影")]
    [Tooltip("請將掛有 CameraRecorder 的主攝影機拖進來")]
    public CameraRecorder mainCamRecorder;

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
    private bool isGameOver = false; // 🔥 新增：判斷遊戲是否已結束 (進入回放)

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
        isGameOver = false; // 確保開始時狀態正確
        StartCoroutine(StartGameRoutine());
    }

    // 🔥 強制停止遊戲循環的方法
    // 這會在 GoalPoint 觸發時被呼叫，防止進入下一回合
    public void StopGameLoop()
    {
        Debug.Log("遊戲結束，停止所有計時與循環。");

        // 殺死 StartGameRoutine 和 RoundCycleSequence
        StopAllCoroutines();

        IsRoundActive = false;
        isGameOver = true; // 🔥 設定為 True，用來隱藏 Round 文字

        // 更新 UI 狀態讓玩家知道結束了
        SetPhaseStatus("遊戲結束 - 精彩回放", Color.cyan);
        uiTimeLeft = 0;
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

        // 自動尋找主攝影機錄影機 (防止忘記拉)
        if (mainCamRecorder == null)
        {
            mainCamRecorder = FindObjectOfType<CameraRecorder>();
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

        // 1️⃣ 復活、重置位置與 **開始錄影**
        foreach (var p in players)
        {
            if (p.score != null) p.score.Revive();
            ResetPlayerPosition(p);

            // 啟動玩家錄影 (維持只錄玩家)
            ReplayRecorder recorder = p.placer.GetComponent<ReplayRecorder>();
            if (recorder != null)
            {
                recorder.StartNewRecording();
            }
        }

        // 啟動鏡頭錄影
        if (mainCamRecorder != null)
        {
            mainCamRecorder.StartRecording();
        }
        else
        {
            Debug.LogWarning("注意：沒有設定 CameraRecorder，重播時鏡頭不會動！");
        }

        // 發牌邏輯
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

    // --- OnGUI 顯示邏輯 ---
    void OnGUI()
    {
        if (players.Count == 0) return;

        // 1. 顯示階段文字 (例如：生存挑戰、遊戲結束-精彩回放)
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

        // 2. 顯示 Round 幾 (🔥 只有在遊戲尚未結束時才顯示)
        if (!isGameOver)
        {
            GUIStyle roundStyle = new GUIStyle(GUI.skin.label);
            roundStyle.fontSize = uiRoundFontSize;
            roundStyle.alignment = TextAnchor.UpperCenter;
            roundStyle.fontStyle = FontStyle.Bold;
            roundStyle.normal.textColor = roundTextColor;

            Rect roundRect = new Rect(0, 30, Screen.width, 150);
            GUI.Label(roundRect, $"Round {roundNumber}", roundStyle);
        }
    }
}