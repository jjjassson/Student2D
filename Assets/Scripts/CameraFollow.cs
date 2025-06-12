using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public string playerTag = "Player";
    public float minDistance = 10f;
    public float maxDistance = 30f;
    public float smoothTime = 0.3f;
    public Vector3 offsetDirection = new Vector3(0, 10, -10);

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        if (players.Length == 0) return;

        // 計算角色包圍盒
        Bounds bounds = new Bounds(players[0].transform.position, Vector3.zero);
        foreach (var player in players)
        {
            bounds.Encapsulate(player.transform.position);
        }

        Vector3 center = bounds.center;
        float greatestDistance = Mathf.Max(bounds.size.x, bounds.size.z);

        // 決定攝影機距離
        float distance = Mathf.Clamp(greatestDistance, minDistance, maxDistance);
        Vector3 desiredPos = center + offsetDirection.normalized * distance;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);

        transform.LookAt(center);
    }
}