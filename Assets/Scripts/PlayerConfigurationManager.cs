using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// PlayerConfigurationManager
/// - 管理玩家加入與選角
/// - 當所有玩家準備完成時自動載入 AllGameManager.selectedMapName
/// </summary>
public class PlayerConfigurationManager : MonoBehaviour
{
    public static PlayerConfigurationManager Instance { get; private set; }

    private List<PlayerConfiguration> playerConfigs = new List<PlayerConfiguration>();
    [SerializeField] private int MaxPlayers = 2;

    [Header("Optional: default prefabs per playerIndex (0 = Player1, 1 = Player2)")]
    [SerializeField] private List<GameObject> defaultPrefabs = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("[PCM] Second instance detected!");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
            Debug.Log("[PCM] Awake -> instance set, DontDestroyOnLoad");
        }
    }

    /// <summary>
    /// 當 PlayerInputManager 加入玩家時呼叫
    /// </summary>
    public void HandlePlayerJoin(PlayerInput pi)
    {
        if (pi == null)
        {
            Debug.LogError("[PCM] HandlePlayerJoin received null PlayerInput!");
            return;
        }

        Debug.Log($"[PCM] HandlePlayerJoin called. pi.playerIndex = {pi.playerIndex}, pi.name = {pi.name}");

        if (playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
        {
            Debug.LogWarning($"[PCM] Player with index {pi.playerIndex} already exists. Skipping add.");
            return;
        }

        var newConfig = new PlayerConfiguration(pi);

        // 指派預設 prefab（如果有設定）
        if (pi.playerIndex >= 0 && pi.playerIndex < defaultPrefabs.Count && defaultPrefabs[pi.playerIndex] != null)
        {
            newConfig.SelectedCharacterPrefab = defaultPrefabs[pi.playerIndex];
            Debug.Log($"[PCM] Assigned default prefab to playerIndex {pi.playerIndex}: {defaultPrefabs[pi.playerIndex].name}");
        }

        playerConfigs.Add(newConfig);
        Debug.Log($"[PCM] Added PlayerConfiguration: index={newConfig.PlayerIndex} totalCount={playerConfigs.Count}");

        DebugPrintConfigs();
    }

    /// <summary>
    /// 在選角畫面手動設定玩家角色
    /// </summary>
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

    /// <summary>
    /// 玩家按 Ready
    /// </summary>
    public void ReadyPlayer(int index)
    {
        if (index < 0 || index >= playerConfigs.Count)
        {
            Debug.LogWarning($"[PCM] ReadyPlayer index out of range: {index}");
            return;
        }

        playerConfigs[index].isReady = true;
        Debug.Log($"[PCM] Player {index} isReady = true");

        // 檢查所有玩家是否準備完成
        if (playerConfigs.Count == MaxPlayers && playerConfigs.All(p => p.isReady))
        {
            string selectedMap = AllGameManager.Instance.selectedMapName;

            if (!string.IsNullOrEmpty(selectedMap))
            {
                Debug.Log($"[PCM] All players ready. Loading selected map: {selectedMap}");
                SceneManager.LoadScene(selectedMap);
            }
            else
            {
                Debug.LogWarning("[PCM] No map selected, defaulting to 'Cliff'");
                SceneManager.LoadScene("Cliff");
            }
        }
    }

    /// <summary>
    /// 取得目前玩家設定
    /// </summary>
    public List<PlayerConfiguration> GetPlayerConfigs() => playerConfigs;

    /// <summary>
    /// 印出所有玩家狀態（Debug）
    /// </summary>
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

    /// <summary>
    /// 清除玩家設定（測試用）
    /// </summary>
    public void ClearConfigs()
    {
        playerConfigs.Clear();
        Debug.Log("[PCM] playerConfigs cleared");
    }
}

/// <summary>
/// 單一玩家設定
/// </summary>
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