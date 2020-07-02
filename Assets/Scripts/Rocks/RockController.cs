using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador de roca.
/// Simula el movimiento físico de la roca y calcula el tiempo restante de vuelo <see cref="RemainingFlightTime"/>,
/// la posición estimada de aterrizaje <see cref="EstimatedLandingLocation"/>, y si el último rebote hace que la roca
/// se pase de la meta <see cref="IsLastBounce"/>, determina la velocidad necesaria para caer directamente en la misma.
/// Además dispara un evento cuando alcanza la meta. 
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class RockController : MonoBehaviour
{
    public float Gravity { get; set; }
    public Vector2 BounceMultiplier { get; set; }
    public float RemainingFlightTime { get; private set; } = 0f;
    public Vector2 EstimatedLandingLocation { get; private set; } = Vector2.zero;
    public bool IsLastBounce => EstimatedLandingLocation.x >= levelBounds.Max.x || RemainingFlightTime < 0f;
    public event Action Scored;

    private Transform goal = null;
    private Vector2 velocity = Vector2.zero;
    private float fixedDeltaTime = 0f;
    private bool disableUpdate = false;
    private new Rigidbody2D rigidbody = null;
    private new SpriteRenderer renderer = null;
    private new Collider2D collider2D = null;
    private LevelBounds levelBounds = null;

    private float playerHeight = 0f;
    
    private void Awake()
    {
        var gm = GameManager.Instance;
        goal = gm.Goal;
        levelBounds = gm.LevelBounds;
        rigidbody = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<Collider2D>();
        fixedDeltaTime = Time.fixedDeltaTime;

        var player = gm.Player.transform;
        var box = player.GetChild(0);
        var bounds = box.GetComponent<Collider2D>().bounds;
        Vector2 scale = (Vector2)box.localScale * player.localScale;
        playerHeight = bounds.center.y + bounds.extents.y * scale.y;
    }

    private void Start() => Compute();

    /// <summary>
    /// Se mueve la roca y, si cae al piso, inicia la corrutina <see cref="Destroy"/>
    /// </summary>
    private void FixedUpdate()
    {
        if (disableUpdate) return;

        RemainingFlightTime -= fixedDeltaTime;
        velocity.y += Gravity * fixedDeltaTime;
        Move(velocity * fixedDeltaTime);
        if (rigidbody.position.y <= levelBounds.Min.y)
            StartCoroutine(Destroy(2f));
    }

    /// <summary>
    /// Rebota y/o suma un punto dependiendo de contra qué colisione
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("PlayerBox"))
            Bounce();
        else if (other.gameObject.CompareTag("Goal"))
        {
            collider2D.enabled = false;
            velocity.x = 0f;
            if (velocity.y > 0f) velocity.y = 0f;
            rigidbody.MovePosition(other.transform.position);
            StartCoroutine(Score());
        }
    }

    /// <summary>
    /// Desplaza al rigidbody en base a <paramref name="delta"/>.
    /// </summary>
    /// <param name="delta"></param>
    private void Move(Vector2 delta) => rigidbody.MovePosition(rigidbody.position + delta);

    /// <summary>
    /// Se fija el vector velocidad <see cref="velocity"/> a partir del ángulo <paramref name="radian"/> y la magnitud <paramref name="magnitude"/>
    /// </summary>
    /// <param name="magnitude"></param>
    /// <param name="radian"></param>
    public void SetVelocity(float magnitude, float radian) =>
        velocity = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)) * magnitude;

    /// <summary>
    /// Rebota, recalcula y, si es el último rebote <see cref="IsLastBounce+"/>, determina la velocidad para embocar.
    /// </summary>
    private void Bounce()
    {
        if (velocity.y > 0f) return;

        velocity.y = -velocity.y;
        velocity *= BounceMultiplier;

        Compute();

        if (IsLastBounce)
            SetFinalBounceVelocity();
    }

    /// <summary>
    /// Se calcula el tiempo de vuelo y la posición de aterrizaje.
    /// </summary>
    private void Compute()
    {
        Vector2 position = rigidbody.position;
        float deltaY = playerHeight - position.y;
        float sqrt = Mathf.Pow(velocity.y, 2) + 2f * Gravity * deltaY;
        if (sqrt < 0f)
        {
            RemainingFlightTime = -1f;
            return;
        }

        sqrt = Mathf.Sqrt(sqrt);
        float time = Mathf.Max((-velocity.y + sqrt) / Gravity,
            (-velocity.y - sqrt) / Gravity);

        RemainingFlightTime = time;
        float deltaX = velocity.x * time;
        EstimatedLandingLocation = new Vector2(position.x + deltaX, playerHeight);
    }

    /// <summary>
    /// Se determina y se fija la velocidad tal que la roca emboque en la meta.
    /// </summary>
    private void SetFinalBounceVelocity()
    {
        const float deg = 60f;
        const float radRight = deg * Mathf.Deg2Rad;
        const float radLeft = (180f - deg) * Mathf.Deg2Rad;

        Vector2 position = rigidbody.position;
        Vector2 goalPosition = goal.position;
        Vector2 delta = goalPosition - position;

        float angle = Mathf.Sign(delta.x) >= 0f ? radRight : radLeft;

        float a = -Gravity / 2f * Mathf.Pow(delta.x, 2) / Mathf.Pow(Mathf.Cos(angle), 2);
        float b = delta.x * Mathf.Sin(angle) / Mathf.Cos(angle) - delta.y;

        float v = a / b;

        if (v < 0f) return;
        SetVelocity(Mathf.Sqrt(v), angle);
    }

    /// <summary>
    /// Corrutina llamada cuando la roca cae al piso.
    /// Antes de destruir el objeto, se anima la roca desapareciendo.
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator Destroy(float duration)
    {
        disableUpdate = true;
        velocity.y = 0f;
        var waitForFixedUpdate = new WaitForFixedUpdate();
        collider2D.enabled = false;

        float fromX = velocity.x;
        const float toX = 0f;

        Color fromColor = renderer.color;
        Color toColor = fromColor;
        toColor.a = 0f;

        float angle = 0f;
        float elapsedTime = 0f;
        while (duration > elapsedTime)
        {
            velocity.x = Mathf.Lerp(fromX, toX, elapsedTime / duration);
            Move(velocity * fixedDeltaTime);

            angle -= velocity.x;
            rigidbody.MoveRotation(angle);

            renderer.color = Color.Lerp(fromColor, toColor, elapsedTime / duration);

            elapsedTime += fixedDeltaTime;
            yield return waitForFixedUpdate;
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Al llamar al evento de anotar <see cref="Score"/> y destruir el objeto, se simula que la roca entra en la meta.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Score()
    {
        var wait = new WaitForFixedUpdate();
        Vector2 localScale = transform.localScale;
        Vector2 size = renderer.size * localScale;
        while (size.y > 0f)
        {
            size.y += velocity.y * fixedDeltaTime;
            renderer.size = size / localScale;

            yield return wait;
        }

        Scored?.Invoke();
        Destroy(gameObject);
    }
}