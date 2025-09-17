using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private PlayerScore score;

    private void Start()
    {
        score = GetComponent<PlayerScore>();
        if (score == null)
            score = gameObject.AddComponent<PlayerScore>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            score.ReachGoal();

            // �^�X����
            RoundManager.Instance.EndRound();

            // �i��G�⪱�a�Ԧ^�_�I
            transform.position = new Vector3(0, 2f, 0);
        }
        else if (other.CompareTag("DeathZone"))
        {
            score.FallDown();
            transform.position = new Vector3(0, 2f, 0);
        }
    }
}