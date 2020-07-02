using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador de la guía del jugador.
/// Obtiene la posición de la roca más próxima a caer a partir del controlador del lanzador de rocas <see cref="RockThrowerController"/>.
/// Cuando no hay rocas próximas a caer, sigue al jugador.
/// </summary>
public class PlayerGuideController : MonoBehaviour
{
    private const float duration = 0.5f;

    private DataReference<Vector2> rockLandingPoint = null;
    private Transform player = null;
    private Vector2 offset = Vector2.zero;
    private Vector2 lastValue = Vector2.zero;
    private Coroutine coroutine = null;

    private void Awake()
    {
        player = GameManager.Instance.Player.transform;
        var box = player.GetChild(0);
        var collider = box.GetComponent<BoxCollider2D>();
        float x = collider.bounds.size.x * box.localScale.x * player.localScale.x;
        var renderer = GetComponent<SpriteRenderer>();
        float y = renderer.sprite.bounds.extents.y * transform.localScale.y;
        offset = new Vector2(x, y);
        rockLandingPoint = GameManager.Instance.RockThrower.GetComponent<RockThrowerController>().NextRockLandingPoint;
    }

    private void Start() => coroutine = StartCoroutine(FollowPlayer());

    private void Update()
    {
        Vector2 value = rockLandingPoint.Value;
        if (value == lastValue) return;

        lastValue = value;
        if (value == Vector2.zero)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = StartCoroutine(FollowPlayer());
            return;
        }

        Vector2 from = transform.position;
        Vector2 to = value - offset;

        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(Move(from, to));
    }

    private IEnumerator Move(Vector2 from, Vector2 to)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            transform.position = Vector2.Lerp(from, to, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = to;
        coroutine = null;
    }

    private IEnumerator FollowPlayer()
    {
        float elapsedTime = 0f;
        while (Mathf.Abs(player.position.x - transform.position.x) > Vector2.kEpsilon && elapsedTime < duration)
        {
            Vector2 from = transform.position;
            Vector2 to = player.position;
            to.y -= offset.y;
            while (elapsedTime < duration)
            {
                transform.position = Vector2.Lerp(from, to, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;

                if (to != (Vector2) player.position) break;
            }
        }

        while (true)
        {
            Vector2 position = player.position;
            position.y -= offset.y;
            transform.position = position;
            yield return null;
        }
    }
}