using UnityEngine;

public class RoundManager : MonoBehaviour
{
    private int roundNumber = 0;
    private int totalPlayers;
    private int alivePlayers;

    public delegate void RoundEvent(int round);
    public event RoundEvent OnRoundStart;
    public event RoundEvent OnRoundEnd;

    private static RoundManager instance;
    public static RoundManager Instance => instance;

    [Header("Spawn Settings")]
    public Vector3 spawnPosition = new Vector3(0, 2f, 0);

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        totalPlayers = GameObject.FindGameObjectsWithTag("Player").Length;
        StartRound();
    }

    private void StartRound()
    {
        roundNumber++;
        Debug.Log($"=== Round {roundNumber} 開始！ ===");
        OnRoundStart?.Invoke(roundNumber);

        alivePlayers = totalPlayers;

        // 復活所有玩家
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var ps = player.GetComponent<PlayerScore>();
            if (ps != null)
            {
                ps.Revive(spawnPosition);
            }
        }
    }

    public void EndRound()
    {
        Debug.Log($"=== Round {roundNumber} 結束！ ===");
        OnRoundEnd?.Invoke(roundNumber);

        StartRound();
    }

    // 🔹 當玩家死亡
    public void NotifyPlayerDeath(PlayerScore player)
    {
        alivePlayers--;

        if (alivePlayers <= 0)
        {
            EndRound();
        }
    }

    // 🔹 當有玩家到達終點
    public void NotifyPlayerReachedGoal(PlayerScore player)
    {
        EndRound();
    }
}