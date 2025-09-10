using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InitializeLevel : MonoBehaviour
{
    [SerializeField] private Transform[] playerSpawns; // 玩家生成點

    private void Start()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();

        for (int i = 0; i < playerConfigs.Length; i++)
        {
            var config = playerConfigs[i];
            GameObject prefab = config.SelectedCharacterPrefab;

            if (prefab == null)
            {
                Debug.LogError($"Player {i} 沒有選角色 prefab！");
                continue;
            }

            // Instantiate 角色 prefab
            GameObject playerObject = Instantiate(prefab, playerSpawns[i].position, playerSpawns[i].rotation);

            // 取得角色 prefab 上的 PlayerInput
            PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();

            if (playerInput == null)
            {
                Debug.LogError("角色 prefab 必須有 PlayerInput！");
                continue;
            }

            // 綁定玩家原本使用的控制器
            foreach (var device in config.Input.devices)
            {
                InputUser.PerformPairingWithDevice(device, playerInput.user);
            }

            // 設定 playerIndex
            playerInput.user.AssociateActionsWithUser(playerInput.actions);
        }
    }
}