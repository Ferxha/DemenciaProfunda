using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCameraLook : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Objeto raíz que contiene el CharacterController.")]
    [SerializeField] private Transform playerRoot;

    [Tooltip("Animator del personaje.")]
    [SerializeField] private Animator animator;

    [Header("Sensibilidad")]
    [SerializeField] private float horizontalSensitivity = 0.12f;
    [SerializeField] private float verticalSensitivity = 0.12f;

    [Header("Límite vertical")]
    [SerializeField] private float minVerticalAngle = -60f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Vista horizontal durante animaciones especiales")]
    [SerializeField] private float minSpecialHorizontalAngle = -60f;
    [SerializeField] private float maxSpecialHorizontalAngle = 60f;

    [Header("Estados especiales")]
    [SerializeField] private string countingStateName = "Counting";
    [SerializeField] private string terrifiedStateName = "Terrified";

    [Header("Suavizado")]
    [SerializeField] private bool useSmoothing = true;
    [SerializeField] private float smoothSpeed = 15f;

    [Header("Cursor")]
    [SerializeField] private bool lockCursorOnStart = true;

    private float currentPitch;
    private float specialYaw;

    private Quaternion initialLocalRotation;
    private Quaternion targetLocalRotation;

    private void Awake()
    {
        if (playerRoot == null)
        {
            Debug.LogError(
                "No se asignó Player Root en FirstPersonCameraLook.",
                this
            );

            enabled = false;
            return;
        }

        if (animator == null)
        {
            animator = playerRoot.GetComponentInChildren<Animator>();
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
        initialLocalRotation = transform.localRotation;
        targetLocalRotation = initialLocalRotation;

        currentPitch = 0f;
        specialYaw = 0f;

        if (lockCursorOnStart)
        {
            LockCursor();
        }
    }

    private void Update()
    {
        ReadMouseInput();
        HandleCursor();
    }

    private void LateUpdate()
    {
        ApplyCameraRotation();
    }

    private void ReadMouseInput()
    {
        if (Mouse.current == null ||
            Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();

        float horizontalMouse =
            mouseDelta.x * horizontalSensitivity;

        float verticalMouse =
            mouseDelta.y * verticalSensitivity;

        /*
         * La vista vertical siempre se controla desde
         * CameraPivot y conserva sus límites.
         */
        currentPitch -= verticalMouse;

        currentPitch = Mathf.Clamp(
            currentPitch,
            minVerticalAngle,
            maxVerticalAngle
        );

        bool isSpecialAnimation =
            IsSpecialAnimationPlaying();

        if (isSpecialAnimation)
        {
            /*
             * Durante Counting o Terrified:
             *
             * - El cuerpo no gira.
             * - La cámara puede mirar horizontalmente
             *   dentro de un rango limitado.
             */
            specialYaw += horizontalMouse;

            specialYaw = Mathf.Clamp(
                specialYaw,
                minSpecialHorizontalAngle,
                maxSpecialHorizontalAngle
            );
        }
        else
        {
            /*
             * Durante Idle y Walk:
             *
             * - El movimiento horizontal del mouse gira
             *   todo el personaje sobre el eje Y.
             * - La cámara queda centrada horizontalmente.
             */
            playerRoot.Rotate(
                0f,
                horizontalMouse,
                0f,
                Space.Self
            );

            specialYaw = 0f;
        }

        Quaternion mouseRotation =
            Quaternion.Euler(
                currentPitch,
                specialYaw,
                0f
            );

        targetLocalRotation =
            initialLocalRotation * mouseRotation;
    }

    private bool IsSpecialAnimationPlaying()
    {
        AnimatorStateInfo currentState =
            animator.GetCurrentAnimatorStateInfo(0);

        bool currentIsSpecial =
            currentState.IsName(countingStateName) ||
            currentState.IsName(terrifiedStateName);

        if (currentIsSpecial)
        {
            return true;
        }

        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState =
                animator.GetNextAnimatorStateInfo(0);

            return
                nextState.IsName(countingStateName) ||
                nextState.IsName(terrifiedStateName);
        }

        return false;
    }

    private void ApplyCameraRotation()
    {
        if (!useSmoothing)
        {
            transform.localRotation =
                targetLocalRotation;

            return;
        }

        float interpolation =
            1f - Mathf.Exp(
                -smoothSpeed * Time.deltaTime
            );

        transform.localRotation =
            Quaternion.Slerp(
                transform.localRotation,
                targetLocalRotation,
                interpolation
            );
    }

    private void HandleCursor()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}