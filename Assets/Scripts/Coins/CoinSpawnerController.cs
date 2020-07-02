using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Controlador de instanciación de monedas.
/// </summary>
public class CoinSpawnerController : MonoBehaviour
{
    [SerializeField] private CoinSettingsReference coinSettings = null;
    [SerializeField] private Transform coinUI = null;
    private DataReference<int> score = null;
    private DataReference<int> coins = null;
    private SpawnPool spawnPool = null;
    private LevelBounds levelBounds = null;
    private Transform player = null;

    private int lastValue = 0;

    private void Awake()
    {
        var gm = GameManager.Instance;
        score = gm.Score;
        coins = gm.Coins;
        levelBounds = gm.LevelBounds;
        player = gm.Player.transform;

        spawnPool = new SpawnPool(2, coinSettings.Value.CoinPrefab);
        spawnPool.EnqueueAll();
        var queue = spawnPool.QueuedObjects;
        for (int i = 0; i < queue.Length; i++)
            InitializeCoin(queue[i]);
    }

    private void Update()
    {
        int value = score.Value;
        if (value == 0 || lastValue == value) return;
        lastValue = value;

        if ((float) value % coinSettings.Value.ScoreToSpawn != 0f) return;

        spawnPool.RequeueInactive();
        if (spawnPool.QueueCount == 0)
        {
            spawnPool.Enqueue();
            InitializeCoin(spawnPool.Peek());
        }

        SpawnCoin();
    }

    private void SpawnCoin()
    {
        Vector2 min = levelBounds.Min;
        float x = Random.Range(min.x, levelBounds.Max.x);
        float playerX = player.position.x;
        while (Mathf.Abs(playerX - x) < 2f)
            x = Random.Range(min.x, levelBounds.Max.x);

        GameObject coin = spawnPool.Dequeue();
        coin.transform.position = new Vector2(x, min.y);
        coin.SetActive(true);
    }

    private void InitializeCoin(GameObject gameObject)
    {
        var coin = gameObject.GetComponent<CoinController>();
        coin.Lifetime = coinSettings.Value.Lifetime;
        coin.AnimationDuration = coinSettings.Value.AnimationDuration;
        coin.CoinUI = coinUI;
        coin.CoinGrabbed += () => coins.Value++;
    }
}