using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject Player1;
    public GameObject Player2;

    void Start()
    {
        GameObject selectedPrefab = Random.value < 0.5f ? Player1 : Player2;

        // Instantiate at spawner's position and rotation
        Instantiate(selectedPrefab, transform.position, transform.rotation);

        // Destroy the spawner since it's just a middleman
        Destroy(gameObject);
    }
}