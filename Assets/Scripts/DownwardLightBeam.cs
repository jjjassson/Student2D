using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DownwardLightBeam : MonoBehaviour
{
    [Header("光線設定")]
    public float maxDistance = 20f;
    public float beamStartWidth = 0.1f;
    public float beamEndWidth = 0.8f;
    public float beamAlpha = 0.6f;
    public bool debugRay = false;

    private LineRenderer line;
    private float currentBeamLength;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = beamStartWidth;
        line.endWidth = beamEndWidth;

        // 基本設定
        line.material = new Material(Shader.Find("Unlit/Transparent"));
        line.numCapVertices = 8;
    }

    private void Update()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * maxDistance;
        currentBeamLength = maxDistance;

        // 往下偵測地面
        if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, maxDistance))
        {
            endPos = hit.point;
            currentBeamLength = hit.distance;
        }

        if (debugRay)
            Debug.DrawLine(startPos, endPos, Color.yellow);

        // 更新位置
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);

        // 計算長度比例（0 = 很近, 1 = 全長）
        float t = Mathf.InverseLerp(0f, maxDistance, currentBeamLength);

        // 🌈 動態建立漸層
        Gradient dynamicGradient = new Gradient();
        dynamicGradient.SetKeys(
            new GradientColorKey[]
            {
                // 頂端顏色（亮白偏黃）
                new GradientColorKey(new Color(1f, 1f, 0.9f), 0f),
                // 底部顏色根據距離變深
                new GradientColorKey(Color.Lerp(new Color(1f, 0.85f, 0.3f), new Color(1f, 0.95f, 0.5f), 1f - t), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(beamAlpha, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        line.colorGradient = dynamicGradient;

        // 💡 調整寬度（地面近 → 光束幾乎不擴散）
        float endW = Mathf.Lerp(beamStartWidth, beamEndWidth, t);
        line.startWidth = beamStartWidth;
        line.endWidth = endW;

        // 微閃動感
        float flicker = Mathf.Lerp(beamAlpha * 0.5f, beamAlpha, Mathf.PingPong(Time.time * 1.3f, 1f));
        line.material.SetColor("_Color", new Color(1f, 1f, 0.8f, flicker));
    }
}