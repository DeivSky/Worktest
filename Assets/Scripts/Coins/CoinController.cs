using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador de las monedas.
/// Controla el ciclo de vida y animación de la moneda, y dispara un evento cuando es agarrada. 
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class CoinController : MonoBehaviour
{
    public float Lifetime { get; set; } = 0f;
    public float AnimationDuration { get; set; } = 0f;
    public Transform CoinUI { get; set; } = null;
    public event Action CoinGrabbed;

    private float lifetime = 0f;
    private new SpriteRenderer renderer = null;
    private new Collider2D collider2D = null;
    private new Camera camera = null;
    private Color color;
    private Color transparent;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        color = renderer.color;
        transparent = color;
        transparent.a = 0f;
        collider2D = GetComponent<Collider2D>();
        camera = Camera.main;
    }

    private void OnEnable()
    {
        collider2D.enabled = true;
        lifetime = Lifetime;
        StartCoroutine(FadeIn());
    }

    private void Update()
    {
        if (!(lifetime > 0f)) return;

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
            StartCoroutine(Disable(StartCoroutine(FadeOut())));
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        collider2D.enabled = false;
        StartCoroutine(Disable(StartCoroutine(GrabbedAnimation())));
    }

    private IEnumerator GrabbedAnimation()
    {
        Vector2 from = transform.position;
        Vector2 to = camera.ScreenToWorldPoint(CoinUI.position);
        float elapsedTime = 0f;
        while (elapsedTime < AnimationDuration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            transform.position = Vector2.Lerp(from, to, elapsedTime / AnimationDuration);
        }

        CoinGrabbed?.Invoke();
    }

    private IEnumerator FadeIn()
    {
        renderer.color = transparent;
        float elapsedTime = 0f;
        while (elapsedTime < AnimationDuration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            renderer.color = Color.Lerp(transparent, color, elapsedTime / AnimationDuration);
        }

        renderer.color = color;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        while (elapsedTime < AnimationDuration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            renderer.color = Color.Lerp(color, transparent, elapsedTime / AnimationDuration);
        }

        renderer.color = transparent;
    }

    private IEnumerator Disable(YieldInstruction wait)
    {
        yield return wait;
        gameObject.SetActive(false);
    }
}