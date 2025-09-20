using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerConfiguration playerConfig;
    private Controls controls;

    private void Awake()
    {
        controls = new Controls();
        controls.GamePlay.Enable(); // ±Ò¥Î Action Map
    }

    public void InitializePlayer(PlayerConfiguration config)
    {
        playerConfig = config;
        config.Input.onActionTriggered += Input_onActionTriggered;
    }

    private void Input_onActionTriggered(CallbackContext context)
    {
        if (context.action == controls.GamePlay.Movement)
        {
            OnMove(context);
        }
    }

    private void OnMove(CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        IMover mover = GetComponents<MonoBehaviour>().OfType<IMover>().FirstOrDefault();
        if (mover != null)
        {
            mover.SetInputVector(input);
        }
    }
}