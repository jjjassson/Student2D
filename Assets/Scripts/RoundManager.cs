using UnityEngine;

public class RoundManager : MonoBehaviour
{
    private int roundNumber = 0;

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
        StartRound();
    }

    private void StartRound()
    {
        roundNumber++;
        Debug.Log($"=== Round {roundNumber} 開始！ ===");
        OnRoundStart?.Invoke(roundNumber);
    }

    public void EndRound()
    {
        Debug.Log($"=== Round {roundNumber} 結束！ ===");
        OnRoundEnd?.Invoke(roundNumber);

        // 開始下一回合
        StartRound();
    }
}