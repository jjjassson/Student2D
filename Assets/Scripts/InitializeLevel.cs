using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InitializeLevel : MonoBehaviour
{
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    // 🔥 改為 Awake：確保在 RoundManager 執行 Start 前，玩家已經生好
    private void Awake()
    {
        // 檢查 Manager 是否存在
        if (PlayerConfigurationManager.Instance == null)
        {
            Debug.LogError("❌ PlayerConfigurationManager 尚未實例化！無法生成玩家。");
            return;
        }

        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();

        foreach (var config in playerConfigs)
        {
            GameObject prefab = config.SelectedCharacterPrefab;
            if (prefab == null) continue;

            // ====================================================
            // 1. 保留你原本的生成點判斷邏輯
            // ====================================================
            Transform targetSpawnPoint;

            if (prefab.GetComponent<Player1>() != null)
            {
                targetSpawnPoint = player1Spawn;
            }

            else if (prefab.GetComponent<Player2>() != null)
            {
                targetSpawnPoint = player2Spawn;
            }
            else
            {
                // 預設 fallback
                targetSpawnPoint = player1Spawn;
            }

            // ====================================================
            // 2. 核心修正：使用 PlayerInput.Instantiate 綁定設備
            // ====================================================
            // 使用這個方法可以同時生成物件 + 強制綁定正確的手把
            var playerInstance = PlayerInput.Instantiate(
                prefab,
                controlScheme: config.ControlScheme, // 鎖定方案 (Gamepad/Keyboard)
                pairWithDevice: config.Device        // 鎖定硬體設備 (避免互換的關鍵)
            );

            // 3. 設定位置與角度 (將生成的角色移到你判斷出的位置)
            playerInstance.transform.position = targetSpawnPoint.position;
            playerInstance.transform.rotation = targetSpawnPoint.rotation;

            playerInstance.name = $"{prefab.name} (P{config.PlayerIndex})";
            playerInstance.neverAutoSwitchControlSchemes = true;

            // 4. 設定分數初始點 (保留原本邏輯)
            PlayerScore playerScore = playerInstance.GetComponent<PlayerScore>();
            if (playerScore != null)
            {
                playerScore.SetInitialSpawn(targetSpawnPoint);
            }

            Debug.Log($"✅ {playerInstance.name} 生成於 {targetSpawnPoint.name}。綁定設備: {config.Device?.displayName}");
        }
    }
}