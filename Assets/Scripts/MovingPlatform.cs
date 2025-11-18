using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("移動設定")]
    public float moveDistance = 3f;   // 平移距離（左右來回）
    public float moveSpeed = 2f;      // 移動速度

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingToTarget = true;

    private void Start()
    {
        startPos = transform.position;
        targetPos = startPos + new Vector3(moveDistance, 0, 0);
    }

    private void Update()
    {
        // 平滑移動到目標
        if (movingToTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
                movingToTarget = false;  // 換方向
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, startPos) < 0.01f)
                movingToTarget = true;  // 換方向
        }
    }
}
