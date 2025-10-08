using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [Tooltip("�i�J�ɪ����t���v (�Ҧp 0.5 = ��b)")]
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
            player.SetSpeedMultiplier(1f); // ���}��^�_���`
        }
    }
}

