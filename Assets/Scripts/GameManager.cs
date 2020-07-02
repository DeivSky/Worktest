using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton con la información principal del nivel.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } = null;
    public DataReference<float> GameTime { get; } = new DataReference<float>();
    [Range(10f, 600f)] [SerializeField] private float gameLength = 60f;
    public DataReference<int> Score { get; } = new DataReference<int>();
    public DataReference<int> Coins { get; } = new DataReference<int>();
    public LevelBounds LevelBounds { get; private set; } = null;
    public Transform Goal { get; private set; } = null;
    public GameObject Player { get; private set; } = null;
    public GameObject RockThrower { get; private set; } = null;
    public LevelDifficulty Difficulty => difficulty;
    [SerializeField] private LevelDifficulty difficulty = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (Difficulty != null)
            Difficulty.ApplyDifficulty();

        Goal = GameObject.Find("goal").transform;
        LevelBounds = new LevelBounds(GameObject.Find("minBound").transform, GameObject.Find("maxBound").transform);
        Player = GameObject.Find("Hero");
        RockThrower = GameObject.Find("RockThrower");
        GameTime.Value = gameLength;
    }

    private void Update()
    {
        GameTime.Value -= Time.deltaTime;
        if (GameTime.Value <= 0f)
            SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}