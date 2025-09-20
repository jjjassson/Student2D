using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InitializeLevel : MonoBehaviour
{
    [SerializeField] private Transform[] playerSpawns;

    private void Start()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();

        for (int i = 0; i < playerConfigs.Length; i++)
        {
            var config = playerConfigs[i];
            GameObject prefab = config.SelectedCharacterPrefab;

            if (prefab == null)
            {
                Debug.LogError($"Player {i} �S���﨤�� prefab�I");
                continue;
            }

            GameObject playerObject = Instantiate(prefab, playerSpawns[i].position, playerSpawns[i].rotation);

            // �j�w���
            PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                foreach (var device in config.Input.devices)
                    InputUser.PerformPairingWithDevice(device, playerInput.user);

                playerInput.user.AssociateActionsWithUser(playerInput.actions);
            }

            // �j�w PlayerInputHandler
            var inputHandler = playerObject.GetComponent<PlayerInputHandler>();
            if (inputHandler != null)
                inputHandler.InitializePlayer(config);
        }
    }
}