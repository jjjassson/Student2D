using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [Tooltip("進入時的移速倍率 (例如 0.5 = 減半)")]
    public float slowMultiplier = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null)
        {
            player.SetSpeedMultiplier(slowMultiplier);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null)
        {
            player.SetSpeedMultiplier(1f); // 離開後回復正常
        }
    }
}

