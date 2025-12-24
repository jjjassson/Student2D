using UnityEngine;
using UnityEngine.InputSystem; // 引用 Input System

public class PlayerInventory : MonoBehaviour
{
    [Header("玩家資訊")]
    public int playerIndex = 0; // P1=0, P2=1...

    private GridObjectPlacer placer;

    void Start()
    {
        placer = GetComponent<GridObjectPlacer>();

        // 自動抓取 PlayerInput 的 index (如果你是用 PlayerInputManager 生成的)
        var input = GetComponent<PlayerInput>();
        if (input != null)
        {
            playerIndex = input.playerIndex;
        }
    }

    // 當 RoundManager 發牌時呼叫這個
    public void EquipItem(BuildingData newItem)
    {
        // 1. 設定放置腳本的資料
        if (placer != null)
        {
            placer.SetBuildingData(newItem);
        }

        // 2. 通知 UI 更新圖片
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdatePlayerItemUI(playerIndex, newItem.uiSprite);
        }
    }

    // 當回合開始/結束時，控制能不能動
    public void SetPlacementState(bool canPlace)
    {
        if (placer != null)
        {
            placer.SetPlacementAllowed(canPlace);
        }
    }
}