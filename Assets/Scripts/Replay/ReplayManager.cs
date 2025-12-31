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
        // 面板開關狀態維持你在 Editor 的設定
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
            // 🔥 改回來了：這裡不再關閉主攝影機，讓它保持原狀
        }
        else
        {
            Debug.LogError("找不到 CameraRecorder！");
            yield break;
        }

        // 2. 開啟面板與重播攝影機
        if (replayPanel) replayPanel.SetActive(true);
        if (replayCamera) replayCamera.gameObject.SetActive(true);

        // 清除舊分身
        foreach (var g in activeGhosts) Destroy(g);
        activeGhosts.Clear();

        // 3. 生成分身並隱藏本尊
        List<GhostRunner> runners = new List<GhostRunner>();
        foreach (var recorder in allRecorders)
        {
            List<ReplayRecorder.ReplayFrame> data = recorder.GetReplayData();
            if (data == null || data.Count == 0) continue;
            if (recorder.myGhostPrefab == null) continue;

            // 生成分身
            GameObject ghost = Instantiate(recorder.myGhostPrefab, data[0].position, data[0].rotation);

            // 強制移除分身的物理組件 (避免鬼跟鬼互撞)
            Rigidbody rb = ghost.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);
            Collider[] cols = ghost.GetComponentsInChildren<Collider>();
            foreach (var c in cols) Destroy(c);

            activeGhosts.Add(ghost);
            runners.Add(new GhostRunner { ghostObject = ghost, frames = data });

            // 🔥🔥 關鍵：只隱藏場上的玩家本尊 🔥🔥
            recorder.gameObject.SetActive(false);
        }

        Debug.Log("本尊已隱藏，開始無限循環重播...");

        // 4. 無限循環播放
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
        }
    }
}