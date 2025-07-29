using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InitializeLevel : MonoBehaviour
{
    [SerializeField]
    private Transform[] playerSpawns;

    void Start()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();

        for (int i = 0; i < playerConfigs.Length; i++)
        {
            var config = playerConfigs[i];

            // �ϥΨC�쪱�a��ܪ� prefab
            GameObject playerPrefab = config.SelectedCharacterPrefab;

            if (playerPrefab != null)
            {
                // ��Ҥ� prefab
                var playerObject = Instantiate(playerPrefab, playerSpawns[i].position, playerSpawns[i].rotation);

                // ������� PlayerInput ����s prefab �W
                PlayerInput originalInput = config.Input;
                originalInput.transform.SetParent(null);
                originalInput.transform.position = playerSpawns[i].position;

                // �� PlayerInput ���j��s�� prefab�]�p�G�A prefab ������ݭn Input�^
                originalInput.SwitchCurrentControlScheme(originalInput.currentControlScheme, originalInput.devices.ToArray());

                // �� Input ���� prefab �W�]�i��A�ݧA�]�p�޿�^
                originalInput.gameObject.SetActive(false); // �����쥻�� placeholder
                originalInput.transform.SetParent(playerObject.transform, true);
                originalInput.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError($"Player {i} �S���﨤�� prefab�I");
            }
        }
    }
}
