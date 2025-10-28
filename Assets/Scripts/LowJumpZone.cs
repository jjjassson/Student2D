using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowJumpZone : MonoBehaviour
{
    [Tooltip("進入時的跳躍倍率 (例如 0.5 = 跳躍高度減半)")]
    public float jumpMultiplier = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null)
        {
            player.SetJumpMultiplier(jumpMultiplier);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null)
        {
            player.SetJumpMultiplier(1f); // 離開後回復正常
        }
    }
}

