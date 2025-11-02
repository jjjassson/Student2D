using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SlowZone : MonoBehaviour
{
    [Tooltip("角色速度倍率（例如 0.5 = 減半）")]
    [Range(0.1f, 2f)]
    public float slowMultiplier = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null)
        {
            player.ApplySpeedMultiplier(slowMultiplier);
            Debug.Log("▶ 進入減速區");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null)
        {
            player.ResetSpeed();
            Debug.Log("◀ 離開減速區");
        }
    }
}
