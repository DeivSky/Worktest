using System;
using System.Collections;
using UnityEngine;

public class PlayerGuideController : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    
    private Vector2Reference rockLandingPoint = null;
    private Transform player = null;
    private Vector2 offset = Vector2.zero;
    private Vector2 lastValue = Vector2.zero;
    private Coroutine coroutine = null;

    private void Awake()
    {
        player = GameManager.Instance.PlayerController.transform;
        var box = player.GetChild(0);
        var collider = box.GetComponent<BoxCollider2D>();
        float x = collider.bounds.extents.x * box.localScale.x * player.localScale.x;
        var renderer = GetComponent<SpriteRenderer>();
        float y = renderer.sprite.bounds.extents.y;
        offset = new Vector2(x, y);
    }

    private void Start() => rockLandingPoint = GameManager.Instance.RockThrowerController.EstimatedLandingPoint;
    
    private void Update()
    {
        Vector2 value = rockLandingPoint.Value;
        if (value == Vector2.zero || value == lastValue) return;

        lastValue = value;
        Vector2 from = transform.position;
        from.y = value.y - offset.y;
        Vector2 to = value - offset;
        
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(LerpPosition(from, to, duration));
    }

    private IEnumerator LerpPosition(Vector2 from, Vector2 to, float duration)
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
}
