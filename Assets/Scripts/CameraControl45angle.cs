using UnityEngine;

public class CameraController45Angle : MonoBehaviour
{
    [Header("要觀看的目標物件（平台中心）")]
    public Transform target;

    // 這兩個參數保留只是為了方便編輯，實際不會用到
    [Header("距離與高度設定（目前未使用）")]
    public float distance = 10f;
    public float height = 10f;

    // 不再使用 Start() 自動定位

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(target.position, 0.5f);
        }
    }
}
