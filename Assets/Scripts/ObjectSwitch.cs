using UnityEngine;

public class ObjectSwitch : MonoBehaviour
{
    public GameObject Player1;
    public GameObject Player2;

    private GameObject currentInstance;

    void Start()
    {
        // �w�]���J prefab1
        SwitchToPrefab1();
    }

    public void SwitchToPrefab1()
    {
        ReplaceWith(Player1);
    }

    public void SwitchToPrefab2()
    {
        ReplaceWith(Player2);
    }

    private void ReplaceWith(GameObject prefab)
    {
        if (currentInstance != null)
        {
            Destroy(currentInstance);
        }

        currentInstance = Instantiate(prefab, transform);
        currentInstance.transform.localPosition = Vector3.zero;
        currentInstance.transform.localRotation = Quaternion.identity;
    }

    // ��K���եΡA�� 1/2 ����
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchToPrefab1();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchToPrefab2();
        }
    }
}
