using UnityEngine;
using UnityEngine.InputSystem;

public class InPutHandler : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public GameObject cursorVisual; // �Ψ���ܦ۩w�q���

    [Header("Cursor Settings")]
    public float cursorSpeed = 1000f;

    private GameControls controls;
    private Vector2 cursorInput;
    private Vector2 mousePosition;
    private bool useMouse = false;

    private void Awake()
    {
        controls = new GameControls();

        // �n�챱���С]�Ҧp P1��P4�^
        controls.Player.Move.performed += ctx => cursorInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => cursorInput = Vector2.zero;

        // �ƹ������I���P��m�]�u�����ժ̥Ρ^
        controls.Player.MousePoint.performed += ctx =>
        {
            mousePosition = ctx.ReadValue<Vector2>();
            useMouse = true;
        };

        controls.Player.MouseClick.performed += ctx =>
        {
            Debug.Log("Mouse Clicked!");
            // �i�H�I�s��ܨ禡�A�Ҧp�G
            // SelectObjectAtPosition(mousePosition);
        };

        // �n�����]�p A ���m�^
        controls.Player.Confirm.performed += ctx =>
        {
            Debug.Log("Confirm (Gamepad A) Pressed!");
        };
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        if (useMouse)
        {
            // �ƹ�����Ҧ�
            cursorVisual.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
        }
        else
        {
            // �n�챱��Ҧ��]�[�t���ʡ^
            Vector3 moveDelta = new Vector3(cursorInput.x, cursorInput.y, 0f) * cursorSpeed * Time.deltaTime;
            cursorVisual.transform.position += moveDelta;
        }
    }
}
