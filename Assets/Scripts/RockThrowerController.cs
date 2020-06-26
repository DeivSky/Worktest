using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class RockThrowerController : MonoBehaviour
{
    public Vector2Reference EstimatedLandingPoint { get; private set; } = null;
    
    private SpawnPool spawnPool = null;
    private RockThrowerSettingsReference rockThrowerSettings = null;
    private Animator animator = null;
    private readonly int throwHash = Animator.StringToHash("Throw");
    private Dictionary<int, RockController> rockControllers = null;

    private void Awake()
    {
        rockThrowerSettings = GameManager.Instance.RockThrowerSettings;
        spawnPool = GameManager.Instance.SpawnPool;
        animator = GetComponent<Animator>();
        rockControllers = new Dictionary<int, RockController>(rockThrowerSettings.Value.RockCount);
        EstimatedLandingPoint = ScriptableObject.CreateInstance<Vector2Reference>();
    }

    private IEnumerator Start()
    {
        Transform rockSpawn = transform.GetChild(0);
        Transform pool = GameObject.Find("rockPool").transform;
        var settings = rockThrowerSettings.Value;
        float minRad = settings.MinThrowAngle * Mathf.Deg2Rad;
        float maxRad = settings.MaxThrowAngle * Mathf.Deg2Rad;

        yield return StartCoroutine(spawnPool.InstantiateAllCoroutine(5));

        foreach (var rock in spawnPool)
        {
            var rockController = rock.Value.GetComponent<RockController>();
            rockController.SetVelocity(
                Random.Range(settings.MinRockVelocity, settings.MaxRockVelocity),
                Random.Range(minRad, maxRad));
            var transform = rock.Value.transform;
            transform.parent = pool;
            transform.position = rockSpawn.position;
            rockControllers.Add(rock.Key, rockController);
        }

        StartCoroutine(Throw(settings.RockCount, new WaitForSeconds(settings.TimeBetweenThrows)));
    }

    private void FixedUpdate()
    {
        var activeRocks = spawnPool.ActiveKeys;
        if (activeRocks.Length == 0) return;
        
        float minRemainingFlightTime = float.MaxValue;
        Vector2 estimated = Vector2.zero;
        for (int i = 0; i < activeRocks.Length; i++)
        {
            var rock = rockControllers[activeRocks[i]];
            if (rock.IsLastBounce || minRemainingFlightTime < rock.RemainingFlightTime)
                continue;

            minRemainingFlightTime = rock.RemainingFlightTime;
            estimated = rock.EstimatedLandingLocation;
        }

        EstimatedLandingPoint.Value = estimated;
    }

    private IEnumerator Throw(int count, YieldInstruction wait)
    {
        for (int i = 0; i < count; i++)
        {
            yield return wait;
            animator.SetTrigger(throwHash);
        }
    }

    private void DoThrow() => spawnPool.Dequeue().SetActive(true);

    private void OnDestroy() => Destroy(EstimatedLandingPoint);
}
