using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => instance;
    private static GameManager instance = null;

    public LevelBounds LevelBounds { get; private set; }
    public Transform Goal { get; private set; }

    public PlayerSettingsReference PlayerSettings => playerSettings;
    [SerializeField] private PlayerSettingsReference playerSettings = null;
    public RockThrowerSettingsReference RockThrowerSettings => rockThrowerSettings;
    [SerializeField] private RockThrowerSettingsReference rockThrowerSettings = null;
    public RockSettingsReference RockSettings => rockSettings;
    [SerializeField] private RockSettingsReference rockSettings = null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        Goal = GameObject.Find("goal").transform;
        LevelBounds = new LevelBounds(GameObject.Find("minBound").transform, GameObject.Find("maxBound").transform);
    }
}
