using UnityEngine;
using UnityEngine.SceneManagement;

public class MainStart : MonoBehaviour
{
    public void GamePlay()
    {
        SceneManager.LoadScene("MapSelect");
    }
}