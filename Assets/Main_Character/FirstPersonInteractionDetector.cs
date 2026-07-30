using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class FirstPersonInteractionDetector : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float detectionDistance = 8f;

    [SerializeField] private LayerMask interactableLayers;

    [Header("Depuración")]
    [SerializeField] private bool showDebugRay = true;

    private Camera playerCamera;
    private InteractableKeyObject currentTarget;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError(
                "No se encontró una Camera en este objeto.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        DetectObject();
        HandleInteraction();
    }

    private void DetectObject()
    {
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        bool detected = Physics.Raycast(
            ray,
            out RaycastHit hit,
            detectionDistance,
            interactableLayers,
            QueryTriggerInteraction.Ignore
        );

        if (showDebugRay)
        {
            Debug.DrawRay(
                ray.origin,
                ray.direction * detectionDistance,
                detected ? Color.green : Color.red
            );
        }

        if (!detected)
        {
            ClearCurrentTarget();
            return;
        }

        InteractableKeyObject detectedTarget =
            hit.collider.GetComponentInParent<InteractableKeyObject>();

        if (detectedTarget == null)
        {
            ClearCurrentTarget();
            return;
        }

        if (currentTarget != detectedTarget)
        {
            ClearCurrentTarget();
            currentTarget = detectedTarget;
        }

        currentTarget.SetLookState(
            true,
            hit.distance
        );
    }

    private void HandleInteraction()
    {
        if (currentTarget == null ||
            Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame &&
            currentTarget.CanInteract())
        {
            currentTarget.Collect();
            ClearCurrentTarget();
        }
    }

    private void ClearCurrentTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.SetLookState(false, 0f);
            currentTarget = null;
        }
    }

    private void OnDisable()
    {
        ClearCurrentTarget();
    }
}