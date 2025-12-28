using UnityEngine;
using System.Collections.Generic;

public class ReplayRecorder : MonoBehaviour
{
    // 🔥 記得：每個角色的 Prefab 都要設不一樣的 Ghost Prefab
    [Header("角色設定")]
    public GameObject myGhostPrefab;

    // ... (其餘程式碼與之前相同) ...
    [System.Serializable]
    public struct ReplayFrame { public Vector3 position; public Quaternion rotation; public ReplayFrame(Vector3 p, Quaternion r) { position = p; rotation = r; } }
    private List<ReplayFrame> recordedFrames = new List<ReplayFrame>();
    private bool isRecording = false;

    public void StartNewRecording()
    {
        recordedFrames.Clear();
        isRecording = true;
        recordedFrames.Add(new ReplayFrame(transform.position, transform.rotation));
    }

    public void StopRecording() { isRecording = false; }
    public List<ReplayFrame> GetReplayData() { return new List<ReplayFrame>(recordedFrames); }
    void FixedUpdate() { if (isRecording) recordedFrames.Add(new ReplayFrame(transform.position, transform.rotation)); }
}