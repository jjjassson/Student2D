using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class StretchPlatform : MonoBehaviour
{
    [Header("伸縮設定")]
    [Tooltip("平台最小高度")]
    public float minScaleX = 0.5f;
    [Tooltip("平台最大高度")]
    public float maxScaleX = 2f;
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
            scale.x += Time.deltaTime * stretchSpeed;
            if (scale.x >= maxScaleX)
            {
                scale.x = maxScaleX;
                isExpanding = false;
            }
        }
        else
        {
            scale.x -= Time.deltaTime * stretchSpeed;
            if (scale.x <= minScaleX)
            {
                scale.x = minScaleX;
                isExpanding = true;
            }
        }

        transform.localScale = scale;

       
    }

    

    // ✅ 可選：玩家接觸測試（除錯用）
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            Debug.Log("Player 碰到了伸縮平台！");
    }
}
