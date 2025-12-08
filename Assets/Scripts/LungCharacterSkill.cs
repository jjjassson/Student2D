using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 只掛載於「肺」角色的物件上
public class LungCharacterSkill : MonoBehaviour
{
    [Header("【可調整】技能時間設定")]
    [Tooltip("煙霧開啟（啟用）持續的時間 (秒)。ON Period。")]
    public float smokeOnDuration = 30f;

    [Tooltip("煙霧關閉（禁用/冷卻）持續的時間 (秒)。OFF Period。")]
    public float smokeOffDuration = 30f;

    [Header("範圍偵測設定")]
    [Tooltip("煙霧擴散的範圍半徑")]
    public float detectionRadius = 10f;

    [Tooltip("只偵測具有此 Layer 的物件 (確保只有玩家被偵測到)")]
    public LayerMask playerLayer;

    private void Start()
    {
        // 確保時間設置合理
        if (smokeOnDuration > 0 && smokeOffDuration >= 0)
        {
            // 角色在場上時，立即啟動技能週期
            StartCoroutine(SmokeCycleCoroutine());
        }
        else
        {
            Debug.LogError("LungCharacterSkill: 請確保煙霧開啟時間 (ON) 大於 0，關閉時間 (OFF) 大於等於 0。");
        }
    }

    // 負責技能循環的協程
    IEnumerator SmokeCycleCoroutine()
    {
        // 1. 讓遊戲開始後有一個初始延遲 (等待第一次 OFF 週期結束)
        // 如果 smokeOffDuration 設為 0，則立刻發動。
        yield return new WaitForSeconds(smokeOffDuration);

        while (true)
        {
            // --- 週期性啟動 (ON) ---
            TriggerSmokeForDuration();

            // 2. 等待煙霧開啟的 自定義秒數
            yield return new WaitForSeconds(smokeOnDuration);

            // --- 週期性關閉 (OFF) ---
            StopSmokeForDuration();

            // 3. 等待煙霧關閉的 自定義秒數 (下次啟動前的間隔)
            yield return new WaitForSeconds(smokeOffDuration);
        }
    }

    /// <summary>
    /// 在範圍內找到其他玩家，並呼叫該玩家的煙霧物件的 SetActive(true)。
    /// </summary>
    void TriggerSmokeForDuration()
    {
        Debug.Log($"肺技能發動，煙霧 ON 持續 {smokeOnDuration} 秒。");

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRadius,
            playerLayer
        );

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.gameObject == this.gameObject)
            {
                continue; // 自我免疫：排除肺角色自己
            }

            // 查找目標物件上的 SmokeToggle 腳本
            // 使用 true 參數確保能找到預設禁用的物件
            SmokeToggle smokeToggle = hit.GetComponentInChildren<SmokeToggle>(true);

            if (smokeToggle != null)
            {
                // **核心控制：啟用 GameObject，觸發 SmokeToggle.OnEnable() 啟動粒子**
                smokeToggle.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 在範圍內找到其他玩家，並呼叫該玩家的煙霧物件的 SetActive(false)。
    /// </summary>
    void StopSmokeForDuration()
    {
        Debug.Log($"煙霧效果結束，進入 OFF 週期，持續 {smokeOffDuration} 秒。");

        // 再次偵測範圍，確保關閉的是當前範圍內的所有玩家
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRadius,
            playerLayer
        );

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.gameObject == this.gameObject)
            {
                continue; // 排除肺角色自己
            }

            SmokeToggle smokeToggle = hit.GetComponentInChildren<SmokeToggle>(true);

            if (smokeToggle != null)
            {
                // **核心控制：禁用 GameObject，觸發 SmokeToggle.OnDisable() 停止粒子**
                smokeToggle.gameObject.SetActive(false);
            }
        }
    }

    // 繪製偵測範圍（僅用於 Unity 編輯器）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}