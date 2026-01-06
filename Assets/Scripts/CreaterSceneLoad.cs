using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreaterSceneLoad : MonoBehaviour
{
    public void CreaterScene()
    {
        SceneManager.LoadScene("Creaters");
    }

    public void BackMain()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

