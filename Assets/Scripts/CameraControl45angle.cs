using UnityEngine;

public class CameraController45Angle : MonoBehaviour
{
    [Header("�n�[�ݪ��ؼЪ���]���x���ߡ^")]
    public Transform target;

    // �o��ӰѼƫO�d�u�O���F��K�s��A��ڤ��|�Ψ�
    [Header("�Z���P���׳]�w�]�ثe���ϥΡ^")]
    public float distance = 10f;
    public float height = 10f;

    // ���A�ϥ� Start() �۰ʩw��

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(target.position, 0.5f);
        }
    }
}
