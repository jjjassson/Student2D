using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowJumpZone : MonoBehaviour
{
    [Tooltip("�i�J�ɪ����D���v (�Ҧp 0.5 = ���D���״�b)")]
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
            player.SetJumpMultiplier(1f); // ���}��^�_���`
        }
    }
}

