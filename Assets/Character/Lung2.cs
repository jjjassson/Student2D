using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class Lung2 : MonoBehaviour
{
    [Header("角色基本參數")]
    public float moveSpeed = 4f;
    public float jumpForce = 1f;
    public float gravityValue = -9.81f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool groundedPlayer;

    [HideInInspector] public bool isSlowed = false;
    [HideInInspector] public bool isJumpReduced = false;
    [HideInInspector] public bool isInverted = false;

    private float defaultMoveSpeed;
    private float defaultJumpForce;

    // ================= 煙霧 =================
    [Header("煙霧技能")]
    public GameObject smokeObject;
    public float smokeDuration = 5f;
    public float smokeCooldown = 10f;
    public bool autoLoop = true;

    [Header("煙霧範圍")]
    public float smokeRange = 3f;

    [Header("煙霧材質")]
    public Material smokeMaterial;

    private ParticleSystem smokeParticle;
    private ParticleSystemRenderer smokeRenderer;

    private HashSet<Lung2> affectedTargets = new HashSet<Lung2>();

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;

        if (smokeObject != null)
        {
            smokeParticle = smokeObject.GetComponent<ParticleSystem>();
            smokeRenderer = smokeObject.GetComponent<ParticleSystemRenderer>();

            if (smokeRenderer != null && smokeMaterial != null)
                smokeRenderer.material = smokeMaterial;

            smokeObject.SetActive(false);
        }

        if (autoLoop)
            StartCoroutine(SmokeLoop());
    }

    IEnumerator SmokeLoop()
    {
        while (true)
        {
            SetSmoke(true);

            float timer = 0f;
            while (timer < smokeDuration)
            {
                timer += Time.deltaTime;
                UpdateSmokeEffect();
                yield return null;
            }

            SetSmoke(false);
            yield return new WaitForSeconds(smokeCooldown);
        }
    }

    void SetSmoke(bool active)
    {
        if (smokeParticle == null) return;

        if (active)
        {
            smokeObject.SetActive(true);
            smokeParticle.Play();
        }
        else
        {
            smokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            smokeObject.SetActive(false);
            ResetAllAffected();
        }
    }

    void UpdateSmokeEffect()
    {
        HashSet<Lung2> currentFrame = new HashSet<Lung2>();

        Collider[] hits = Physics.OverlapSphere(transform.position, smokeRange);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            if (hit.TryGetComponent<Lung2>(out var l2))
            {
                l2.ApplySpeedMultiplier(0.5f);
                currentFrame.Add(l2);
            }
        }

        foreach (var old in affectedTargets)
        {
            if (!currentFrame.Contains(old))
                old.ResetSpeed();
        }

        affectedTargets = currentFrame;
    }

    void ResetAllAffected()
    {
        foreach (var t in affectedTargets)
            t.ResetSpeed();

        affectedTargets.Clear();
    }

    // ===== 原本功能 =====
    public void ApplySpeedMultiplier(float multiplier)
    {
        moveSpeed = defaultMoveSpeed * multiplier;
        isSlowed = multiplier < 1f;
    }

    public void ResetSpeed()
    {
        moveSpeed = defaultMoveSpeed;
        isSlowed = false;
    }

    public void ApplyJumpMultiplier(float multiplier)
    {
        jumpForce = defaultJumpForce * multiplier;
        isJumpReduced = multiplier < 1f;
    }

    public void ResetJump()
    {
        jumpForce = defaultJumpForce;
        isJumpReduced = false;
    }

    public void InvertMovement()
    {
        isInverted = true;
    }

    public void ResetInverted()
    {
        isInverted = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && groundedPlayer)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravityValue);
        }
    }

    private void Update()
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && velocity.y < 0)
            velocity.y = 0f;

        Vector3 move = new Vector3(moveInput.x, 0, 0);

        if (isInverted)
            move *= -1f;

        controller.Move(move * Time.deltaTime * moveSpeed);

        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Mathf.Abs(move.x) > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                720f * Time.deltaTime
            );
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null) return;

        PlatformDisappear platform = hit.collider.GetComponent<PlatformDisappear>();
        if (platform != null)
            platform.OnStepped();
    }
}