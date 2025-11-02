using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LowJumpZone : MonoBehaviour
{
    [Tooltip("跳躍力倍率（例如 0.5 = 減半）")]
    [Range(0.1f, 2f)]
    public float jumpMultiplier = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null)
        {
            player.ApplyJumpMultiplier(jumpMultiplier);
            Debug.Log("▶ 進入低跳區");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null)
        {
            player.ResetJump();
            Debug.Log("◀ 離開低跳區");
        }
    }
}
