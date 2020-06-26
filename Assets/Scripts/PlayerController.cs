using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    private PlayerSettingsReference playerSettings = null;
    private LevelBounds levelBounds = null;
    
    private Animator animator = null;
    private SpriteRenderer renderer = null;
    private ParticleSystem particleSystem = null;
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
        playerSettings = GameManager.Instance.PlayerSettings;
#if UNITY_ANDROID && !UNITY_EDITOR
        touchAction = GetComponent<PlayerInput>().actions.FindAction("Touch", true);
#endif
        moveAction = GetComponent<PlayerInput>().actions.FindAction("Movement", true);
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        particleSystem = GetComponent<ParticleSystem>();
        mainModule = particleSystem.main;
        shapeModule = particleSystem.shape;
        levelBounds = GameManager.Instance.LevelBounds;
    }

    private void Update()
    {
        PollInput();
        Move();
    }

    private void Move()
    {
        bool isMoving = processedInput != 0f;
        animator.SetBool(isMovingHash, isMoving);
        
        if (!isMoving) return;

        Vector3 position = transform.position;
        if(processedInput > 0f && position.x >= levelBounds.Max.x) return;
        if(processedInput < 0f && position.x <= levelBounds.Min.x) return;
        
        position += Vector3.right * (processedInput * playerSettings.Value.Speed * Time.deltaTime);
        transform.position = position;
    }

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
        
        PlayDustParticles(input < 0f);
    }

    private void PlayDustParticles(bool facingLeft)
    {
        renderer.flipX = facingLeft;
        shapeModule.scale = new Vector3(facingLeft ? -1f : 1f, 1f, 1f);
        mainModule.startRotationY = facingLeft ?  180f * Mathf.Deg2Rad : 0f;
        particleSystem.Play();
    }

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
