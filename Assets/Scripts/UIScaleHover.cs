using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // 用于访问 Image 组件
using System.Collections; // 用于协程

// 实现 IPointerEnterHandler 和 IPointerExitHandler 接口来捕捉鼠标事件
public class UIScaleHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("缩放设置")]
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f); // 鼠标悬停时的大小
    public float animationDuration = 0.15f; // 动画完成所需的时间 (秒)

    [Header("颜色设置 (可选)")]
    public bool enableColorChange = true;
    public Color hoverColor = new Color(0.85f, 0.85f, 1f); // 悬停时的亮蓝色调

    private RectTransform rectTransform;
    private Image imageComponent;

    private Vector3 originalScale; // 原始大小
    private Color originalColor; // 原始颜色

    private Coroutine activeCoroutine; // 用于存储当前运行的动画协程

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        imageComponent = GetComponent<Image>();

        // 确保找到了组件
        if (rectTransform == null || (enableColorChange && imageComponent == null))
        {
            Debug.LogError("UIScaleHover 脚本需要附加到带有 RectTransform 和 Image 组件的 UI 对象上。", this);
            enabled = false;
            return;
        }

        // 存储原始值
        originalScale = rectTransform.localScale;
        if (imageComponent != null)
        {
            originalColor = imageComponent.color;
        }
    }

    // 当鼠标进入时调用
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 如果有其他动画正在运行，先停止它
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);

        // 启动进入动画
        activeCoroutine = StartCoroutine(AnimateScaleAndColor(hoverScale, hoverColor));
    }

    // 当鼠标离开时调用
    public void OnPointerExit(PointerEventData eventData)
    {
        // 如果有其他动画正在运行，先停止它
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);

        // 启动恢复动画
        activeCoroutine = StartCoroutine(AnimateScaleAndColor(originalScale, originalColor));
    }

    // 通用的缩放和颜色平滑过渡协程
    IEnumerator AnimateScaleAndColor(Vector3 targetScale, Color targetColor)
    {
        float timeElapsed = 0f;
        Vector3 startScale = rectTransform.localScale;
        Color startColor = (imageComponent != null) ? imageComponent.color : targetColor; // 避免空引用

        while (timeElapsed < animationDuration)
        {
            // 计算插值比例 (0 到 1)
            float t = timeElapsed / animationDuration;

            // 使用 Mathf.SmoothStep 让动画两端更平滑
            t = Mathf.SmoothStep(0f, 1f, t);

            // 1. 缩放平滑过渡
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            // 2. 颜色平滑过渡 (如果启用了颜色变化)
            if (enableColorChange && imageComponent != null)
            {
                imageComponent.color = Color.Lerp(startColor, targetColor, t);
            }

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        // 确保动画结束时达到精确的目标值
        rectTransform.localScale = targetScale;
        if (enableColorChange && imageComponent != null)
        {
            imageComponent.color = targetColor;
        }

        activeCoroutine = null; // 动画完成，清除协程引用
    }
}