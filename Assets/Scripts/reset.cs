using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class reset : MonoBehaviour
{
    public void resetgame()
    {
        SceneManager.LoadScene("Main");
        if (Input.GetKey("z"))
        {
            SceneManager.LoadScene("Main");
        }

    }
}
