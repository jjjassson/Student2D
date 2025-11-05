using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InitializeLevel : MonoBehaviour
{
    [SerializeField] private Transform[] playerSpawns;

    private void Start()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();

        foreach (var config in playerConfigs)
        {
            int playerIndex = config.PlayerIndex; // 🔹 玩家編號（0 = Player1, 1 = Player2）
            if (playerIndex >= playerSpawns.Length)
            {
                Debug.LogWarning($"Player {playerIndex} 沒有對應的 Spawn 點！");
                continue;
            }

            GameObject prefab = config.SelectedCharacterPrefab;
            if (prefab == null)
            {
                Debug.LogError($"Player {playerIndex} 沒有選角色 prefab！");
                continue;
            }

            // 🔹 生成玩家在對應 Spawn
            GameObject playerObject = Instantiate(
                prefab,
                playerSpawns[playerIndex].position,
                playerSpawns[playerIndex].rotation
            );

            // 綁定控制器
            PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                foreach (var device in config.Input.devices)
                {
                    InputUser.PerformPairingWithDevice(device, playerInput.user);
                }

                playerInput.user.AssociateActionsWithUser(playerInput.actions);
            }

            // 綁定 PlayerInputHandler
            var inputHandler = playerObject.GetComponent<PlayerInputHandler>();
            if (inputHandler != null)
            {
                inputHandler.InitializePlayer(config);
            }

            // 🔹 綁定 PlayerScore 的初始 Spawn
            var playerScore = playerObject.GetComponent<PlayerScore>();
            if (playerScore != null)
            {
                playerScore.SetInitialSpawn(playerSpawns[playerIndex]);
            }

            Debug.Log($"✅ Player {playerIndex + 1} 生成於 {playerSpawns[playerIndex].name}");
        }
    }
}