using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class SpawnPlayerSetupMenu : MonoBehaviour
{
    // 在 Inspector 中拖曳要實例化的玩家菜單預製件
    public GameObject playerSetupMenuPrefab;

    // 我們不再依賴 public 欄位或 Inspector 拖曳，而是直接獲取自身（此腳本掛在 PlayerInput Prefab 上）
    private PlayerInput input;

    private void Awake()
    {
        // 1. 獲取自身的 PlayerInput 組件
        input = GetComponent<PlayerInput>();
        if (input == null)
        {
            Debug.LogError("SpawnPlayerSetupMenu: 找不到 PlayerInput 組件！");
            return;
        }

        // 確保找到根 UI 佈局
        GameObject rootMenu = GameObject.Find("MainLayout");

        if (rootMenu != null)
        {
            // 2. 實例化該玩家專屬的菜單
            var menu = Instantiate(playerSetupMenuPrefab, rootMenu.transform);

            // 3. 獲取新菜單上的 UI Input Module
            InputSystemUIInputModule uiModule = menu.GetComponentInChildren<InputSystemUIInputModule>();

            if (uiModule != null)
            {
                // *** 關鍵步驟 A: 強制綁定 PlayerInput 到這個 UI Module ***
                // 這告訴 PlayerInput 組件：你的 UI 輸入事件應該導向這個特定的 UI Module。
                input.uiInputModule = uiModule;

                // (可選但建議) 確保 UI Module 使用這個 PlayerInput 的裝置
                // uiModule.actionsAsset = input.actions; 
            }
            else
            {
                Debug.LogError("SpawnPlayerSetupMenu: 菜單預製件上找不到 InputSystemUIInputModule！");
            }

            // 4. *** 關鍵步驟 B: 將 PlayerIndex 傳遞給 PlayerSetupMenuController ***
            PlayerSetupMenuController menuController = menu.GetComponent<PlayerSetupMenuController>();
            if (menuController != null)
            {
                menuController.SetPlayerIndex(input.playerIndex);
            }
            else
            {
                Debug.LogError("SpawnPlayerSetupMenu: 菜單預製件上找不到 PlayerSetupMenuController！");
            }
        }
    }
}