using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    [Header("Player Settings")]
    public int maxHealth = 100;

    private int currentHealth;
    private bool isDead = false;

    // 🔹 讓 CameraFollow 或其他程式可以讀取玩家是否活著
    public bool isAlive => !isDead;

    // 🔹 儲存玩家初始出生點
    private Transform initialSpawn;

    private void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    /// <summary>
    /// 設定初始出生點（通常在生成玩家時呼叫）
    /// </summary>
    /// <param name="spawn">玩家出生點 Transform</param>
    public void SetInitialSpawn(Transform spawn)
    {
        initialSpawn = spawn;
    }

    /// <summary>
    /// 受傷
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 死亡
    /// </summary>
    public void Die()
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

    /// <summary>
    /// 玩家掉落死亡
    /// </summary>
    public void FallDown()
    {
        Debug.Log($"{gameObject.name} 掉落死亡！");
        Die();
    }

    /// <summary>
    /// 復活玩家，回到初始出生點
    /// </summary>
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

    /// <summary>
    /// 玩家到達終點
    /// </summary>
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