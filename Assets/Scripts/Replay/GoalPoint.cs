using UnityEngine;

public class GoalPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家到達終點！");

            // 1. 停止所有玩家的錄影
            ReplayRecorder[] allPlayers = FindObjectsOfType<ReplayRecorder>();
            foreach (var r in allPlayers) r.StopRecording();

            // 停止鏡頭錄影
            CameraRecorder camRec = FindObjectOfType<CameraRecorder>();
            if (camRec != null) camRec.StopRecording();

            // 2. 🔥 呼叫 GridRoundManager 停止遊戲循環 (不會有下一回合)
            if (GridRoundManager.Instance != null)
            {
                GridRoundManager.Instance.StopGameLoop();
            }

            // 3. 呼叫重播 (並開啟 Replay Panel)
            // 請確保你的 ReplayManager 已經是設定為「無限循環」的版本
            if (ReplayManager.Instance != null)
            {
                ReplayManager.Instance.StartGlobalReplay();
            }
        }
    }
}