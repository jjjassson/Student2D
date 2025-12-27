using UnityEngine;

public class HollowDeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 嘗試抓 Player1
        Player1 player1 = other.GetComponent<Player1>();
        if (player1 != null)
        {
            HandlePlayer1Death(player1);
            return;
        }

        // 如果未來有 Player2
        Player2 player2 = other.GetComponent<Player2>();
        if (player2 != null)
        {
            HandlePlayer2Death(player2);
            return;
        }
    }

    private void HandlePlayer1Death(Player1 player)
    {
        // 如果 Player1 已經有 DieAndRespawn()
        var method = player.GetType().GetMethod("DieAndRespawn");
        if (method != null)
        {
            method.Invoke(player, null);
        }
        else
        {
            // 保底方案（避免你現在還沒加死亡系統）
            Debug.LogWarning("Player1 尚未實作 DieAndRespawn()，已暫時關閉物件");
            player.gameObject.SetActive(false);
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
            Debug.LogWarning("Player2 尚未實作 DieAndRespawn()");
            player.gameObject.SetActive(false);
        }
    }
}
