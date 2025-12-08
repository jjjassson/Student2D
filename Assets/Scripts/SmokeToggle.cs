using UnityEngine;

// 確保腳本掛載在帶有 Particle System 元件的物件上
public class SmokeToggle : MonoBehaviour
{
    private ParticleSystem smokeParticleSystem;

    void Awake()
    {
        // 只需要在初始化時取得 Particle System 元件
        smokeParticleSystem = GetComponent<ParticleSystem>();

        if (smokeParticleSystem == null)
        {
            Debug.LogError("SmokeToggle 錯誤：腳本必須掛載在帶有 ParticleSystem 元件的物件上！");
        }
    }

    // 當這個 GameObject 被啟用 (SetActive(true)) 時，就會執行這個函式
    void OnEnable()
    {
        if (smokeParticleSystem != null)
        {
            // **核心指令：物件一被啟用，立即開始發射粒子**
            smokeParticleSystem.Play();
            Debug.Log("【煙霧開啟成功】: OnEnable 觸發 Particle System.Play()。");
        }
    }

    // 當這個 GameObject 被禁用 (SetActive(false)) 時，就會執行這個函式
    void OnDisable()
    {
        if (smokeParticleSystem != null)
        {
            // **核心指令：物件被禁用，立即停止發射粒子**
            // 使用 StopEmittingAndClear 確保畫面粒子立刻消失
            smokeParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Debug.Log("【煙霧關閉成功】: OnDisable 觸發 Particle System.Stop()。");
        }
    }
}