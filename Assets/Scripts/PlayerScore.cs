using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public int score = 0;

    // 玩家到達終點
    public void ReachGoal()
    {
        score += 1;
        Debug.Log($"{gameObject.name} reached goal! Score: {score}");
    }

    // 玩家掉落
    public void FallDown()
    {
        score -= 1;
        Debug.Log($"{gameObject.name} fell! Score: {score}");
    }
}