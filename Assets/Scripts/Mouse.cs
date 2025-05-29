using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCursorController : MonoBehaviour
{
    public RectTransform cursorUI; // �ۤv�����UI (Image)
    public float moveSpeed = 500f;

    private Vector2 moveInput;

    void Update()
    {
        Vector3 pos = cursorUI.anchoredPosition;
        pos += (Vector3)moveInput * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, 0, Screen.width);
        pos.y = Mathf.Clamp(pos.y, 0, Screen.height);
        cursorUI.anchoredPosition = pos;
    }

    public void OnMoveCursor(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("Player clicked something!");
            // �A�i�H�[�W raycast �ˬd�O�_�I��F���� icon ��
        }
    }
}