using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonCharacterController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;

    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Animaciones de inactividad")]
    [SerializeField] private float specialIdleDelay = 7f;

    private CharacterController characterController;

    private Vector2 movementInput;
    private float verticalVelocity;
    private float idleTimer;

    private bool isWalking;
    private bool hasPlayerInput;
    private bool playingSpecialIdle;
    private bool specialAnimationStarted;

    // false = Counting
    // true = Terrified
    private bool playTerrifiedNext;

    private static readonly int IsWalkingHash =
        Animator.StringToHash("IsWalking");

    private static readonly int CountingHash =
        Animator.StringToHash("Counting");

    private static readonly int TerrifiedHash =
        Animator.StringToHash("Terrified");

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (characterController == null)
        {
            Debug.LogError(
                "No se encontró CharacterController en el jugador.",
                this
            );

            enabled = false;
            return;
        }

        if (animator == null)
        {
            Debug.LogError(
                "No se encontró el Animator del personaje.",
                this
            );

            enabled = false;
        }
    }

    private void Start()
    {
        movementInput = Vector2.zero;
        verticalVelocity = 0f;
        idleTimer = 0f;

        isWalking = false;
        hasPlayerInput = false;
        playingSpecialIdle = false;
        specialAnimationStarted = false;

        // La primera animación especial será Counting.
        playTerrifiedNext = false;
    }

    private void Update()
    {
        ReadMovementInput();

        if (characterController != null &&
            characterController.enabled &&
            gameObject.activeInHierarchy)
        {
            HandleMovement();
        }

        HandleAnimations();
    }

    private void ReadMovementInput()
    {
        movementInput = Vector2.zero;

        if (Keyboard.current == null)
        {
            isWalking = false;
            hasPlayerInput = false;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        // W: avanzar
        if (Keyboard.current.wKey.isPressed)
        {
            vertical += 1f;
        }

        // S: retroceder
        if (Keyboard.current.sKey.isPressed)
        {
            vertical -= 1f;
        }

        // A: desplazamiento hacia la izquierda
        if (Keyboard.current.aKey.isPressed)
        {
            horizontal -= 1f;
        }

        // D: desplazamiento hacia la derecha
        if (Keyboard.current.dKey.isPressed)
        {
            horizontal += 1f;
        }

        movementInput = Vector2.ClampMagnitude(
            new Vector2(horizontal, vertical),
            1f
        );

        isWalking = movementInput.sqrMagnitude > 0.01f;
        hasPlayerInput = isWalking;
    }

    private void HandleMovement()
    {
        /*
         * El movimiento se calcula respecto a la orientación
         * actual del jugador. Esa orientación será controlada
         * horizontalmente por el mouse.
         */
        Vector3 horizontalMovement =
            transform.right * movementInput.x +
            transform.forward * movementInput.y;

        horizontalMovement *= walkSpeed;

        if (characterController.isGrounded &&
            verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMovement = horizontalMovement;
        finalMovement.y = verticalVelocity;

        characterController.Move(
            finalMovement * Time.deltaTime
        );
    }

    private void HandleAnimations()
    {
        animator.SetBool(IsWalkingHash, isWalking);

        /*
         * Cualquier movimiento WASD interrumpe las
         * animaciones especiales y reinicia el contador.
         */
        if (hasPlayerInput)
        {
            CancelSpecialIdle();
            return;
        }

        if (playingSpecialIdle)
        {
            CheckSpecialAnimationState();
            return;
        }

        idleTimer += Time.deltaTime;

        if (idleTimer >= specialIdleDelay)
        {
            PlayNextSpecialIdle();
        }
    }

    private void PlayNextSpecialIdle()
    {
        idleTimer = 0f;
        playingSpecialIdle = true;
        specialAnimationStarted = false;

        if (playTerrifiedNext)
        {
            animator.ResetTrigger(CountingHash);
            animator.SetTrigger(TerrifiedHash);
        }
        else
        {
            animator.ResetTrigger(TerrifiedHash);
            animator.SetTrigger(CountingHash);
        }

        playTerrifiedNext = !playTerrifiedNext;
    }

    private void CheckSpecialAnimationState()
    {
        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);

        bool isCounting =
            stateInfo.IsName("Counting");

        bool isTerrified =
            stateInfo.IsName("Terrified");

        if (isCounting || isTerrified)
        {
            specialAnimationStarted = true;
            return;
        }

        if (specialAnimationStarted &&
            stateInfo.IsName("Idle") &&
            !animator.IsInTransition(0))
        {
            playingSpecialIdle = false;
            specialAnimationStarted = false;
            idleTimer = 0f;
        }
    }

    private void CancelSpecialIdle()
    {
        idleTimer = 0f;
        playingSpecialIdle = false;
        specialAnimationStarted = false;

        animator.ResetTrigger(CountingHash);
        animator.ResetTrigger(TerrifiedHash);
    }
}