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

            // 回合結束
            RoundManager.Instance.EndRound();

            // 可選：把玩家拉回起點
            transform.position = new Vector3(0, 2f, 0);
        }
        else if (other.CompareTag("DeathZone"))
        {
            score.FallDown();
            transform.position = new Vector3(0, 2f, 0);
        }
    }
}