using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerSelector : MonoBehaviour
{
    [SerializeField] private Player1 player1Controller;
    [SerializeField] private Player2 player2Controller;

    private void Awake()
    {
        int random = UnityEngine.Random.Range(1, 3);

        if (random == 1)
        {
            player1Controller.enabled = true;
            player2Controller.enabled = false;
            Debug.Log("分配到 Player1 控制邏輯");
        }
        else
        {
            player1Controller.enabled = false;
            player2Controller.enabled = true;
            Debug.Log("分配到 Player2 控制邏輯");
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        player1Controller?.OnMove(context);
        player2Controller?.OnMove(context);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        player1Controller?.OnJump(context);
        player2Controller?.OnJump(context);
    }
}