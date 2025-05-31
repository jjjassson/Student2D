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
        Debug.Log("���a�[�J�G" + playerInput.playerIndex);
        // �i�b�o�̳]�w�B�~�����A�A�Ҧp UI�BCamera �l�ܵ�
    }

    // �C�������a�n�[�J�e�A���� playerPrefab
    public void OnPlayerJoinRequest()
    {
        int random = Random.Range(1, 3);
        if (random == 1)
        {
            Debug.Log("�N�ͦ� Player1");
            inputManager.playerPrefab = player1Prefab;
        }
        else
        {
            Debug.Log("�N�ͦ� Player2");
            inputManager.playerPrefab = player2Prefab;
        }
    }
}