using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FirstPersonObjectDetector : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float detectionDistance = 4f;

    [Tooltip("Capas que puede detectar el Raycast.")]
    [SerializeField] private LayerMask interactableLayers;

    [Tooltip("Permite visualizar el Raycast en la ventana Scene.")]
    [SerializeField] private bool showDebugRay = true;

    private Camera playerCamera;
    private OutlineTarget currentTarget;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError(
                "FirstPersonObjectDetector necesita una Camera.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        DetectObject();
    }

    private void DetectObject()
    {
        /*
         * El punto 0.5, 0.5 representa exactamente
         * el centro de la pantalla.
         */
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (showDebugRay)
        {
            Debug.DrawRay(
                ray.origin,
                ray.direction * detectionDistance
            );
        }

        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                detectionDistance,
                interactableLayers,
                QueryTriggerInteraction.Ignore))
        {
            OutlineTarget detectedTarget =
                hit.collider.GetComponentInParent<OutlineTarget>();

            if (detectedTarget != null)
            {
                SetCurrentTarget(detectedTarget);
                return;
            }
        }

        ClearCurrentTarget();
    }

    private void SetCurrentTarget(OutlineTarget newTarget)
    {
        // Ya estamos mirando el mismo objeto.
        if (currentTarget == newTarget)
        {
            return;
        }

        // Oculta el contorno del objeto anterior.
        if (currentTarget != null)
        {
            currentTarget.HideOutline();
        }

        currentTarget = newTarget;
        currentTarget.ShowOutline();
    }

    private void ClearCurrentTarget()
    {
        if (currentTarget == null)
        {
            return;
        }

        currentTarget.HideOutline();
        currentTarget = null;
    }

    private void OnDisable()
    {
        ClearCurrentTarget();
    }
}
