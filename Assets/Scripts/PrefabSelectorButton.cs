using UnityEngine;
using UnityEngine.UI;

public class PrefabSelectorButton : MonoBehaviour
{
    public ObjectPlacer placer;            // �� ObjectPlacer ������i��
    public GameObject prefabToPlace;       // ��A�Q��ܪ� Prefab �i��

    void Start()
    {
        // �]�w���s�I���ƥ�
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (placer != null && prefabToPlace != null)
        {
            placer.SetSelectedObject(prefabToPlace);
        }
        else
        {
            Debug.LogWarning("�|�����w placer �� prefabToPlace�I");
        }
    }
}
