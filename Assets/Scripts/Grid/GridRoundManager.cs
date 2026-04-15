using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// 1. 定義資料結構 (修正了剛剛漏掉的部分)
[System.Serializable]
public class GridItemPair
{
    public string itemName = "New Item";
    public GameObject mainPrefab;      // 主物件
    public GameObject secondaryPrefab; // 副物件 (Z軸固定)
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

    [Header("舊版文字 UI 設定")]
    public bool showUI = true;
    public int uiFontSize = 60;
    public int uiRoundFontSize = 80;
    public Color roundTextColor = Color.white;
    public Color placementColor = Color.green;
    public Color survivalColor = Color.red;
    public Color defaultColor = Color.yellow;

    [Header("新版圖形 UI 設定")]
    public bool showNewUI = true;
    [Tooltip("請把你整個新 UI 的總面板拖進來")]
    public GameObject newUIPanel;

    [Tooltip("請將 Hierarchy 中那個叫做『數字』的資料夾物件拖到這裡")]
    public GameObject numberFolder;

    [Header("狀態圖片設定")]
    public Image phaseImage;
    public Sprite placementPhaseSprite;
    public Sprite survivalPhaseSprite;
    public Sprite defaultPhaseSprite;

    [Header("倒數數字圖片設定 (請依序放 0 到 9)")]
    public Sprite[] numberSprites = new Sprite[10];
    public Image timerTensImage;
    public Image timerUnitsImage;

    [Header("攝影機錄影")]
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
    private int roundNumber = 0;
    public bool IsRoundActive { get; private set; } = false;
    private bool isGameOver = false;

    // UI 顯示用的變數
    private string uiCurrentPhaseText = "";
    private float uiTimeLeft = 0f;
    private Color uiCurrentColor = Color.white;

    // 事件
    public event System.Action<float> OnCountdownTick;
    public event System.Action<string> OnPhaseChange;
    public event System.Action<int> OnRoundStart;
    public event System.Action<int> OnRoundEnd;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 將新關卡的 UI 和攝影機，交接給存活著的舊 Manager
            Instance.newUIPanel = this.newUIPanel;
            Instance.numberFolder = this.numberFolder; // 交接「數字」資料夾
            Instance.phaseImage = this.phaseImage;
            Instance.timerTensImage = this.timerTensImage;
            Instance.timerUnitsImage = this.timerUnitsImage;
            Instance.mainCamRecorder = this.mainCamRecorder;
            Instance.itemFolder = this.itemFolder;

            // 確保每次進入新關卡時，狀態都是全新的
            Instance.isGameOver = false;
            Instance.roundNumber = 0;

            // 停止舊的迴圈，重新啟動新關卡的流程
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.StartGameRoutine());

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        isGameOver = false;
        StartCoroutine(StartGameRoutine());
    }

    private void Update()
    {
        // 判斷條件：(必須開啟新 UI) 且 (遊戲尚未結束)
        bool shouldShow = showNewUI && !isGameOver;

        if (newUIPanel != null)
        {
            newUIPanel.SetActive(shouldShow);
        }

        // 強制控制「數字」資料夾的開關，重播時會被關閉
        if (numberFolder != null)
        {
            numberFolder.SetActive(shouldShow);
        }

        if (shouldShow)
        {
            UpdateTimerImages();
        }
    }

    private void UpdateTimerImages()
    {
        if (timerTensImage == null || timerUnitsImage == null || numberSprites.Length < 10) return;

        int timeInt = Mathf.Max(0, Mathf.CeilToInt(uiTimeLeft));
        if (timeInt > 99) timeInt = 99;

        int tens = timeInt / 10;
        int units = timeInt % 10;

        timerTensImage.sprite = numberSprites[tens];
        timerUnitsImage.sprite = numberSprites[units];
    }

    public void StopGameLoop()
    {
        Debug.Log("停止遊戲循環，進入重播，隱藏所有 UI。");

        StopAllCoroutines();

        IsRoundActive = false;
        isGameOver = true; // 設定為 true 後，Update 會自動把數字資料夾隱藏

        SetPhaseStatus("遊戲結束", Color.cyan);
        uiTimeLeft = 0;
    }

    IEnumerator StartGameRoutine()
    {
        yield return new WaitForEndOfFrame();

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
        OnRoundStart?.Invoke(roundNumber);

        foreach (var p in players)
        {
            if (p.score != null) p.score.Revive();
            ResetPlayerPosition(p);

            ReplayRecorder recorder = p.placer.GetComponent<ReplayRecorder>();
            if (recorder != null)
            {
                recorder.StartNewRecording();
            }
        }

        if (mainCamRecorder != null)
        {
            mainCamRecorder.StartRecording();
        }

        if (itemFolder.Count > 0)
        {
            foreach (var p in players)
            {
                GridItemPair randomPair = itemFolder[Random.Range(0, itemFolder.Count)];
                p.placer.AssignNewObjectPair(randomPair.mainPrefab, randomPair.secondaryPrefab);
            }
        }

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

        if (phaseImage != null)
        {
            if (text == "放置階段")
                phaseImage.sprite = placementPhaseSprite;
            else if (text == "生存挑戰")
                phaseImage.sprite = survivalPhaseSprite;
            else
                phaseImage.sprite = defaultPhaseSprite;
        }
    }

    private void SetAllPlayersPlacementMode(bool active)
    {
        foreach (var p in players) if (p.placer != null) p.placer.SetPlacementMode(active);
    }

    private void ResetPlayerPosition(PlayerData p)
    {
        if (p.rb != null)
        {
            p.rb.velocity = Vector3.zero;
            p.rb.angularVelocity = Vector3.zero;
        }

        CharacterController cc = p.placer.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (p.placer != null)
        {
            p.placer.transform.position = p.spawnPoint;
        }

        if (cc != null) cc.enabled = true;
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

    void OnGUI()
    {
        if (!showUI || isGameOver) return;

        if (players.Count == 0) return;

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

        GUIStyle roundStyle = new GUIStyle(GUI.skin.label);
        roundStyle.fontSize = uiRoundFontSize;
        roundStyle.alignment = TextAnchor.UpperCenter;
        roundStyle.fontStyle = FontStyle.Bold;
        roundStyle.normal.textColor = roundTextColor;

        Rect roundRect = new Rect(0, 30, Screen.width, 150);
        GUI.Label(roundRect, $"Round {roundNumber}", roundStyle);
    }
}