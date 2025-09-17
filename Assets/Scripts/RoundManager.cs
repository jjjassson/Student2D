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
        Debug.Log($"=== Round {roundNumber} �}�l�I ===");
        OnRoundStart?.Invoke(roundNumber);
    }

    public void EndRound()
    {
        Debug.Log($"=== Round {roundNumber} �����I ===");
        OnRoundEnd?.Invoke(roundNumber);

        // �}�l�U�@�^�X
        StartRound();
    }
}