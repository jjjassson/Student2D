using UnityEngine;

[RequireComponent(typeof(PlayerScore))]
public class PlayerCollision : MonoBehaviour
{
    private PlayerScore score;

    private void Awake()
    {
        // 取得或自動新增 PlayerScore
        score = GetComponent<PlayerScore>();
        if (score == null)
        {
            score = gameObject.AddComponent<PlayerScore>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            // 通知玩家到達終點
            score.ReachGoal();

            // 暫時隱藏玩家，直到下一回合復活
            gameObject.SetActive(false);

            Debug.Log($"{gameObject.name} 到達 Goal，暫時隱藏。");
        }
        else if (other.CompareTag("DeathZone"))
        {
            // 使用公開接口處理掉落死亡
            score.FallDown();

            // 暫時隱藏玩家，直到下一回合復活
            gameObject.SetActive(false);

            Debug.Log($"{gameObject.name} 掉入 DeathZone，暫時隱藏。");
        }
    }
}