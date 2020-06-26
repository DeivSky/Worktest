using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } = null;

    public LevelBounds LevelBounds { get; private set; }
    public Transform Goal { get; private set; }
    public SpawnPool SpawnPool { get; private set; }
    public PlayerController PlayerController { get; private set; }
    public RockThrowerController RockThrowerController { get; private set; }

    public PlayerSettingsReference PlayerSettings => playerSettings;
    [SerializeField] private PlayerSettingsReference playerSettings = null;
    public RockThrowerSettingsReference RockThrowerSettings => rockThrowerSettings;
    [SerializeField] private RockThrowerSettingsReference rockThrowerSettings = null;
    public RockSettingsReference RockSettings => rockSettings;
    [SerializeField] private RockSettingsReference rockSettings = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Goal = GameObject.Find("goal").transform;
        LevelBounds = new LevelBounds(GameObject.Find("minBound").transform, GameObject.Find("maxBound").transform);
        PlayerController = GameObject.Find("Hero").GetComponent<PlayerController>();
        RockThrowerController = GameObject.Find("RockThrower").GetComponent<RockThrowerController>();
        SpawnPool = new SpawnPool(RockThrowerSettings.Value.RockCount, RockThrowerSettings.Value.RockPrefab);
    }
}
