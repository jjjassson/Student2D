using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    [Header("Player Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    private bool isDead = false;

    public bool isAlive => !isDead;

    private Transform initialSpawn; // 🔹 原始出生點

    private void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    // 記住初始出生點
    public void SetInitialSpawn(Transform spawn)
    {
        initialSpawn = spawn;
    }

    // 受傷
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 死亡
    private void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;

        gameObject.SetActive(false);

        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.NotifyPlayerDeath(this);
        }
    }

    public void FallDown()
    {
        Debug.Log($"{gameObject.name} 掉落死亡！");
        Die();
    }

    // 復活（優先使用原始 Spawn）
    public void Revive()
    {
        isDead = false;
        currentHealth = maxHealth;

        if (initialSpawn != null)
        {
            transform.position = initialSpawn.position;
            transform.rotation = initialSpawn.rotation;
        }

        gameObject.SetActive(true);
        Debug.Log($"{gameObject.name} 復活在初始出生點");
    }

    public void ReachGoal()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.NotifyPlayerReachedGoal(this);
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}