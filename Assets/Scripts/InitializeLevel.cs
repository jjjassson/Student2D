using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inezializelevel : MonoBehaviour
{
    [SerializeField]
    private Transform[] playerSpawns;
    [SerializeField]
    private GameObject PlayerPrefab;


    // Start is called before the first frame update
    void Start()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();
        for (int i = 0; i < playerConfigs.Length; i++)
        {
            var playerConfig = Instantiate(PlayerPrefab, playerSpawns[i].position, playerSpawns[i].rotation, gameObject.transform);

        }
    }

}
