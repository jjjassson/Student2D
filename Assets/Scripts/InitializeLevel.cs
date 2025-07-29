using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InitializeLevel : MonoBehaviour
{
    [SerializeField]
    private Transform[] playerSpawns;

    void Start()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();

        for (int i = 0; i < playerConfigs.Length; i++)
        {
            var config = playerConfigs[i];

            // 使用每位玩家選擇的 prefab
            GameObject playerPrefab = config.SelectedCharacterPrefab;

            if (playerPrefab != null)
            {
                // 實例化 prefab
                var playerObject = Instantiate(playerPrefab, playerSpawns[i].position, playerSpawns[i].rotation);

                // 把對應的 PlayerInput 移到新 prefab 上
                PlayerInput originalInput = config.Input;
                originalInput.transform.SetParent(null);
                originalInput.transform.position = playerSpawns[i].position;

                // 把 PlayerInput 重綁到新的 prefab（如果你 prefab 有控制器需要 Input）
                originalInput.SwitchCurrentControlScheme(originalInput.currentControlScheme, originalInput.devices.ToArray());

                // 把 Input 移到 prefab 上（可選，看你設計邏輯）
                originalInput.gameObject.SetActive(false); // 關掉原本的 placeholder
                originalInput.transform.SetParent(playerObject.transform, true);
                originalInput.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError($"Player {i} 沒有選角色 prefab！");
            }
        }
    }
}
