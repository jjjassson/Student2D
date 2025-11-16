using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CharacterLight : MonoBehaviour
{
    [Header("光線設定")]
    public float maxDistance = 20f;           // 最大射線距離
    public Color beamColor = Color.yellow;    // 光線顏色
    public float beamStartWidth = 0.1f;       // 起點粗細
    public float beamEndWidth = 0.1f;         // 末端粗細

    [Header("地面光斑設定")]
    public GameObject groundLightPrefab;      // 預製的圓形光斑
    public float groundLightSize = 1f;        // 光斑大小
    private GameObject groundLightInstance;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;

        line.startWidth = beamStartWidth;
        line.endWidth = beamEndWidth;

        line.material = new Material(Shader.Find("Unlit/Color"));
        line.material.color = beamColor;

        // 生成光斑
        if (groundLightPrefab != null)
        {
            groundLightInstance = Instantiate(groundLightPrefab);
            groundLightInstance.transform.localScale = Vector3.one * groundLightSize;
        }
    }

    private void Update()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * maxDistance;

        if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, maxDistance))
        {
            endPos = hit.point;

            // 更新地面光斑位置
            if (groundLightInstance != null)
            {
                groundLightInstance.transform.position = hit.point + Vector3.up * 0.01f; // 避免 Z-fighting
                groundLightInstance.transform.rotation = Quaternion.Euler(90, 0, 0); // 面向上方
            }
        }

        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
    }
}