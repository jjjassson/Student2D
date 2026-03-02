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

        // 🔥 關鍵修改：不使用 SetActive(false)，改用自訂的狀態切換
        SetPlayerState(false);

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

        // 確保重新啟用時物理狀態正確
        SetPlayerState(true);

        // 如果你有使用 CharacterController，傳送前必須先關閉它，傳送後再打開
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (initialSpawn != null)
        {
            transform.position = initialSpawn.position;
            transform.rotation = initialSpawn.rotation;
        }

        if (cc != null) cc.enabled = true;

        Debug.Log($"{gameObject.name} 復活在初始出生點");
    }

    public void ReachGoal()
    {
        // 🔥 抵達終點後，也要隱藏並停止玩家動作，但不關閉 PlayerInput
        SetPlayerState(false);

        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.NotifyPlayerReachedGoal(this);
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // ==========================================
    // ★★★ 關鍵新增：用來控制玩家顯示與物理的函式 ★★★
    // ==========================================
    private void SetPlayerState(bool state)
    {
        // 1. 處理碰撞器 (Collider / CharacterController)
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = state;

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = state;

        // 2. 處理剛體 (防止死亡後還受到重力或碰撞影響)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = !state;
            if (!state)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 3. 處理模型渲染 (隱藏/顯示身體)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = state;
        }

        // 4. 關閉輸入處理，讓死掉的玩家不能亂動
        PlayerInputHandler inputHandler = GetComponent<PlayerInputHandler>();
        if (inputHandler != null) inputHandler.enabled = state;
    }
}