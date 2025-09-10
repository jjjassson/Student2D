using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InitializeLevel : MonoBehaviour
{
    [SerializeField] private Transform[] playerSpawns; // ���a�ͦ��I

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

            // Instantiate ���� prefab
            GameObject playerObject = Instantiate(prefab, playerSpawns[i].position, playerSpawns[i].rotation);

            // ���o���� prefab �W�� PlayerInput
            PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();

            if (playerInput == null)
            {
                Debug.LogError("���� prefab ������ PlayerInput�I");
                continue;
            }

            // �j�w���a�쥻�ϥΪ����
            foreach (var device in config.Input.devices)
            {
                InputUser.PerformPairingWithDevice(device, playerInput.user);
            }

            // �]�w playerIndex
            playerInput.user.AssociateActionsWithUser(playerInput.actions);
        }
    }
}