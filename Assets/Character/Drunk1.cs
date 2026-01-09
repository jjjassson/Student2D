using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class Drunk1 : MonoBehaviour
{
    [Header("能力設定")]
    public float possessRange = 3f;
    public float possessDuration = 10f;
    public float cooldown = 30f;

    [Header("被附身者閃光設定")]
    public Color flashColor = Color.cyan;
    public float flashSpeed = 5f;

    [Header("自身透明度設定")]
    [Range(0f, 1f)]
    public float selfAlphaDuringPossess = 0.3f;

    [Header("輸入設定")]
    public InputActionReference possessAction;

    private CharacterController controller;
    private MonoBehaviour selfPlayer;
    private MonoBehaviour targetPlayer;

    private Renderer selfRenderer;
    private Renderer targetRenderer;

    private Color[] originalSelfColors;
    private Color[] originalTargetColors;

    private bool isPossessing;
    private bool isOnCooldown;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        selfPlayer = GetComponent<Player1>();
        selfRenderer = GetComponentInChildren<Renderer>();
    }

    private void OnEnable()
    {
        possessAction?.action.Enable();
    }

    private void OnDisable()
    {
        possessAction?.action.Disable();
    }

    private void Update()
    {
        if (isPossessing || isOnCooldown) return;

        if (possessAction != null && possessAction.action.WasPressedThisFrame())
            TryPossess();
    }

    private void TryPossess()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, possessRange);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Player1 p1 = hit.GetComponent<Player1>();
            Player2 p2 = hit.GetComponent<Player2>();

            if (p1 != null || p2 != null)
            {
                targetPlayer = (MonoBehaviour)(p1 != null ? p1 : p2);
                targetRenderer = targetPlayer.GetComponentInChildren<Renderer>();
                StartCoroutine(PossessRoutine());
                break;
            }
        }
    }

    private IEnumerator PossessRoutine()
    {
        isPossessing = true;
        isOnCooldown = true;

        selfPlayer.enabled = false;
        controller.enabled = false;

        SaveOriginalColors();
        StartVisualEffects();

        yield return new WaitForSeconds(possessDuration);

        EndPossession();
        StartCoroutine(CooldownRoutine());
    }

    private void SaveOriginalColors()
    {
        if (selfRenderer != null)
        {
            originalSelfColors = new Color[selfRenderer.materials.Length];
            for (int i = 0; i < selfRenderer.materials.Length; i++)
                originalSelfColors[i] = selfRenderer.materials[i].color;
        }

        if (targetRenderer != null)
        {
            originalTargetColors = new Color[targetRenderer.materials.Length];
            for (int i = 0; i < targetRenderer.materials.Length; i++)
                originalTargetColors[i] = targetRenderer.materials[i].color;
        }
    }

    private void StartVisualEffects()
    {
        if (targetRenderer != null)
            StartCoroutine(TargetFlashRoutine());

        if (selfRenderer != null)
            StartCoroutine(SelfFadeRoutine());
    }

    private void EndPossession()
    {
        RestoreColors();

        controller.enabled = true;
        selfPlayer.enabled = true;
        isPossessing = false;
    }

    private void RestoreColors()
    {
        if (selfRenderer != null && originalSelfColors != null)
            for (int i = 0; i < selfRenderer.materials.Length; i++)
                selfRenderer.materials[i].color = originalSelfColors[i];

        if (targetRenderer != null && originalTargetColors != null)
            for (int i = 0; i < targetRenderer.materials.Length; i++)
                targetRenderer.materials[i].color = originalTargetColors[i];
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }

    private IEnumerator TargetFlashRoutine()
    {
        float t = 0f;
        while (isPossessing)
        {
            t += Time.deltaTime * flashSpeed;
            float lerp = Mathf.PingPong(t, 1f);

            for (int i = 0; i < targetRenderer.materials.Length; i++)
                targetRenderer.materials[i].color =
                    Color.Lerp(originalTargetColors[i], flashColor, lerp);

            yield return null;
        }
    }

    private IEnumerator SelfFadeRoutine()
    {
        while (isPossessing)
        {
            for (int i = 0; i < selfRenderer.materials.Length; i++)
            {
                Color c = originalSelfColors[i];
                c.a = selfAlphaDuringPossess;
                selfRenderer.materials[i].color = c;
            }
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, possessRange);
    }
}
