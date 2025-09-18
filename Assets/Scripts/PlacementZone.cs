using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementZone : MonoBehaviour, IDropHandler
{
    public GameObject prefabToPlace;

    public void OnDrop(PointerEventData eventData)
    {
        Vector3 worldPos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out worldPos))
        {
            Instantiate(prefabToPlace, worldPos, Quaternion.identity);
        }
    }
}
