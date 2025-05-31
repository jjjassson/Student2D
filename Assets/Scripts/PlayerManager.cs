using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private void OnEnable()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
    }

    private void OnDisable()
    {
        PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        int index = playerInput.playerIndex;

        Debug.Log($"Player {index + 1} joined.");

        // 切換控制方案或腳本
        if (index == 0)
        {
            playerInput.SwitchCurrentActionMap("Player1Controls");
            playerInput.gameObject.AddComponent<Player1>();
        }
        else if (index == 1)
        {
            playerInput.SwitchCurrentActionMap("Player2Controls");
            playerInput.gameObject.AddComponent<Player2>();
        }
        else
        {
            Debug.LogWarning("Only 2 players supported.");
            Destroy(playerInput.gameObject);
        }
    }
}
