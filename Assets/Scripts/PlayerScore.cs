using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public int score = 0;
    public bool isAlive = true; // ¬O§_¦s¬¡

    public void ReachGoal()
    {
        score += 1;
        Debug.Log($"{gameObject.name} reached goal! Score: {score}");
        RoundManager.Instance.NotifyPlayerReachedGoal(this);
    }

    public void FallDown()
    {
        score -= 1;
        isAlive = false;
        Debug.Log($"{gameObject.name} fell! Score: {score}");
        RoundManager.Instance.NotifyPlayerDeath(this);
    }

    public void Revive(Vector3 spawnPos)
    {
        isAlive = true;
        transform.position = spawnPos;
        gameObject.SetActive(true);
        Debug.Log($"{gameObject.name} revived!");
    }
}