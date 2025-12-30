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
            // 1. 生成點判斷邏輯 (已擴充所有角色)
            // ====================================================
            Transform targetSpawnPoint;

            // 檢查是否為 Player 1 系列的角色
            if (prefab.GetComponent<Player1>() != null ||
                prefab.GetComponent<Drunk1>() != null ||
                prefab.GetComponent<Liver1>() != null ||
                prefab.GetComponent<Love1>() != null ||
                prefab.GetComponent<Lung1>() != null ||
                prefab.GetComponent<Student1>() != null ||
                prefab.GetComponent<Triangle1>() != null)
            {
                targetSpawnPoint = player1Spawn;
            }
            // 檢查是否為 Player 2 系列的角色
            else if (prefab.GetComponent<Player2>() != null ||
                     prefab.GetComponent<Drunk2>() != null ||
                     prefab.GetComponent<Liver2>() != null ||
                     prefab.GetComponent<Love2>() != null ||
                     prefab.GetComponent<Lung2>() != null ||
                     prefab.GetComponent<Student2>() != null ||
                     prefab.GetComponent<Triangle2>() != null)
            {
                targetSpawnPoint = player2Spawn;
            }
            else
            {
                // 預設 fallback (如果都不是，預設給 P1)
                targetSpawnPoint = player1Spawn;
                Debug.LogWarning($"⚠️ 無法識別角色類型: {prefab.name}，將預設生成在 Player1 位置");
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