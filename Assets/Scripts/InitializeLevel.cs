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


            GameObject playerPrefab = config.SelectedCharacterPrefab;

            if (playerPrefab != null)
            {

                var playerObject = Instantiate(playerPrefab, playerSpawns[i].position, playerSpawns[i].rotation);


                PlayerInput originalInput = config.Input;
                originalInput.transform.SetParent(null);
                originalInput.transform.position = playerSpawns[i].position;


                originalInput.SwitchCurrentControlScheme(originalInput.currentControlScheme, originalInput.devices.ToArray());


                originalInput.gameObject.SetActive(false);
                originalInput.transform.SetParent(playerObject.transform, true);
                originalInput.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError($"Player {i} 沒有選角色 prefab！");
            }
        }
    }
}