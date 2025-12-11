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

            // 1. 決定 Spawn 點
            Transform spawnPoint;
            if (prefab.GetComponent<Player1>() != null)
                spawnPoint = player1Spawn;
            else if (prefab.GetComponent<Player2>() != null)
                spawnPoint = player2Spawn;
            else
                spawnPoint = player1Spawn;

            // 2. 生成玩家物件
            GameObject playerObject = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            playerObject.name = $"{prefab.name} (P{config.PlayerIndex})";

            // 設定分數初始點 (依據你的專案需求)
            PlayerScore playerScore = playerObject.GetComponent<PlayerScore>();
            if (playerScore != null)
            {
                playerScore.SetInitialSpawn(spawnPoint);
            }

            // ====================================================
            // 🔴 3. 核心修正：綁定設備並鎖定方案
            // ====================================================
            PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();

            if (playerInput != null)
            {
                // A. 先暫停輸入，避免設定時觸發不必要的事件
                playerInput.DeactivateInput();

                // B. 關鍵設定：禁止自動切換方案
                // 這能防止 "碰到鍵盤後，控制器就失效" 或 "控制器跑到另一位玩家身上"
                playerInput.neverAutoSwitchControlSchemes = true;

                // C. 強制綁定從 Menu 帶過來的設備 (解決 CS1061 錯誤的地方)
                // 我們使用 config.Device (硬體) 而不是 config.Input (組件)
                if (config.Device != null)
                {
                    // SwitchCurrentControlScheme 會同時做三件事：
                    // 1. 設定方案名稱 (例如 "Gamepad")
                    // 2. 強制配對該 Device
                    // 3. 剔除其他不相關的設備
                    playerInput.SwitchCurrentControlScheme(config.ControlScheme, config.Device);
                }
                else
                {
                    Debug.LogWarning($"P{config.PlayerIndex} 沒有偵測到明確設備，將使用預設設定。");
                }

                // D. 恢復輸入
                playerInput.ActivateInput();
            }

            Debug.Log($"✅ {playerObject.name} 生成完畢。綁定設備: {config.Device?.displayName}");
        }
    }
}