using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class RockThrowerController : MonoBehaviour
{
    public DataReference<Vector2> NextRockLandingPoint { get; } = new DataReference<Vector2>();
    [SerializeField] private RockSettingsReference rockSettings = null;

    private DataReference<int> score = null;
    private Animator animator = null;
    private readonly int throwHash = Animator.StringToHash("Throw");
    private SpawnPool spawnPool = null;
    private Dictionary<int, RockController> rockControllers = null;
    private int[] activeKeys = null;

    private void Awake()
    {
        score = GameManager.Instance.Score;
        spawnPool = new SpawnPool(rockSettings.Value.RockCount, rockSettings.Value.RockPrefab);
        animator = GetComponent<Animator>();
        int count = rockSettings.Value.RockCount;
        rockControllers = new Dictionary<int, RockController>(count);
        activeKeys = new int[count];
    }

    private IEnumerator Start()
    {
        Transform rockSpawn = transform.GetChild(0);
        Transform pool = new GameObject("rockPool").transform;
        var settings = rockSettings.Value;
        float minRadian = settings.MinThrowAngle * Mathf.Deg2Rad;
        float maxRadian = settings.MaxThrowAngle * Mathf.Deg2Rad;

        yield return StartCoroutine(spawnPool.EnqueueAllCoroutine(5));

        foreach (var rock in spawnPool)
        {
            var rockController = rock.Value.GetComponent<RockController>();
            rockController.Gravity = settings.Gravity;
            rockController.BounceMultiplier = settings.BounceMultiplier;
            rockController.SetVelocity(
                Random.Range(settings.MinRockVelocity, settings.MaxRockVelocity),
                Random.Range(minRadian, maxRadian));
            rockController.Scored += () => score.Value++;
            var transform = rock.Value.transform;
            transform.parent = pool;
            transform.position = rockSpawn.position;
            rockControllers.Add(rock.Key, rockController);
        }

        StartCoroutine(ThrowCoroutine(settings.RockCount, settings.MinTimeBetweenThrows, settings.MaxTimeBetweenThrows));
    }

    private void FixedUpdate() => GetNextLandingPoint();
    
    private void GetNextLandingPoint()
    {
        int count = spawnPool.GetActiveKeys(activeKeys);
        if(count == 0)
        {
            NextRockLandingPoint.Value = Vector2.zero;
            return;
        }

        float minRemainingFlightTime = float.MaxValue;
        Vector2 point = Vector2.zero;
        for (int i = 0; i < count; i++)
        {
            var rock = rockControllers[activeKeys[i]];
            if (rock.IsLastBounce || minRemainingFlightTime < rock.RemainingFlightTime)
                continue;

            minRemainingFlightTime = rock.RemainingFlightTime;
            point = rock.EstimatedLandingLocation;
        }

        NextRockLandingPoint.Value = point;
    }

    private IEnumerator ThrowCoroutine(int count, float minTime, float maxTime)
    {
        for (int i = 0; i < count; i++)
        {
            float random = Random.Range(minTime, maxTime);
            float elapsedTime = 0f;
            while (elapsedTime < random)
            {
                yield return null;
                elapsedTime += Time.deltaTime;
            }
            animator.ResetTrigger(throwHash);
            animator.SetTrigger(throwHash);
        }
    }

    private void Throw() => spawnPool.Dequeue().SetActive(true);
}
