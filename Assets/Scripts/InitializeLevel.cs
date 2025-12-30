using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InitializeLevel : MonoBehaviour
{
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    private void Awake()
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
            if (prefab == null) continue;

            // 1. 生成點判斷邏輯
            Transform targetSpawnPoint;

            // 判斷 Player 1 陣營
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
            // 判斷 Player 2 陣營
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
                targetSpawnPoint = player1Spawn;
            }

            // 2. 生成並綁定設備
            var playerInstance = PlayerInput.Instantiate(
                prefab,
                controlScheme: config.ControlScheme,
                pairWithDevice: config.Device
            );

            // ====================================================
            // 🔥 3. 修正：暫時關閉 CharacterController 以確保移動成功
            // ====================================================
            CharacterController charController = playerInstance.GetComponent<CharacterController>();

            // 如果有 CharacterController，先關掉
            if (charController != null)
            {
                charController.enabled = false;
            }

            // 設定位置與角度
            playerInstance.transform.position = targetSpawnPoint.position;
            playerInstance.transform.rotation = targetSpawnPoint.rotation;

            // 移動完後，再打開
            if (charController != null)
            {
                charController.enabled = true;
            }
            // ====================================================

            playerInstance.name = $"{prefab.name} (P{config.PlayerIndex})";
            playerInstance.neverAutoSwitchControlSchemes = true;

            // 4. 設定分數初始點
            PlayerScore playerScore = playerInstance.GetComponent<PlayerScore>();
            if (playerScore != null)
            {
                playerScore.SetInitialSpawn(targetSpawnPoint);
            }

            Debug.Log($"✅ {playerInstance.name} 生成於 {targetSpawnPoint.name}");
        }
    }
}