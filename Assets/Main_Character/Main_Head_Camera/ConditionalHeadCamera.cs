using UnityEngine;

public class ConditionalHeadCamera : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform headBone;
    [SerializeField] private Animator animator;

    [Header("Estados que permiten movimiento de cabeza")]
    [SerializeField] private string countingStateName = "Counting";
    [SerializeField] private string terrifiedStateName = "Terrified";

    [Header("Seguimiento durante animaciones especiales")]
    [SerializeField] private bool followHeadPosition = true;
    [SerializeField] private bool followHeadRotation = true;

    [Header("Suavizado")]
    [SerializeField] private bool useSmoothing = true;
    [SerializeField] private float positionSmoothSpeed = 18f;
    [SerializeField] private float rotationSmoothSpeed = 18f;

    /*
     * Posición y rotación inicial de la cabeza respecto
     * al objeto raíz del jugador.
     */
    private Vector3 stableHeadLocalPosition;
    private Quaternion stableHeadLocalRotation;

    private bool initialized;

    private void Awake()
    {
        if (playerRoot == null)
        {
            playerRoot = transform.parent;
        }

        if (animator == null && playerRoot != null)
        {
            animator = playerRoot.GetComponentInChildren<Animator>();
        }

        if (playerRoot == null)
        {
            Debug.LogError(
                "No se asignó el Player Root en ConditionalHeadCamera.",
                this
            );

            enabled = false;
            return;
        }

        if (headBone == null)
        {
            Debug.LogError(
                "No se asignó el hueso Head en ConditionalHeadCamera.",
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
        /*
         * Guardamos dónde está la cabeza inicialmente con
         * respecto al jugador. Esta será la posición estable
         * usada en Idle y Walk.
         */
        stableHeadLocalPosition =
            playerRoot.InverseTransformPoint(headBone.position);

        stableHeadLocalRotation =
            Quaternion.Inverse(playerRoot.rotation) *
            headBone.rotation;

        transform.SetPositionAndRotation(
            GetStableWorldPosition(),
            GetStableWorldRotation()
        );

        initialized = true;
    }

    private void LateUpdate()
    {
        if (!initialized)
        {
            return;
        }

        bool followAnimatedHead = IsSpecialAnimationPlaying();

        Vector3 targetPosition;
        Quaternion targetRotation;

        if (followAnimatedHead)
        {
            /*
             * Durante Counting o Terrified, utiliza la posición
             * y rotación reales de la cabeza animada.
             */
            targetPosition = followHeadPosition
                ? headBone.position
                : GetStableWorldPosition();

            targetRotation = followHeadRotation
                ? headBone.rotation
                : GetStableWorldRotation();
        }
        else
        {
            /*
             * Durante Idle y Walk, conserva una posición estable
             * respecto al cuerpo del jugador.
             */
            targetPosition = GetStableWorldPosition();
            targetRotation = GetStableWorldRotation();
        }

        ApplyCameraRootTransform(targetPosition, targetRotation);
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

        /*
         * También revisamos el siguiente estado para que el
         * seguimiento comience durante la transición.
         */
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

    private Vector3 GetStableWorldPosition()
    {
        return playerRoot.TransformPoint(stableHeadLocalPosition);
    }

    private Quaternion GetStableWorldRotation()
    {
        return playerRoot.rotation * stableHeadLocalRotation;
    }

    private void ApplyCameraRootTransform(
        Vector3 targetPosition,
        Quaternion targetRotation)
    {
        if (!useSmoothing)
        {
            transform.SetPositionAndRotation(
                targetPosition,
                targetRotation
            );

            return;
        }

        float positionInterpolation =
            1f - Mathf.Exp(
                -positionSmoothSpeed * Time.deltaTime
            );

        float rotationInterpolation =
            1f - Mathf.Exp(
                -rotationSmoothSpeed * Time.deltaTime
            );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            positionInterpolation
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationInterpolation
        );
    }
}