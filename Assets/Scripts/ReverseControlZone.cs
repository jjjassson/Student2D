using UnityEngine;

public class ReverseControlZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null)
        {
            player.SetReverse(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null)
        {
            player.SetReverse(false);
        }
    }
}
