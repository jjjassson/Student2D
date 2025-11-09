using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// 更健壯的 PlayerConfigurationManager
/// - 顯示大量 Debug.Log 以便排錯
/// - 可指定 defaultPrefabs（選填），當玩家加入時會預設設定 SelectedCharacterPrefab
/// - 提供 DebugPrintConfigs() 可在任何時候手動呼叫印出目前狀態
/// </summary>
public class PlayerConfigurationManager : MonoBehaviour
{
    public static PlayerConfigurationManager Instance { get; private set; }

    private List<PlayerConfiguration> playerConfigs = new List<PlayerConfiguration>();
    [SerializeField] private int MaxPlayers = 2;

    // 可在 Inspector 拉兩個預設 prefab（如果你想讓加入時就有預設角色）
    [Header("Optional: default prefabs per playerIndex (0 = Player1, 1 = Player2)")]
    [SerializeField] private List<GameObject> defaultPrefabs = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("[Singleton] Second instance of PlayerConfigurationManager!");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
            Debug.Log("[PCM] Awake -> instance set, DontDestroyOnLoad");
        }
    }

    // 重要：當 PlayerInputManager 觸發加入時會呼叫這裡（確保 PlayerInputManager 正確設定）
    public void HandlePlayerJoin(PlayerInput pi)
    {
        if (pi == null)
        {
            Debug.LogError("[PCM] HandlePlayerJoin received null PlayerInput!");
            return;
        }

        Debug.Log($"[PCM] HandlePlayerJoin called. pi.playerIndex = {pi.playerIndex}, pi.name = {pi.name}");

        // 如果已存在同 playerIndex 的 config 就忽略（避免重複）
        if (playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
        {
            Debug.LogWarning($"[PCM] Player with index {pi.playerIndex} already exists in playerConfigs. Skipping add.");
            return;
        }

        var newConfig = new PlayerConfiguration(pi);

        // 如果 Inspector 裡有預設 prefab，嘗試用 playerIndex 把它放進 SelectedCharacterPrefab（避免 null）
        if (pi.playerIndex >= 0 && pi.playerIndex < defaultPrefabs.Count && defaultPrefabs[pi.playerIndex] != null)
        {
            newConfig.SelectedCharacterPrefab = defaultPrefabs[pi.playerIndex];
            Debug.Log($"[PCM] Assigned default prefab to playerIndex {pi.playerIndex}: {defaultPrefabs[pi.playerIndex].name}");
        }
        else
        {
            Debug.Log($"[PCM] No default prefab for playerIndex {pi.playerIndex} (defaultPrefabs size = {defaultPrefabs.Count})");
        }

        playerConfigs.Add(newConfig);
        Debug.Log($"[PCM] Added PlayerConfiguration: index={newConfig.PlayerIndex} totalCount={playerConfigs.Count}");

        // 每次加入後印出目前清單（便於檢查順序與 index）
        DebugPrintConfigs();
    }

    // 在你選角畫面/測試時可調用這方法手動設定
    public void SetPlayerCharacterPrefab(int index, GameObject prefab)
    {
        if (index >= 0 && index < playerConfigs.Count)
        {
            playerConfigs[index].SelectedCharacterPrefab = prefab;
            Debug.Log($"[PCM] SetPlayerCharacterPrefab index={index} prefab={(prefab ? prefab.name : "null")}");
        }
        else
        {
            Debug.LogWarning($"[PCM] SetPlayerCharacterPrefab failed: index {index} out of range (count={playerConfigs.Count})");
        }
    }

    public List<PlayerConfiguration> GetPlayerConfigs() => playerConfigs;

    public void ReadyPlayer(int index)
    {
        if (index < 0 || index >= playerConfigs.Count)
        {
            Debug.LogWarning($"[PCM] ReadyPlayer index out of range: {index}");
            return;
        }

        playerConfigs[index].isReady = true;
        Debug.Log($"[PCM] Player {index} isReady = true");

        if (playerConfigs.Count == MaxPlayers && playerConfigs.All(p => p.isReady))
        {
            Debug.Log("[PCM] All players ready. Loading scene 'Cliff'");
            SceneManager.LoadScene("Cliff"); // 替換成你的遊戲場景
        }
    }

    // 方便檢查目前 playerConfigs 的內容
    public void DebugPrintConfigs()
    {
        Debug.Log($"[PCM] ==== DebugPrintConfigs (Count={playerConfigs.Count}) ====");
        for (int i = 0; i < playerConfigs.Count; i++)
        {
            var p = playerConfigs[i];
            Debug.Log($"[PCM] #{i} -> PlayerIndex={p.PlayerIndex}, SelectedPrefab={(p.SelectedCharacterPrefab ? p.SelectedCharacterPrefab.name : "null")}, isReady={p.isReady}, InputName={(p.Input ? p.Input.name : "null")}");
        }
        Debug.Log("[PCM] =====================================");
    }

    // 幫助你在 Editor 測試時清除記錄（非必要）
    public void ClearConfigs()
    {
        playerConfigs.Clear();
        Debug.Log("[PCM] playerConfigs cleared");
    }
}

public class PlayerConfiguration
{
    public PlayerConfiguration(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;
        Input = pi;
        isReady = false;
        SelectedCharacterPrefab = null;
    }

    public PlayerInput Input { get; private set; }
    public int PlayerIndex { get; private set; }
    public bool isReady { get; set; }
    public GameObject SelectedCharacterPrefab { get; set; }
}