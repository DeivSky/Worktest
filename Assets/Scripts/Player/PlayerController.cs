using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador del jugador.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerSettingsReference playerSettings = null;

    private LevelBounds levelBounds = null;
    private Animator animator = null;
    private new SpriteRenderer renderer = null;
    private new ParticleSystem particleSystem = null;
    private ParticleSystem.ShapeModule shapeModule;
    private ParticleSystem.MainModule mainModule;
    private InputAction moveAction = null;
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private float rawInput = 0f;
    private float processedInput = 0f;
    private Coroutine coroutine = null;
#if UNITY_ANDROID && !UNITY_EDITOR
    private InputAction touchAction = null;
    private readonly float halfScreenWidth = Screen.width / 2f;
#endif

    private void Awake()
    {
        levelBounds = GameManager.Instance.LevelBounds;
        var playerInput = GetComponent<PlayerInput>();
#if UNITY_ANDROID && !UNITY_EDITOR
        touchAction = playerInput.actions.FindAction("Touch", true);
#endif
        moveAction = playerInput.actions.FindAction("Movement", true);
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        particleSystem = GetComponent<ParticleSystem>();
        mainModule = particleSystem.main;
        shapeModule = particleSystem.shape;
    }

    private void Update()
    {
        PollInput();
        Move();
    }

    /// <summary>
    /// Método para obtener el Input.
    /// Se está utilizando el nuevo sistema de input <see cref="UnityEngine.InputSystem"/>.
    /// Al arrancar, frenar o cambiar de dirección, se utiliza una corrutina para simular la inercia del movimiento, y
    /// se utiliza el sistema de partículas para producir el efecto del polvo.
    /// También gira el sprite del personaje si está yendo a la izquierda.
    /// </summary>
    private void PollInput()
    {
        float input = 0;
#if UNITY_ANDROID && !UNITY_EDITOR
        input = touchAction.ReadValue<float>() > 0f ? moveAction.ReadValue<float>() >= halfScreenWidth ? 1f : -1f : 0f;
#else
        input = moveAction.ReadValue<float>();
#endif

        if (input == rawInput) return;

        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine =
            StartCoroutine(LerpMovement(processedInput, input, playerSettings.Value.InertiaDuration));

        rawInput = input;

        if (input == 0f) return;

        bool facingLeft = input < 0f;
        renderer.flipX = facingLeft;
        PlayDustParticles(facingLeft);
    }

    /// <summary>
    /// Método para el movimiento del personaje.
    /// Mueve al personaje en la dirección del input procesado (<see cref="processedInput"/>) por las corrutinas,
    /// por la velocidad configurada, acciona la animación de caminar del personaje en caso de estarse moviendo,
    /// y limita el movimiento del personaje a dentro de la caja de juego.
    /// </summary>
    private void Move()
    {
        bool isMoving = processedInput != 0f;
        animator.SetBool(isMovingHash, isMoving);

        if (!isMoving) return;

        Vector3 position = transform.position;
        if (processedInput > 0f && position.x >= levelBounds.Max.x) return;
        if (processedInput < 0f && position.x <= levelBounds.Min.x) return;

        position += Vector3.right * (processedInput * playerSettings.Value.Speed * Time.deltaTime);
        transform.position = position;
    }

    /// <summary>
    /// Se reproducen las partículas de polvo y se giran de hacer falta <paramref name="facingLeft"/>.
    /// </summary>
    /// <param name="facingLeft"></param>
    private void PlayDustParticles(bool facingLeft)
    {
        shapeModule.scale = new Vector3(facingLeft ? -1f : 1f, 1f, 1f);
        mainModule.startRotationY = facingLeft ? 180f * Mathf.Deg2Rad : 0f;
        particleSystem.Play();
    }

    /// <summary>
    /// Corrutina de interpolación de input.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator LerpMovement(float from, float to, float time)
    {
        float elapsedTime = 0f;
        while (time > elapsedTime)
        {
            processedInput = Mathf.Lerp(from, to, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        processedInput = to;
        coroutine = null;
    }
}