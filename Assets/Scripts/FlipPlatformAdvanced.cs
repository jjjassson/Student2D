using System.Collections;
using UnityEngine;

public class FlipPlatformAdvanced : MonoBehaviour
{
    [Header("時間設定")]
    public float interval = 3f;              // 每個狀態維持時間
    public float flipDuration = 0.6f;         // 翻轉所需時間
    public float colliderDisableDelay = 0.2f; // 翻轉後多久關 Collider
    public float warningTime = 1f;             // 翻轉前警告閃爍時間

    [Header("翻轉設定")]
    public float flipAngle = 180f;             // X 軸翻轉角度

    [Header("閃爍設定")]
    public float blinkInterval = 0.15f;

    private Collider col;
    private Renderer[] renderers;

    private float timer = 0f;
    private bool colliderEnabled = true;
    private bool isFlipping = false;

    private Quaternion startRotation;
    private Quaternion targetRotation;

    private void Start()
    {
        col = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();

        if (col == null)
        {
            Debug.LogError($"{name} 沒有 Collider，FlipPlatformAdvanced 無法運作");
            enabled = false;
            return;
        }

        col.enabled = true;
    }

    private void Update()
    {
        if (isFlipping) return;

        timer += Time.deltaTime;

        // 進入警告階段
        if (timer >= interval - warningTime && timer < interval)
        {
            if (!IsInvoking(nameof(Blink)))
                InvokeRepeating(nameof(Blink), 0f, blinkInterval);
        }

        // 到時間，開始翻轉
        if (timer >= interval)
        {
            CancelInvoke(nameof(Blink));
            SetRendererVisible(true);

            timer = 0f;
            StartCoroutine(FlipRoutine());
        }
    }

    private IEnumerator FlipRoutine()
    {
        isFlipping = true;

        // 設定旋轉目標
        startRotation = transform.rotation;
        targetRotation = startRotation * Quaternion.Euler(flipAngle, 0f, 0f);

        float t = 0f;
        bool colliderToggled = false;

        while (t < flipDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / flipDuration);

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, normalized);

            // 延遲關 / 開 Collider
            if (!colliderToggled && t >= colliderDisableDelay)
            {
                colliderEnabled = !colliderEnabled;
                col.enabled = colliderEnabled;
                colliderToggled = true;
            }

            yield return null;
        }

        transform.rotation = targetRotation;
        isFlipping = false;
    }

    // ===== 閃爍 =====
    private void Blink()
    {
        foreach (var r in renderers)
        {
            if (r != null)
                r.enabled = !r.enabled;
        }
    }

    private void SetRendererVisible(bool visible)
    {
        foreach (var r in renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }
}
