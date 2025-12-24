using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    // 定義一個簡單的類別來存 UI 元件
    [System.Serializable]
    public class PlayerSlot
    {
        public Image itemIcon;   // 顯示抽到的物品
        public Image avatarIcon; // (選用) 顯示玩家頭像 P1, P2...
    }

    // 在 Inspector 裡把 4 個格子的物件拖進來
    public PlayerSlot[] playerSlots;

    // 這是單例模式，讓 PlayerInventory 容易找到它
    public static GameUIManager Instance;

    void Awake()
    {
        Instance = this;
    }

    // 供 PlayerInventory 呼叫更新畫面
    public void UpdatePlayerItemUI(int playerIndex, Sprite icon)
    {
        // 防呆：確保 index 沒有超過陣列範圍 (例如 playerIndex 是 0~3)
        if (playerIndex >= 0 && playerIndex < playerSlots.Length)
        {
            if (playerSlots[playerIndex].itemIcon != null)
            {
                playerSlots[playerIndex].itemIcon.sprite = icon;
                // 如果 icon 是空的就隱藏圖片，有圖就顯示
                playerSlots[playerIndex].itemIcon.enabled = (icon != null);
            }
        }
    }
}