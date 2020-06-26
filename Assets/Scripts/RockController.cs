using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class RockController : MonoBehaviour
{
    public float RemainingFlightTime { get; private set; } = 0f;
    public Vector2 EstimatedLandingLocation { get; private set; } = Vector2.zero;
    public bool IsLastBounce => EstimatedLandingLocation.x >= levelBounds.Max.x || RemainingFlightTime < 0f;
    
    private RockSettingsReference rockSettings = null;
    private Transform goal = null;
    private Vector2 velocity = Vector2.zero;
    private float fixedDeltaTime = 0f;
    private bool dying = false;
    private Rigidbody2D rigidbody = null;
    private SpriteRenderer renderer = null;
    private Collider2D collider2D = null;
    private LevelBounds levelBounds = null;

    private void Awake()
    {
        goal = GameManager.Instance.Goal;
        rockSettings = GameManager.Instance.RockSettings;
        rigidbody = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<Collider2D>();
        fixedDeltaTime = Time.fixedDeltaTime;
        levelBounds = GameManager.Instance.LevelBounds;
    }

    private void Start() => Compute();

    private void FixedUpdate()
    {
        if (dying) return;

        RemainingFlightTime -= fixedDeltaTime;
        velocity.y += rockSettings.Value.Gravity * fixedDeltaTime;
        Move(velocity * fixedDeltaTime);
        if (rigidbody.position.y <= levelBounds.Min.y) 
            StartCoroutine(Destroy(2f));
    }

    private void Move(Vector2 delta) => rigidbody.MovePosition(rigidbody.position + delta);

    private void OnCollisionEnter2D(Collision2D other)
    {
        switch (other.transform.tag)
        {
            case "Player":
                Bounce();
                break;
            case "Goal":
                velocity.x = 0f;
                if (velocity.y > 0f) velocity.y = 0f; 
                rigidbody.MovePosition(other.transform.position);
                StartCoroutine(Scored());
                break;
        }
    }

    public void SetVelocity(float magnitude, float radian) =>
        velocity = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)) * magnitude;

    private void Bounce()
    {
        if (velocity.y > 0f) return;

        velocity.y = -velocity.y;
        velocity *= rockSettings.Value.BounceMultiplier;

        Compute();
        
        if (IsLastBounce)
            SetFinalBounceVelocity();
    }

    private void Compute()
    {
        float gravity = rockSettings.Value.Gravity;
        Vector2 position = rigidbody.position;
        float deltaY = levelBounds.Min.y - position.y;
        float sqrt = Mathf.Pow(velocity.y, 2) + 2f * gravity * deltaY;
        if (sqrt < 0f)
        {
            RemainingFlightTime = -1f;
            return;
        }

        sqrt = Mathf.Sqrt(sqrt);
        float time = Mathf.Max((-velocity.y + sqrt) / gravity,
            (-velocity.y - sqrt) / gravity);
        
        RemainingFlightTime = time;
        float deltaX = velocity.x * time;
        EstimatedLandingLocation = new Vector2(position.x + deltaX, levelBounds.Min.y);
    }

    private void SetFinalBounceVelocity()
    {
        const float deg = 60f;
        const float radRight = deg * Mathf.Deg2Rad;
        const float radLeft = (180f - deg) * Mathf.Deg2Rad;
        
        Vector2 position = rigidbody.position;
        Vector2 goalPosition = goal.position;
        float gravity = rockSettings.Value.Gravity;

        float x = goalPosition.x - position.x;
        float y = goalPosition.y - position.y;
        float angle = Mathf.Sign(x) >= 0f ? radRight : radLeft;

        float a = gravity / 2f * Mathf.Pow(x, 2) / Mathf.Pow(Mathf.Cos(angle), 2);
        float b = x * Mathf.Sin(angle) / Mathf.Cos(angle) - y;

        float v = -a / b;

        if (v < 0f) return;
        SetVelocity(Mathf.Sqrt(v), angle);
    }

    private IEnumerator Destroy(float duration)
    {
        dying = true;
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

    private IEnumerator Scored()
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

        Destroy(gameObject);
    }
}
