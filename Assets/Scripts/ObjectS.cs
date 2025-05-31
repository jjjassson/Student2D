using UnityEngine;

public class ObjectC: MonoBehaviour
{
    public GameObject Player1;
    public GameObject Player2;

    void Awake()
    {
        bool useFirst = Random.value < 0.5f;

        Player1.SetActive(useFirst);
        Player2.SetActive(!useFirst);
    }
}