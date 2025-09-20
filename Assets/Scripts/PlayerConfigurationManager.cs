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

    private void Awake()
    {
        if (Instance != null)
            Debug.LogWarning("[Singleton] Second instance of PlayerConfigurationManager!");
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public void HandlePlayerJoin(PlayerInput pi)
    {
        if (!playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
            playerConfigs.Add(new PlayerConfiguration(pi));
    }

    public void SetPlayerCharacterPrefab(int index, GameObject prefab)
    {
        if (index >= 0 && index < playerConfigs.Count)
            playerConfigs[index].SelectedCharacterPrefab = prefab;
    }

    public List<PlayerConfiguration> GetPlayerConfigs() => playerConfigs;

    public void ReadyPlayer(int index)
    {
        playerConfigs[index].isReady = true;
        if (playerConfigs.Count == MaxPlayers && playerConfigs.All(p => p.isReady))
        {
            SceneManager.LoadScene("Cliff"); // 替換成你的遊戲場景
        }
    }
}

public class PlayerConfiguration
{
    public PlayerConfiguration(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;
        Input = pi;
    }

    public PlayerInput Input { get; private set; }
    public int PlayerIndex { get; private set; }
    public bool isReady { get; set; }
    public GameObject SelectedCharacterPrefab { get; set; }
}