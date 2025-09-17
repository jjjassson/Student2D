using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public int score = 0;

    // ���a��F���I
    public void ReachGoal()
    {
        score += 1;
        Debug.Log($"{gameObject.name} reached goal! Score: {score}");
    }

    // ���a����
    public void FallDown()
    {
        score -= 1;
        Debug.Log($"{gameObject.name} fell! Score: {score}");
    }
}