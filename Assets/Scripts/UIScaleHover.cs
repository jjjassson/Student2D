using UnityEngine;
using UnityEngine.EventSystems; // 必須引用
using UnityEngine.UI;
using System.Collections;

// 新增 ISelectHandler 和 IDeselectHandler 介面
public class UIScaleHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("縮放設定")]
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    public float animationDuration = 0.15f;

    [Header("顏色設定 (可選)")]
    public bool enableColorChange = true;
    public Color hoverColor = new Color(0.85f, 0.85f, 1f);

    private RectTransform rectTransform;
    private Image imageComponent;

    private Vector3 originalScale;
    private Color originalColor;

    private Coroutine activeCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        imageComponent = GetComponent<Image>();

        if (rectTransform == null)
        {
            // 如果只是做縮放，Image 不是必須的，除非你要變色
            Debug.LogError("UIScaleHover 需要 RectTransform。", this);
            enabled = false;
            return;
        }

        originalScale = rectTransform.localScale;

        if (imageComponent != null)
        {
            originalColor = imageComponent.color;
        }
    }

    // --- 滑鼠事件 ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySelectAnimation();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PlayDeselectAnimation();
    }

    // --- 搖桿/鍵盤選取事件 (新增的部分) ---
    // 當搖桿移動選到這個按鈕時觸發
    public void OnSelect(BaseEventData eventData)
    {
        PlaySelectAnimation();
    }

    // 當搖桿移開去選別的按鈕時觸發
    public void OnDeselect(BaseEventData eventData)
    {
        PlayDeselectAnimation();
    }

    // --- 動畫邏輯封裝 ---

    private void PlaySelectAnimation()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(AnimateScaleAndColor(hoverScale, hoverColor));
    }

    private void PlayDeselectAnimation()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(AnimateScaleAndColor(originalScale, originalColor));
    }

    // 這裡保持你原本寫得很好的協程邏輯
    IEnumerator AnimateScaleAndColor(Vector3 targetScale, Color targetColor)
    {
        float timeElapsed = 0f;
        Vector3 startScale = rectTransform.localScale;
        Color startColor = (imageComponent != null) ? imageComponent.color : targetColor;

        while (timeElapsed < animationDuration)
        {
            float t = timeElapsed / animationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            if (enableColorChange && imageComponent != null)
            {
                imageComponent.color = Color.Lerp(startColor, targetColor, t);
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = targetScale;
        if (enableColorChange && imageComponent != null)
        {
            imageComponent.color = targetColor;
        }

        activeCoroutine = null;
    }
}