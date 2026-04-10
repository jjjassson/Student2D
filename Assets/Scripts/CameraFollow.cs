using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public string playerTag = "Player";
    public float minDistance = 10f;
    public float maxDistance = 30f;
    public float smoothTime = 0.3f;
    public Vector3 offsetDirection = new Vector3(0, 10, -10);

    [Header("Camera Bounds (鏡頭邊界限制)")]
    public bool enableMinXLimit = true; // 是否開啟 X 軸下限
    public float minXLimit = -13f;      // 當 X 小於這個數值時，停止追蹤

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        // 溫馨提醒：在 LateUpdate 每一幀執行 FindGameObjectsWithTag 非常消耗效能。
        // 建議未來優化時，可以在玩家生成或死亡時才去更新一個共用的 List<GameObject>。
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        var alivePlayers = new List<GameObject>();

        foreach (var p in players)
        {
            var ps = p.GetComponent<PlayerScore>();
            if (ps != null && ps.isAlive)
                alivePlayers.Add(p);
        }

        if (alivePlayers.Count == 0) return;

        Bounds bounds = new Bounds(alivePlayers[0].transform.position, Vector3.zero);
        foreach (var player in alivePlayers)
        {
            bounds.Encapsulate(player.transform.position);
        }

        Vector3 center = bounds.center;

        // ==========================================
        // 新增邏輯：限制中心點的 X 軸最小值
        // ==========================================
        if (enableMinXLimit)
        {
            // Mathf.Max 會取兩者中較大的值。
            // 例如 center.x 是 -15，minXLimit 是 -13，它就會鎖死在 -13。
            // 如果 center.x 是 -10，它就會正常保持 -10。
            center.x = Mathf.Max(center.x, minXLimit);
        }
        // ==========================================

        float greatestDistance = Mathf.Max(bounds.size.x, bounds.size.z);

        float distance = Mathf.Clamp(greatestDistance, minDistance, maxDistance);
        Vector3 desiredPos = center + offsetDirection.normalized * distance;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);
        transform.LookAt(center);
    }
}