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

    /// <summary>
    /// 開始新的一回合
    /// </summary>
    private void StartRound()
    {
        roundNumber++;
        Debug.Log($"=== Round {roundNumber} 開始！ ===");
        OnRoundStart?.Invoke(roundNumber);

        alivePlayers = totalPlayers;

        // 復活所有玩家（回到自己的初始 Spawn）
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            var ps = player.GetComponent<PlayerScore>();
            if (ps != null)
            {
                ps.Revive();
            }
        }
    }

    /// <summary>
    /// 結束目前回合
    /// </summary>
    public void EndRound()
    {
        Debug.Log($"=== Round {roundNumber} 結束！ ===");
        OnRoundEnd?.Invoke(roundNumber);

        StartRound(); // 🔹 自動進入下一回合
    }

    /// <summary>
    /// 玩家死亡時呼叫
    /// </summary>
    public void NotifyPlayerDeath(PlayerScore player)
    {
        alivePlayers--;

        Debug.Log($"{player.gameObject.name} 死亡，剩下 {alivePlayers}/{totalPlayers}");

        if (alivePlayers <= 0)
        {
            EndRound();
        }
    }

    /// <summary>
    /// 玩家到達終點時呼叫
    /// </summary>
    public void NotifyPlayerReachedGoal(PlayerScore player)
    {
        Debug.Log($"{player.gameObject.name} 到達終點！");
        EndRound();
    }
}