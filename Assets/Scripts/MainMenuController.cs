using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador del menú principal. Controla las acciones del menú y el movimiento del fondo.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private LevelDifficulty gamePreset = null;
    [SerializeField] private TextMeshProUGUI difficultyText = null;
    [SerializeField] private SpriteRenderer backgroundRenderer = null;
    [SerializeField] private float backgroundSpeed = 2f;
    private Transform background = null;
    private bool isHard = true;

    private void Awake()
    {
        background = backgroundRenderer.transform;
        difficultyText.text = gamePreset.DifficultyMode.ToString();
    }

    private void Update()
    {
        float d = backgroundSpeed * Time.deltaTime;
        backgroundRenderer.size += Vector2.right * d;
        background.position += Vector3.left * d;
    }

    public void Play() => SceneManager.LoadScene(1, LoadSceneMode.Single);

    public void ToggleDifficulty()
    {
        isHard = !isHard;
        gamePreset.DifficultyMode = isHard ? LevelDifficulty.Mode.Hard : LevelDifficulty.Mode.Easy;
        difficultyText.text = gamePreset.DifficultyMode.ToString();
    }

    public void Exit() =>
#if !UNITY_EDITOR
        Application.Quit();
#else
        UnityEditor.EditorApplication.isPlaying = false;
#endif
}