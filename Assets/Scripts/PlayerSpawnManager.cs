using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    private PlayerInputManager inputManager;

    private void Awake()
    {
        inputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        inputManager.onPlayerJoined += OnPlayerJoined;
    }

    private void OnDisable()
    {
        inputManager.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log("玩家加入：" + playerInput.playerIndex);
        // 可在這裡設定額外的狀態，例如 UI、Camera 追蹤等
    }

    // 每次有玩家要加入前，改變 playerPrefab
    public void OnPlayerJoinRequest()
    {
        int random = Random.Range(1, 3);
        if (random == 1)
        {
            Debug.Log("將生成 Player1");
            inputManager.playerPrefab = player1Prefab;
        }
        else
        {
            Debug.Log("將生成 Player2");
            inputManager.playerPrefab = player2Prefab;
        }
    }
}