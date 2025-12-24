using UnityEngine;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Game/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public GameObject mainPrefab;      // 主物件
    public GameObject secondaryPrefab; // 副物件 (Z=0)
    public Sprite uiSprite;            // UI 圖示
}