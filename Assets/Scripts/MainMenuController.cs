using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundRenderer = null;
    private Transform background = null;
    [SerializeField] private float backgroundSpeed = 2f;

    private void Awake() => background = backgroundRenderer.transform;

    private void Update()
    {
        float d = backgroundSpeed * Time.deltaTime;
        backgroundRenderer.size += Vector2.right * d;
        background.position += Vector3.left * d;
    }

    public void Play() => SceneManager.LoadScene(1, LoadSceneMode.Single);

#if !UNITY_EDITOR
    public void Exit() => Application.Quit();
#else
    public void Exit() => UnityEditor.EditorApplication.isPlaying = false;
#endif
}
