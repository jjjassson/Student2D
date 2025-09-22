using System.Collections.Generic;
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
        float greatestDistance = Mathf.Max(bounds.size.x, bounds.size.z);

        float distance = Mathf.Clamp(greatestDistance, minDistance, maxDistance);
        Vector3 desiredPos = center + offsetDirection.normalized * distance;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);
        transform.LookAt(center);
    }
}