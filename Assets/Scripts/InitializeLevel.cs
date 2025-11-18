using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InitializeLevel : MonoBehaviour
{
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    private void Start()
    {
        if (PlayerConfigurationManager.Instance == null)
        {
            Debug.LogError("❌ PlayerConfigurationManager 尚未實例化！無法生成玩家。");
            return;
        }

        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();

        foreach (var config in playerConfigs)
        {
            GameObject prefab = config.SelectedCharacterPrefab;
            if (prefab == null)
            {
                Debug.LogError($"❌ Player {config.PlayerIndex} 沒有選角色 prefab！");
                continue;
            }

            // 1. 🔹 根據角色腳本決定 spawn 點
            Transform spawnPoint;
            if (prefab.GetComponent<Player1>() != null)
                spawnPoint = player1Spawn;
            else if (prefab.GetComponent<Player2>() != null)
                spawnPoint = player2Spawn;
            else
            {
                Debug.LogWarning($"Prefab {prefab.name} 沒有 Player1/Player2 腳本，預設到 player1Spawn");
                spawnPoint = player1Spawn;
            }

            // 2. 🔹 生成玩家
            GameObject playerObject = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            playerObject.name = $"{prefab.name} (P{config.PlayerIndex})";

            // 3. 🔴 關鍵修正：設定 PlayerScore 的初始出生點
            PlayerScore playerScore = playerObject.GetComponent<PlayerScore>();
            if (playerScore != null)
            {
                playerScore.SetInitialSpawn(spawnPoint);
            }
            else
            {
                Debug.LogError($"❌ {playerObject.name} 缺少 PlayerScore 腳本！請確保您的 Prefab 上有此腳本。");
            }

            // 4. 🔹 綁定 PlayerInput
            PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();
            if (playerInput != null && config.Input != null)
            {
                foreach (var device in config.Input.devices)
                    // 這裡執行綁定操作
                    InputUser.PerformPairingWithDevice(device, playerInput.user);
            }

            Debug.Log($"✅ {playerObject.name} 生成於 {spawnPoint.name}");
        }
    }
}