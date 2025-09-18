using UnityEngine;
using UnityEngine.UI;

public class ItemSelector : MonoBehaviour
{
    public GameObject[] items;
    public Transform content;
    public GameObject buttonPrefab;

    void Start()
    {
        foreach (var item in items)
        {
            GameObject currentItem = item;
            var btn = Instantiate(buttonPrefab, content);
            btn.GetComponentInChildren<Text>().text = currentItem.name;
            btn.GetComponent<Button>().onClick.AddListener(() => SelectItem(currentItem));
        }
    }

    void SelectItem(GameObject obj)
    {
        GameManager.Instance.selectedObject = obj;
        Debug.Log($"Selected: {obj.name}");
    }
}
