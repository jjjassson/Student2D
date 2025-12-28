using UnityEngine;

public class GoalPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. 停止所有人的錄影 (包含鏡頭)
            ReplayRecorder[] allPlayers = FindObjectsOfType<ReplayRecorder>();
            foreach (var r in allPlayers) r.StopRecording();

            // 🔥 停止鏡頭錄影
            CameraRecorder camRec = FindObjectOfType<CameraRecorder>();
            if (camRec != null) camRec.StopRecording();

            // 2. 呼叫重播 (不需要參數了)
            if (ReplayManager.Instance != null)
            {
                ReplayManager.Instance.StartGlobalReplay();
            }
        }
    }
}