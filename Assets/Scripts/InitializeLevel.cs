using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InitializeLevel : MonoBehaviour
{
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    private void Start()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();

        foreach (var config in playerConfigs)
        {
            GameObject prefab = config.SelectedCharacterPrefab;
            if (prefab == null)
            {
                Debug.LogError($"❌ Player {config.PlayerIndex} 沒有選角色 prefab！");
                continue;
            }

            // 🔹 根據角色腳本決定 spawn
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

            // 🔹 生成玩家
            GameObject playerObject = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            // 🔹 綁定 PlayerInput
            PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();
            if (playerInput != null && config.Input != null)
            {
                foreach (var device in config.Input.devices)
                    InputUser.PerformPairingWithDevice(device, playerInput.user);
            }

            Debug.Log($"✅ {prefab.name} 生成於 {spawnPoint.name}");
        }
    }
}