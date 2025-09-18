using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject selectedObject;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void SelectThis(GameObject prefab)
    {
        GameManager.Instance.selectedObject = prefab;
    }
    public void DeselectThis()
    {
        GameManager.Instance.selectedObject = null;
    }
}
