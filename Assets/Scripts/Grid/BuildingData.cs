using UnityEngine;

// 這行屬性讓你可以在 Project 視窗右鍵 -> Create -> Game -> Building Data 來新增卡片
[CreateAssetMenu(fileName = "NewBuilding", menuName = "Game/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("物件資訊")]
    public string buildingName; // 物件名稱
    public GameObject prefab;   // 實際要蓋在場景裡的 3D 模型
    public Sprite icon;         // 顯示在 UI 上的圖示
}