using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance;

    [Header("UI 設定")]
    public GameObject replayPanel;
    public RawImage replayDisplay;

    [Header("重播攝影機")]
    public Camera replayCamera; // 這是專門負責重播的那台攝影機
    public float autoCloseDelay = 4.0f;

    // 內部資料結構
    private class GhostRunner
    {
        public GameObject ghostObject;
        public List<ReplayRecorder.ReplayFrame> frames;
    }

    private List<GameObject> activeGhosts = new List<GameObject>();
    private Coroutine replayCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (replayPanel) replayPanel.SetActive(false);
        if (replayCamera) replayCamera.gameObject.SetActive(false);
    }

    // 這裡我們不再需要 winnerRecorder，因為鏡頭路徑已經固定了
    public void StartGlobalReplay()
    {
        if (replayCoroutine != null) StopCoroutine(replayCoroutine);
        replayCoroutine = StartCoroutine(ExactReplayRoutine());
    }

    IEnumerator ExactReplayRoutine()
    {
        // 1. 取得所有玩家的錄影資料
        ReplayRecorder[] allRecorders = FindObjectsOfType<ReplayRecorder>();

        // 🔥 2. 取得主攝影機的錄影資料
        CameraRecorder mainCamRecorder = FindObjectOfType<CameraRecorder>();
        List<CameraRecorder.CameraFrame> camFrames = null;

        if (mainCamRecorder != null)
        {
            camFrames = mainCamRecorder.GetData();
        }
        else
        {
            Debug.LogError("找不到 CameraRecorder！無法重現運鏡。");
            yield break;
        }

        // 3. 準備 UI 與重播攝影機
        if (replayPanel) replayPanel.SetActive(true);
        if (replayCamera) replayCamera.gameObject.SetActive(true);

        // 清除舊的分身
        foreach (var g in activeGhosts) Destroy(g);
        activeGhosts.Clear();

        // 4. 生成所有分身
        List<GhostRunner> runners = new List<GhostRunner>();

        foreach (var recorder in allRecorders)
        {
            List<ReplayRecorder.ReplayFrame> data = recorder.GetReplayData();
            if (data == null || data.Count == 0) continue;
            if (recorder.myGhostPrefab == null) continue;

            GameObject ghost = Instantiate(recorder.myGhostPrefab, data[0].position, data[0].rotation);
            activeGhosts.Add(ghost);

            runners.Add(new GhostRunner { ghostObject = ghost, frames = data });
        }

        Debug.Log($"開始重播：鏡頭影格數 {camFrames.Count}");

        // 5. 開始同步播放 (以鏡頭的影格數為準)
        int currentFrameIndex = 0;
        int maxFrames = camFrames.Count;

        while (currentFrameIndex < maxFrames)
        {
            // A. 更新每一隻鬼的位置
            foreach (var runner in runners)
            {
                if (currentFrameIndex < runner.frames.Count)
                {
                    runner.ghostObject.transform.position = runner.frames[currentFrameIndex].position;
                    runner.ghostObject.transform.rotation = runner.frames[currentFrameIndex].rotation;
                }
            }

            // 🔥 B. 更新重播攝影機的位置 (完全照抄主攝影機剛剛的動作)
            if (replayCamera != null)
            {
                replayCamera.transform.position = camFrames[currentFrameIndex].position;
                replayCamera.transform.rotation = camFrames[currentFrameIndex].rotation;
                replayCamera.fieldOfView = camFrames[currentFrameIndex].fieldOfView;
            }

            currentFrameIndex++;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("重播結束");
        yield return new WaitForSeconds(autoCloseDelay);
        CloseReplay();
    }

    public void CloseReplay()
    {
        if (replayPanel) replayPanel.SetActive(false);
        if (replayCamera) replayCamera.gameObject.SetActive(false);
        foreach (var g in activeGhosts) if (g != null) Destroy(g);
        activeGhosts.Clear();
        replayCoroutine = null;
    }
}