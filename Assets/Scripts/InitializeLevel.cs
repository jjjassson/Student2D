using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InitializeLevel : MonoBehaviour
{
    [Header("Group 1 Spawn Points (1, 3, 5, 7)")]
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player3Spawn;
    [SerializeField] private Transform player5Spawn;
    [SerializeField] private Transform player7Spawn;

    [Header("Group 2 Spawn Points (2, 4, 6, 8)")]
    [SerializeField] private Transform player2Spawn;
    [SerializeField] private Transform player4Spawn;
    [SerializeField] private Transform player6Spawn;
    [SerializeField] private Transform player8Spawn;

    private void Awake()
    {
        if (PlayerConfigurationManager.Instance == null)
        {
            Debug.LogError("❌ PlayerConfigurationManager 尚未實例化！無法生成玩家。");
            return;
        }

        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();

        // 1. 建立生成點陣列，方便依序取用
        Transform[] group1Spawns = new Transform[] { player1Spawn, player3Spawn, player5Spawn, player7Spawn };
        Transform[] group2Spawns = new Transform[] { player2Spawn, player4Spawn, player6Spawn, player8Spawn };

        // 2. 建立計數器，紀錄各組目前分配到第幾個位置
        int group1Index = 0;
        int group2Index = 0;

        foreach (var config in playerConfigs)
        {
            GameObject prefab = config.SelectedCharacterPrefab;
            if (prefab == null) continue;

            Transform targetSpawnPoint;

            // ====================================================
            // 🔥 3. 陣營判斷與依序分配邏輯
            // ====================================================

            // 判斷是否為 Group 1 (1, 3, 5, 7)
            if (prefab.GetComponent<Player1>() != null ||
                prefab.GetComponent<Drunk1>() != null ||
                prefab.GetComponent<Liver1>() != null ||
                prefab.GetComponent<Love1>() != null ||
                prefab.GetComponent<Lung1>() != null ||
                prefab.GetComponent<Student1>() != null ||
                prefab.GetComponent<Triangle1>() != null)
            {
                // 從 Group 1 陣列中取出當前的位置
                // 使用 % 運算符是為了防止溢位（例如超過4人時回到第1個點，雖然你可能限制人數，但這是保險）
                targetSpawnPoint = group1Spawns[group1Index % group1Spawns.Length];

                // 讓下一個 Group 1 的人去下一個點
                group1Index++;
            }
            // 判斷是否為 Group 2 (2, 4, 6, 8)
            else if (prefab.GetComponent<Player2>() != null ||
                     prefab.GetComponent<Drunk2>() != null ||
                     prefab.GetComponent<Liver2>() != null ||
                     prefab.GetComponent<Love2>() != null ||
                     prefab.GetComponent<Lung2>() != null ||
                     prefab.GetComponent<Student2>() != null ||
                     prefab.GetComponent<Triangle2>() != null)
            {
                // 從 Group 2 陣列中取出當前的位置
                targetSpawnPoint = group2Spawns[group2Index % group2Spawns.Length];

                // 讓下一個 Group 2 的人去下一個點
                group2Index++;
            }
            else
            {
                // 防呆：如果都不是，預設丟到 Group 1 的下一個空位
                Debug.LogWarning($"⚠️ 未知角色 {prefab.name}，預設分配至 Group 1");
                targetSpawnPoint = group1Spawns[group1Index % group1Spawns.Length];
                group1Index++;
            }

            // ====================================================
            // 4. 生成並綁定設備
            // ====================================================
            var playerInstance = PlayerInput.Instantiate(
                prefab,
                controlScheme: config.ControlScheme,
                pairWithDevice: config.Device
            );

            // ====================================================
            // 5. 確保 CharacterController 關閉後移動，避免物理彈飛
            // ====================================================
            CharacterController charController = playerInstance.GetComponent<CharacterController>();

            if (charController != null) charController.enabled = false;

            playerInstance.transform.position = targetSpawnPoint.position;
            playerInstance.transform.rotation = targetSpawnPoint.rotation;

            if (charController != null) charController.enabled = true;
            // ====================================================

            playerInstance.name = $"{prefab.name} (P{config.PlayerIndex})";
            playerInstance.neverAutoSwitchControlSchemes = true;

            // 設定分數初始點
            PlayerScore playerScore = playerInstance.GetComponent<PlayerScore>();
            if (playerScore != null)
            {
                playerScore.SetInitialSpawn(targetSpawnPoint);
            }

            Debug.Log($"✅ {playerInstance.name} 生成於 {targetSpawnPoint.name}");
        }
    }
}