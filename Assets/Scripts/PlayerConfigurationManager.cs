using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerConfigurationManager : MonoBehaviour
{
    public static PlayerConfigurationManager Instance { get; private set; }

    private List<PlayerConfiguration> playerConfigs = new List<PlayerConfiguration>();

    [SerializeField] private int MaxPlayers = 4; // 這裡預設改為 4，可視需求在 Inspector 調整

    [Header("Optional: default prefabs per playerIndex (0 = Player1, 1 = Player2)")]
    [SerializeField] private List<GameObject> defaultPrefabs = new List<GameObject>();

    [Header("UI Settings")]
    [Tooltip("請將畫面上的『指定加入提示 UI (例如一行文字或圖案)』拖曳到這裡，絕對不要拖到整個 Canvas！")]
    [SerializeField] private GameObject specificJoinUIPrompt;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void HandlePlayerJoin(PlayerInput pi)
    {
        if (pi == null) return;

        // 【防火牆邏輯】：防止遊戲進行中產生鬼魂輸入
        if (GridRoundManager.Instance != null)
        {
            bool isExistingPlayer = playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex);

            if (!isExistingPlayer)
            {
                Debug.LogWarning($"[系統攔截] 偵測到遊戲中生成了帶有 PlayerInput 的物件: {pi.gameObject.name}。已移除 Input 元件以防止控制器錯亂。");
                pi.DeactivateInput();
                Destroy(pi);
                return;
            }
        }

        // 如果該索引的玩家已存在，就不重複處理
        if (playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
            return;

        // 建立設定檔 (儲存硬體設備等重要資訊)
        var newConfig = new PlayerConfiguration(pi);

        if (pi.playerIndex >= 0 && pi.playerIndex < defaultPrefabs.Count && defaultPrefabs[pi.playerIndex] != null)
        {
            newConfig.SelectedCharacterPrefab = defaultPrefabs[pi.playerIndex];
        }

        playerConfigs.Add(newConfig);
        Debug.Log($"玩家 {newConfig.PlayerIndex} 加入。設備: {newConfig.Device?.displayName}, 方案: {newConfig.ControlScheme}");

        // === 核心邏輯：當玩家人數達到 3 人或 4 人時，關閉指定的 UI 提示 ===
        if (playerConfigs.Count >= 1)
        {
            if (specificJoinUIPrompt != null)
            {
                specificJoinUIPrompt.SetActive(false);
                Debug.Log("玩家人數達到 1 人或以上，已關閉指定的加入提示 UI。");
            }
        }
    }

    public void SetPlayerCharacterPrefab(int index, GameObject prefab)
    {
        if (index >= 0 && index < playerConfigs.Count)
        {
            playerConfigs[index].SelectedCharacterPrefab = prefab;
        }
    }

    public void ReadyPlayer(int index)
    {
        if (index < 0 || index >= playerConfigs.Count)
            return;

        playerConfigs[index].isReady = true;

        if (playerConfigs.Count == MaxPlayers && playerConfigs.All(p => p.isReady))
        {
            string selectedMap = null;
            if (AllGameManager.Instance != null)
            {
                selectedMap = AllGameManager.Instance.selectedMapName;
            }

            if (!string.IsNullOrEmpty(selectedMap))
            {
                Debug.Log($"載入地圖：{selectedMap}");
                SceneManager.LoadScene(selectedMap);
            }
            else
            {
                Debug.Log("未選擇地圖，預設載入：Cliff");
                SceneManager.LoadScene("Cliff");
            }
        }
    }

    public List<PlayerConfiguration> GetPlayerConfigs() => playerConfigs;

    public void ClearConfigs() => playerConfigs.Clear();
}

/// <summary>
/// 玩家配置資料類別
/// </summary>
[System.Serializable]
public class PlayerConfiguration
{
    public PlayerConfiguration(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;

        if (pi.devices.Count > 0)
        {
            Device = pi.devices[0];
        }

        ControlScheme = pi.currentControlScheme;
        isReady = false;
        SelectedCharacterPrefab = null;
    }

    public int PlayerIndex { get; private set; }
    public InputDevice Device { get; private set; }
    public string ControlScheme { get; private set; }
    public bool isReady { get; set; }
    public GameObject SelectedCharacterPrefab { get; set; }
}