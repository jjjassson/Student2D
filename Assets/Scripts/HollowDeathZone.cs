using UnityEngine;

public class HollowDeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Player1 player1 = other.GetComponent<Player1>();
        if (player1 != null)
        {
            HandlePlayer1Death(player1);
            return;
        }

        Player2 player2 = other.GetComponent<Player2>();
        if (player2 != null)
        {
            HandlePlayer2Death(player2);
            return;
        }
    }

    private void HandlePlayer1Death(Player1 player)
    {
        var method = player.GetType().GetMethod("DieAndRespawn");
        if (method != null)
        {
            method.Invoke(player, null);
        }
        else
        {
            Debug.LogWarning("Player1 尚未實作 DieAndRespawn()，改用傳送");
            player.transform.position = Vector3.zero;
        }
    }

    private void HandlePlayer2Death(Player2 player)
    {
        var method = player.GetType().GetMethod("DieAndRespawn");
        if (method != null)
        {
            method.Invoke(player, null);
        }
        else
        {
            Debug.LogWarning("Player2 尚未實作 DieAndRespawn()，改用傳送");
            player.transform.position = Vector3.zero;
        }
    }
}