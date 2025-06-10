using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;


public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private Player1 mover;

    private void Awake()
    {

        playerInput = GetComponent<PlayerInput>();
        var Movers = FindObjectsOfType<Player1>();
        var Index = playerInput.playerIndex;
        mover = GetComponent<Player1>();

    }
    
    public void OnMove(CallbackContext context)
    {
        if (mover != null)
            mover.SetInputVector(context.ReadValue<Vector2>());
    }
}
