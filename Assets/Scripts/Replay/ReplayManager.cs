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
    public Camera replayCamera;
    public float restartDelay = 2.0f; // 重播播完後，停頓幾秒再重播

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

        // 🔥 修改 1：移除了所有 SetActive(false) 的程式碼
        // 現在遊戲開始時，Panel 的開關狀態完全由你在 Unity Editor 裡決定
        // 如果你希望一開始是關的，請在 Editor 裡把勾勾取消
    }

    public void StartGlobalReplay()
    {
        if (replayCoroutine != null) StopCoroutine(replayCoroutine);
        replayCoroutine = StartCoroutine(ExactReplayRoutine());
    }

    IEnumerator ExactReplayRoutine()
    {
        // 1. 取得資料
        ReplayRecorder[] allRecorders = FindObjectsOfType<ReplayRecorder>();
        CameraRecorder mainCamRecorder = FindObjectOfType<CameraRecorder>();
        List<CameraRecorder.CameraFrame> camFrames = null;

        if (mainCamRecorder != null)
        {
            camFrames = mainCamRecorder.GetData();
        }
        else
        {
            Debug.LogError("找不到 CameraRecorder！");
            yield break;
        }

        // 🔥 修改 2：只有在到達終點觸發重播時，才強制開啟 Panel
        if (replayPanel) replayPanel.SetActive(true);
        if (replayCamera) replayCamera.gameObject.SetActive(true);

        // 清除舊分身
        foreach (var g in activeGhosts) Destroy(g);
        activeGhosts.Clear();

        // 2. 生成分身
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

        Debug.Log("開始無限循環重播...");

        // 🔥 修改 3：無限迴圈 (While True)
        // 這會讓重播一直重複播放，永遠不會進入「關閉 Panel」的階段
        while (true)
        {
            int currentFrameIndex = 0;
            int maxFrames = camFrames.Count;

            // 播放一次完整的錄影
            while (currentFrameIndex < maxFrames)
            {
                // 更新鬼的位置
                foreach (var runner in runners)
                {
                    if (currentFrameIndex < runner.frames.Count)
                    {
                        runner.ghostObject.transform.position = runner.frames[currentFrameIndex].position;
                        runner.ghostObject.transform.rotation = runner.frames[currentFrameIndex].rotation;
                    }
                }

                // 更新攝影機位置
                if (replayCamera != null)
                {
                    replayCamera.transform.position = camFrames[currentFrameIndex].position;
                    replayCamera.transform.rotation = camFrames[currentFrameIndex].rotation;
                    replayCamera.fieldOfView = camFrames[currentFrameIndex].fieldOfView;
                }

                currentFrameIndex++;
                yield return new WaitForFixedUpdate();
            }

            // 播完一次後，等待幾秒
            yield return new WaitForSeconds(restartDelay);

            // 迴圈會回到開頭，currentFrameIndex 歸零，重新再播一次
            // 這樣 Panel 就會一直開著，直到你手動關閉遊戲
        }
    }
}