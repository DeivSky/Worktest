using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private PlayerSettingsReference playerSettings = null;
    private LevelBounds levelBounds = null;
    
    private Animator animator = null;
    private new ParticleSystem particleSystem = null;
    private InputAction action = null;
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private float rawInput = 0f;
    private float processedInput = 0f;
    private Coroutine coroutine = null;
#if UNITY_ANDROID
    private float halfScreenWidth = Screen.width / 2f;
#endif

    private void Awake()
    {
        playerSettings = GameManager.Instance.PlayerSettings;
        action = GetComponent<PlayerInput>().actions.FindAction("Movement", true);
        animator = GetComponent<Animator>();
        particleSystem = GetComponent<ParticleSystem>();
        levelBounds = GameManager.Instance.LevelBounds;
    }

    private void OnEnable()
    {
        action.started += OnMovementInput;
        action.canceled += OnMovementInput;
    }
    
    private void OnDisable()
    {
        action.started -= OnMovementInput;
        action.canceled -= OnMovementInput;
    }

    private void Update() => Move();

    private void Move()
    {
        bool isMoving = processedInput != 0f;
        animator.SetBool(this.isMovingHash, isMoving);
        
        if (!isMoving) return;

        Vector3 position = transform.position;
        if(processedInput > 0f && position.x >= levelBounds.RightBound) return;
        if(processedInput < 0f && position.x <= levelBounds.LeftBound) return;
        
        position += Vector3.right * (processedInput * playerSettings.Value.Speed * Time.deltaTime);
        transform.position = position;
    }

    private void OnMovementInput(InputAction.CallbackContext obj)
    {
        
        float input = 0f;
        if (!obj.canceled)
        {
            input = obj.ReadValue<float>();
#if UNITY_ANDROID
            input = input >= halfScreenSize ? 1f : -1f;
#endif
        }

        if (input == rawInput) return;
        
        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = 
            StartCoroutine(LerpMovement(processedInput, input, playerSettings.Value.InertiaDuration));

        rawInput = input;
        
        if (input == 0f) return;
        
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * input;
        transform.localScale = localScale;
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
