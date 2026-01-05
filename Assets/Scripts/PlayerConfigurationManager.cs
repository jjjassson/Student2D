using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void HandlePlayerJoin(PlayerInput pi)
    {
        if (pi == null) return;

        // ?????? 【新增防火牆邏輯】 ??????
        // 檢查 GridRoundManager 是否存在。如果存在，代表現在是「遊戲進行中」。
        // 在遊戲進行中，不應該有"新"的玩家加入。
        // 如果這時候有新的 PlayerInput 觸發 (通常是 Replay 鬼魂)，我們直接殺掉它的 Input。
        if (GridRoundManager.Instance != null)
        {
            // 檢查這個 Input 的 Index 是否已經在我們的名單內
            // 如果不在名單內，代表它是外來者 (鬼魂)
            bool isExistingPlayer = playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex);

            if (!isExistingPlayer)
            {
                Debug.LogWarning($"[系統攔截] 偵測到遊戲中生成了帶有 PlayerInput 的物件: {pi.gameObject.name}。已移除 Input 元件以防止控制器錯亂。");

                // 1. 關閉 Input 以防萬一
                pi.DeactivateInput();

                // 2. 銷毀該物件身上的 PlayerInput 元件 (保留物件本身，只殺 Input)
                Destroy(pi);

                // 3. 直接返回，不執行後面的加入邏輯
                return;
            }
        }
        // ?????? ----------------------- ??????


        // 如果該索引的玩家已存在，就不重複處理
        if (playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
            return;

        // 建立設定檔 (這裡會儲存重要的設備資訊)
        var newConfig = new PlayerConfiguration(pi);

        // 指派預設 prefab（如果有設定）
        if (pi.playerIndex >= 0 && pi.playerIndex < defaultPrefabs.Count && defaultPrefabs[pi.playerIndex] != null)
        {
            newConfig.SelectedCharacterPrefab = defaultPrefabs[pi.playerIndex];
        }

        playerConfigs.Add(newConfig);
        Debug.Log($"玩家 {newConfig.PlayerIndex} 加入。設備: {newConfig.Device?.displayName}, 方案: {newConfig.ControlScheme}");
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
            // 嘗試獲取地圖名稱，如果 AllGameManager 不存在則忽略
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

        // 1. 儲存硬體設備 (例如手把或鍵盤)
        // 這是解決跨場景控制器遺失或互換的關鍵
        if (pi.devices.Count > 0)
        {
            Device = pi.devices[0];
        }

        // 2. 儲存控制方案名稱 (例如 "Gamepad", "Keyboard")
        ControlScheme = pi.currentControlScheme;

        isReady = false;
        SelectedCharacterPrefab = null;
    }

    public int PlayerIndex { get; private set; }

    // 儲存 InputDevice 物件，這是不會被銷毀的全域參照
    public InputDevice Device { get; private set; }

    // 儲存方案名稱字串
    public string ControlScheme { get; private set; }

    public bool isReady { get; set; }
    public GameObject SelectedCharacterPrefab { get; set; }
}