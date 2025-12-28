using UnityEngine;
using System.Collections.Generic;

public class CameraRecorder : MonoBehaviour
{
    // 定義資料結構
    [System.Serializable]
    public struct CameraFrame
    {
        public Vector3 position;
        public Quaternion rotation;
        public float fieldOfView; // 連鏡頭縮放(FOV)都一起錄

        public CameraFrame(Vector3 pos, Quaternion rot, float fov)
        {
            position = pos;
            rotation = rot;
            fieldOfView = fov;
        }
    }

    private List<CameraFrame> recordedFrames = new List<CameraFrame>();
    private bool isRecording = false;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // 由 GridRoundManager 呼叫
    public void StartRecording()
    {
        recordedFrames.Clear();
        isRecording = true;
        RecordFrame(); // 第一幀
    }

    public void StopRecording()
    {
        isRecording = false;
    }

    public List<CameraFrame> GetData()
    {
        return new List<CameraFrame>(recordedFrames);
    }

    void FixedUpdate()
    {
        if (isRecording)
        {
            RecordFrame();
        }
    }

    void RecordFrame()
    {
        // 記錄位置、旋轉、與鏡頭遠近(FOV)
        recordedFrames.Add(new CameraFrame(transform.position, transform.rotation, cam.fieldOfView));
    }
}