using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCursorController : MonoBehaviour
{
    public int playerId = 0;
    public RectTransform cursorUI;
    public float moveSpeed = 500f;

    private Vector2 moveInput;
    private Vector2 mouseInput;
    private GameControls controls;

    void Awake()
    {
        controls = new GameControls();
    }

    void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => moveInput = Vector2.zero;

        if (playerId == 0)
        {
            controls.Player.MousePoint.performed += ctx => mouseInput = ctx.ReadValue<Vector2>();
        }
    }

    void Update()
    {
        if (playerId == 0)
        {
            cursorUI.position = mouseInput;
        }
        else
        {
            Vector3 pos = cursorUI.position;
            pos += (Vector3)(moveInput * moveSpeed * Time.deltaTime);
            pos.x = Mathf.Clamp(pos.x, 0, Screen.width);
            pos.y = Mathf.Clamp(pos.y, 0, Screen.height);
            cursorUI.position = pos;
        }
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }
}
