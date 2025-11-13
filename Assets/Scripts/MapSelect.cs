using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelect : MonoBehaviour
{
    public void SelectMap(string mapName)
    {
        AllGameManager.Instance.SetMap(mapName);
        SceneManager.LoadScene("PlayerSetup");
    }
}