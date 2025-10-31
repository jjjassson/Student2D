using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [SerializeField] private float slowMultiplier = 0.5f; // 減速倍率
    private Player1 player; // 你的玩家腳本

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null) return;

        if (hit.collider.CompareTag("Player"))
        {
            // 嘗試取得 Player 腳本
            if (player == null)
                player = hit.collider.GetComponent<Player1>();

            if (player != null && !player.isSlowed)
            {
                player.moveSpeed *= slowMultiplier;
                player.isSlowed = true;
                Debug.Log("踩到減速區！");
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && player != null)
        {
            player.ResetSpeed();
            Debug.Log("離開減速區，速度恢復");
        }
    }
}
