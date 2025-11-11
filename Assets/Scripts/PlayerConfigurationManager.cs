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

        if (playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
            return;

        var newConfig = new PlayerConfiguration(pi);

        // 指派預設 prefab（如果有設定）
        if (pi.playerIndex >= 0 && pi.playerIndex < defaultPrefabs.Count && defaultPrefabs[pi.playerIndex] != null)
        {
            newConfig.SelectedCharacterPrefab = defaultPrefabs[pi.playerIndex];
        }

        playerConfigs.Add(newConfig);
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
            string selectedMap = AllGameManager.Instance.selectedMapName;

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