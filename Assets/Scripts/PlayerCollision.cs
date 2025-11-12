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
            gameObject.SetActive(false); // 抵達終點後不再受控制
        }
        else if (other.CompareTag("DeathZone"))
        {
            score.FallDown();
            gameObject.SetActive(false); // 死亡後暫時消失
        }
    }
}