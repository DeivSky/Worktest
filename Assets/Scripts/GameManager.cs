using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } = null;
    public DataReference<float> GameTime { get; } = new DataReference<float>();
    [Range(10f, 600f)] [SerializeField]
    private float gameLength = 60f;
    public DataReference<int> Score { get; } = new DataReference<int>();
    public DataReference<int> Coins { get; } = new DataReference<int>();
    public LevelBounds LevelBounds { get; private set; } = null;
    public Transform Goal { get; private set; } = null;
    public GameObject Player { get; private set; } = null;
    public GameObject RockThrower { get; private set; } = null;

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
        Player = GameObject.Find("Hero");
        RockThrower = GameObject.Find("RockThrower");
        GameTime.Value = gameLength;
    }

    private void Update() => GameTime.Value -= Time.deltaTime;
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameManager))]
[CanEditMultipleObjects]
public class GameManagerEditor : Editor
{
    private GameManager prop;

    private void OnEnable() => prop = (GameManager) serializedObject.targetObject;

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            GUI.enabled = false;
            EditorGUILayout.FloatField(
                "Duración del juego",
                prop.GameTime.Value);
        }
        else
        {
            GUI.enabled = true;
            var property = serializedObject.FindProperty("gameLength");
            property.floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Duración del juego", property.floatValue), 5f, 600f);
        }
    }

    public override bool RequiresConstantRepaint() => Application.isPlaying;
}
#endif
