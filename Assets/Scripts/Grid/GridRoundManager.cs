using UnityEngine;
using System.Collections.Generic;
using System.Collections; 

public class GridRoundManager : MonoBehaviour
{
    // --- 修改處：類型改為 GridRoundManager ---
    public static GridRoundManager Instance { get; private set; }

    [Header("物件資料夾")]
    public List<GameObject> objectFolder = new List<GameObject>();

    private void Awake()
    {
        // --- 修改處：Singleton 檢查 ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // 自動測試：延遲 1 秒後發牌
        StartCoroutine(AutoStartRoundDebug());
    }

    IEnumerator AutoStartRoundDebug()
    {
        yield return new WaitForSeconds(1.0f);
        Debug.Log("【測試模式】GridRoundManager：自動開始回合...");
        StartRound();
    }

    [ContextMenu("開始回合 (發送物件)")]
    public void StartRound()
    {
        if (objectFolder.Count == 0)
        {
            Debug.LogError("錯誤：GridRoundManager 的 Object Folder 是空的！請拖曳 Prefab 進去！");
            return;
        }

        // 尋找場上所有玩家 (注意：玩家身上必須有 GridObjectPlacer 腳本)
        GridObjectPlacer[] players = FindObjectsOfType<GridObjectPlacer>();

        if (players.Length == 0)
        {
            Debug.LogError("錯誤：場景中找不到任何掛有 GridObjectPlacer 的物件！請檢查玩家Prefab。");
            return;
        }

        // 隨機抽選物件
        int randomIndex = Random.Range(0, objectFolder.Count);
        GameObject selectedObj = objectFolder[randomIndex];

        // 發送給所有玩家
        foreach (var player in players)
        {
            player.AssignNewObject(selectedObj);
        }

        Debug.Log($"發送物件完成：{selectedObj.name} 給 {players.Length} 位玩家");
    }
}