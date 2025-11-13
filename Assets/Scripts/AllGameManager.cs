using UnityEngine;

public class AllGameManager : MonoBehaviour
{
    public static AllGameManager Instance;
    public string selectedMapName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMap(string mapName)
    {
        selectedMapName = mapName;
        Debug.Log("¿ï¾Ü¦a¹Ï¡G" + mapName);
    }
}