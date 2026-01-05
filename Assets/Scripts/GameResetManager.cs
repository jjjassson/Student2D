using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResetManager : MonoBehaviour
{
    // 請在 Inspector 中輸入你的主選單場景名稱，例如 "MainMenu" 或 "StartScene"
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void ReturnToMainMenu()
    {
        Debug.Log("正在清除所有遊戲記憶與管理器...");

        // 1. 銷毀玩家配置管理器 (清除手把配對、玩家人數)
        // 這是最重要的一步，因為它紀錄了 PlayerInput 和 Device
        if (PlayerConfigurationManager.Instance != null)
        {
            Destroy(PlayerConfigurationManager.Instance.gameObject);
        }

        // 2. 銷毀全域遊戲管理器 (清除地圖選擇、回合設定)
        if (AllGameManager.Instance != null)
        {
            Destroy(AllGameManager.Instance.gameObject);
        }

        // 3. 銷毀回合管理器 (雖然通常它是跟隨場景的，但為了保險起見也殺掉)
        if (GridRoundManager.Instance != null)
        {
            Destroy(GridRoundManager.Instance.gameObject);
        }

        // 4. 載入主選單
        // 因為管理器都被殺了，主選單裡的 Awake() 會判定 Instance 為 null
        // 進而建立全新的 Instance，就像第一次開啟遊戲一樣
        SceneManager.LoadScene(mainMenuSceneName);
    }
}