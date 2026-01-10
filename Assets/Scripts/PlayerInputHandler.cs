using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users; // 記得引用這個，才能用 InputUser
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    private IMover mover;

    private void Awake()
    {
        mover = GetComponents<MonoBehaviour>().OfType<IMover>().FirstOrDefault();
    }

    // 這個方法必須由 InitializeLevel 或 復活邏輯 呼叫
    public void InitializePlayer(PlayerConfiguration config)
    {
        var playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            // ==========================================
            // ★★★ 關鍵修正開始：在這裡執行強制綁定 ★★★
            // ==========================================

            // 1. 關閉自動切換，防止 Unity 亂抓正在動的手把
            playerInput.neverAutoSwitchControlSchemes = true;

            // 2. 解除當前任何可能的錯誤配對
            playerInput.user.UnpairDevices();

            // 3. 拿出我們在 Menu 階段存好的「正確設備」(config.Device)
            if (config.Device != null)
            {
                // 強制將這個 PlayerInput 與該設備結婚，至死不渝
                InputUser.PerformPairingWithDevice(config.Device, playerInput.user);

                // 再次確認切換到正確的方案 (例如 "Gamepad")
                if (!string.IsNullOrEmpty(config.ControlScheme))
                {
                    playerInput.user.ActivateControlScheme(config.ControlScheme);
                }

                Debug.Log($"玩家 {config.PlayerIndex} 已強制綁定設備: {config.Device.displayName}");
            }
            else
            {
                Debug.LogError($"玩家 {config.PlayerIndex} 的 Config 中沒有 Device 資訊！");
            }

            // ==========================================
            // ★★★ 關鍵修正結束 ★★★
            // ==========================================

            // 訂閱事件
            playerInput.onActionTriggered += Input_onActionTriggered;
        }
        else
        {
            Debug.LogError("PlayerInputHandler: 找不到 PlayerInput 組件！");
        }
    }

    private void Input_onActionTriggered(CallbackContext context)
    {
        if (context.action.name == "Movement")
        {
            OnMove(context);
        }
    }

    private void OnMove(CallbackContext context)
    {
        if (mover != null)
        {
            Vector2 input = context.ReadValue<Vector2>();
            mover.SetInputVector(input);
        }
    }
}