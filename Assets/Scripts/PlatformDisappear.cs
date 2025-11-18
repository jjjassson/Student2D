using UnityEngine;

public class PlatformDisappear : MonoBehaviour
{
    [Header("踩上去後消失的秒數")]
    public float disappearDelay = 1.5f;

    private bool isStepped = false;

    // ❗ Player1 在 OnControllerColliderHit 中會呼叫這裡
    public void OnStepped()
    {
        if (!isStepped)
        {
            isStepped = true;
            Invoke(nameof(Disappear), disappearDelay);
        }
    }

    private void Disappear()
    {
        // 你可以選擇啟用/禁用 或 Destroy
        gameObject.SetActive(false);
    }
}

        // 如果要之後自動回來，可以用： 
        // Invoke(nameo
