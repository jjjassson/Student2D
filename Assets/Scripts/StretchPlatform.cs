using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class StretchPlatform : MonoBehaviour
{
    [Header("伸縮設定")]
    [Tooltip("平台最小高度")]
    public float minScaleY = 0.5f;
    [Tooltip("平台最大高度")]
    public float maxScaleY = 2f;
    [Tooltip("伸縮速度")]
    public float stretchSpeed = 1f;

    [Header("自動往返伸縮")]
    public bool autoStretch = true;

    private BoxCollider boxCollider;
    private Vector3 baseScale;
    private bool isExpanding = true;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = false; // ✅ 要能被踩在上面
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (!autoStretch) return;

        // ✅ 伸縮邏輯
        Vector3 scale = transform.localScale;
        if (isExpanding)
        {
            scale.y += Time.deltaTime * stretchSpeed;
            if (scale.y >= maxScaleY)
            {
                scale.y = maxScaleY;
                isExpanding = false;
            }
        }
        else
        {
            scale.y -= Time.deltaTime * stretchSpeed;
            if (scale.y <= minScaleY)
            {
                scale.y = minScaleY;
                isExpanding = true;
            }
        }

        transform.localScale = scale;

        // ✅ 同步調整碰撞箱的大小與位置
        UpdateColliderWithScale();
    }

    private void UpdateColliderWithScale()
    {
        // BoxCollider 尺寸依照 localScale 更新
        boxCollider.size = Vector3.one; // 維持 1，讓 scale 直接影響實際碰撞體
        boxCollider.center = new Vector3(0f, 0.5f * transform.localScale.y - 0.5f, 0f);
    }

    // ✅ 可選：玩家接觸測試（除錯用）
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            Debug.Log("Player 碰到了伸縮平台！");
    }
}
