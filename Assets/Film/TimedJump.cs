using UnityEngine;

public class TimedJump : MonoBehaviour
{
    // 公開變數，可以在 Unity Inspector 中調整
    [Tooltip("角色跳躍的力量/速度")]
    public float jumpForce = 5f;

    [Tooltip("兩次跳躍之間的時間間隔（秒）")]
    public float jumpInterval = 2.0f;

    // 私有變數
    private Rigidbody rb;
    private float timeToNextJump;

    void Start()
    {
        // 獲取角色上的 Rigidbody 組件
        rb = GetComponent<Rigidbody>();

        // 檢查是否成功獲取 Rigidbody
        if (rb == null)
        {
            Debug.LogError("TimedJump 腳本需要一個 Rigidbody 組件才能運作!");
            enabled = false; // 禁用此腳本，防止錯誤
            return;
        }

        // --- 遊戲開始時隨機第一次跳躍的邏輯 ---

        // 隨機一個 0 到 jumpInterval 之間的時間
        // 第一次跳躍將在這個隨機時間後發生
        float randomDelay = Random.Range(0f, jumpInterval);

        // 設定下一次跳躍的時間
        timeToNextJump = Time.time + randomDelay;

        Debug.Log("遊戲開始，第一次跳躍將在 " + randomDelay.ToString("F2") + " 秒後發生。");
    }

    void Update()
    {
        // 檢查當前時間是否超過了預定的下一次跳躍時間
        if (Time.time >= timeToNextJump)
        {
            // 執行跳躍
            Jump();

            // 更新下一次跳躍的時間：當前時間加上跳躍間隔
            // 使用 timeToNextJump += jumpInterval; 確保間隔準確
            timeToNextJump += jumpInterval;
        }
    }

    // 執行跳躍動作的函式
    void Jump()
    {
        // 為了確保每次跳躍的力量一致，先將垂直速度歸零（可選）
        // 這可以防止在空中連續跳躍時力量疊加
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // 施加一個向上的力量
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        Debug.Log("跳躍! (Time: " + Time.time.ToString("F2") + ")");
    }
}