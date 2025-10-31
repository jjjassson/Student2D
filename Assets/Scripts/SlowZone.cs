using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [SerializeField] private float slowMultiplier = 0.5f; // ��t���v
    private Player1 player; // �A�����a�}��

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null) return;

        if (hit.collider.CompareTag("Player"))
        {
            // ���ը��o Player �}��
            if (player == null)
                player = hit.collider.GetComponent<Player1>();

            if (player != null && !player.isSlowed)
            {
                player.moveSpeed *= slowMultiplier;
                player.isSlowed = true;
                Debug.Log("����t�ϡI");
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && player != null)
        {
            player.ResetSpeed();
            Debug.Log("���}��t�ϡA�t�׫�_");
        }
    }
}
