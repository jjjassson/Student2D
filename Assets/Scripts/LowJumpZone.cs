using UnityEngine;

public class LowJump : MonoBehaviour
{
    [SerializeField] private float jumpMultiplier = 0.5f; // ���D�O�U�����v
    private Player1 player;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null) return;

        if (hit.collider.CompareTag("Player"))
        {
            if (player == null)
                player = hit.collider.GetComponent<Player1>();

            if (player != null && !player.isJumpReduced)
            {
                player.jumpForce *= jumpMultiplier;
                player.isJumpReduced = true;
                Debug.Log("���C���ϡI");
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && player != null)
        {
            player.ResetJump();
            Debug.Log("���}�C���ϡA���D��_");
        }
    }
}
