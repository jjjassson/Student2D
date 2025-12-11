using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    private IMover mover;

    private void Awake()
    {
        // 預先抓取 IMover，不需要每次移動都抓一次，提升效能
        mover = GetComponents<MonoBehaviour>().OfType<IMover>().FirstOrDefault();
    }

    // 這個方法必須由 InitializeLevel 呼叫
    public void InitializePlayer(PlayerConfiguration config)
    {
        // 修正重點：不再使用 config.Input (因為已經刪除了)
        // 而是直接獲取掛在自己身上的 PlayerInput 組件
        // 這個組件在 InitializeLevel 中已經被強制綁定到正確的手把上了
        var playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            // 訂閱事件：使用 PlayerInput 組件的通用事件
            playerInput.onActionTriggered += Input_onActionTriggered;
        }
        else
        {
            Debug.LogError("PlayerInputHandler: 找不到 PlayerInput 組件！");
        }
    }

    private void Input_onActionTriggered(CallbackContext context)
    {
        // 判斷觸發的是哪個動作
        // 請確保你的 Input Actions 裡面的 Action 名稱叫做 "Movement" (大小寫需一致)
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