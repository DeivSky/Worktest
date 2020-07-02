using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Controlador del Lanzador de rocas. Se encarga de instanciar las rocas, y almacena referencias de sus controladores
/// a fin obtener la roca más próxima a caer, y alimentar al controlador de la guía del jugador.
/// </summary>
[RequireComponent(typeof(Animator))]
public class RockThrowerController : MonoBehaviour
{
    public DataReference<Vector2> NextRockLandingPoint { get; } = new DataReference<Vector2>();
    [SerializeField] private RockSettingsReference rockSettings = null;

    private Animator animator = null;
    private readonly int throwHash = Animator.StringToHash("Throw");
    private SpawnPool spawnPool = null;
    private Dictionary<int, RockController> rockControllers = null;
    private int[] activeKeys = null;

    private void Awake()
    {
        var settings = rockSettings.Value;
        spawnPool = new SpawnPool(settings.RockCount, settings.RockPrefab);
        animator = GetComponent<Animator>();
        rockControllers = new Dictionary<int, RockController>(settings.RockCount);
        activeKeys = new int[settings.RockCount];
    }

    /// <summary>
    /// Al comienzo del juego se instancian todas las rocas y se les asignan valores random de velocidad y ángulo de disparo.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Start()
    {
        Transform rockSpawn = transform.GetChild(0);
        Transform pool = new GameObject("rockPool").transform;
        var settings = rockSettings.Value;
        float minRadian = settings.ThrowAngle.Min * Mathf.Deg2Rad;
        float maxRadian = settings.ThrowAngle.Max * Mathf.Deg2Rad;
        var score = GameManager.Instance.Score;

        yield return StartCoroutine(spawnPool.EnqueueAllCoroutine(5));

        foreach (var rock in spawnPool)
        {
            var rockController = rock.Value.GetComponent<RockController>();
            rockController.Gravity = settings.Gravity;
            rockController.BounceMultiplier = settings.BounceMultiplier;
            rockController.SetVelocity(
                Random.Range(settings.RockVelocity.Min, settings.RockVelocity.Max),
                Random.Range(minRadian, maxRadian));
            rockController.Scored += () => score.Value++;
            var transform = rock.Value.transform;
            transform.parent = pool;
            transform.position = rockSpawn.position;
            rockControllers.Add(rock.Key, rockController);
        }

        StartCoroutine(ThrowCoroutine(settings.RockCount, settings.TimeBetweenThrows.Min,
            settings.TimeBetweenThrows.Max));
    }

    private void FixedUpdate() => GetNextLandingPoint();

    /// <summary>
    /// Se obtiene la roca más próxima a caer de las rocas activas (en caso de que haya más de una roca activa al mismo tiempo),
    /// y se obtiene la posición estimada de aterrizaje de esa roca, exponiéndola en <see cref="NextRockLandingPoint"/>.
    /// </summary>
    private void GetNextLandingPoint()
    {
        int count = spawnPool.GetActiveKeys(activeKeys);
        if (count == 0)
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

    /// <summary>
    /// Corrutina para repetir la animación de lanzamiento en períodos aleatorios de tiempo
    /// <paramref name="minTime"/> - <paramref name="maxTime"/>, una determinada cantidad de veces <paramref name="count"/>
    /// </summary>
    /// <param name="count"></param>
    /// <param name="minTime"></param>
    /// <param name="maxTime"></param>
    /// <returns></returns>
    private IEnumerator ThrowCoroutine(int count, float minTime, float maxTime)
    {
        for (int i = 0; i < count; i++)
        {
            float randomTime = Random.Range(minTime, maxTime);
            float elapsedTime = 0f;
            while (elapsedTime < randomTime)
            {
                yield return null;
                elapsedTime += Time.deltaTime;
            }

            animator.SetTrigger(throwHash);
        }
    }

    /// <summary>
    /// Método para hacer aparecer una roca, llamado a través de evento de animación.
    /// </summary>
    private void Throw() => spawnPool.Dequeue().SetActive(true);
}