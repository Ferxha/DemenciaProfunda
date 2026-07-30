using System.Collections;
using UnityEngine;

public class InteractableKeyObject : MonoBehaviour
{
    [Header("Referencias visuales")]
    [Tooltip("Objeto duplicado con el material de contorno.")]
    [SerializeField] private GameObject outlineObject;

    [Tooltip("Modelo normal que desaparecerá al recogerlo.")]
    [SerializeField] private GameObject visualObject;

    [Tooltip("Canvas en World Space que contiene el texto Press E.")]
    [SerializeField] private GameObject interactionCanvas;

    [Header("Interacción")]
    [SerializeField] private float interactionDistance = 2.2f;

    [Header("Efecto especial")]
    [SerializeField] private bool useRealityFlash;

    [SerializeField] private RealityFlashEffect realityFlashEffect;

    private DeepGameManager gameManager;

    private bool collected;
    private bool playerIsLooking;
    private bool interactionLocked;

    private float currentDistance;

    public bool IsCollected => collected;

    private void Awake()
    {
        HideOutline();
        HideInteractionMessage();
    }

    public void Initialize(DeepGameManager manager)
    {
        gameManager = manager;

        collected = false;
        playerIsLooking = false;
        interactionLocked = false;
        currentDistance = 0f;

        if (visualObject != null)
        {
            visualObject.SetActive(true);
        }

        HideOutline();
        HideInteractionMessage();
    }

    public void SetLookState(bool isLooking, float distance)
    {
        if (collected || interactionLocked)
        {
            HideOutline();
            HideInteractionMessage();
            return;
        }

        playerIsLooking = isLooking;
        currentDistance = distance;

        if (playerIsLooking)
        {
            ShowOutline();
        }
        else
        {
            HideOutline();
        }

        if (CanInteract())
        {
            ShowInteractionMessage();
        }
        else
        {
            HideInteractionMessage();
        }
    }

    public bool CanInteract()
    {
        return
            !collected &&
            !interactionLocked &&
            playerIsLooking &&
            currentDistance <= interactionDistance;
    }

    public void Collect()
    {
        if (!CanInteract())
        {
            return;
        }

        HideInteractionMessage();
        HideOutline();

        if (useRealityFlash && realityFlashEffect != null)
        {
            StartCoroutine(CollectWithRealityFlash());
        }
        else
        {
            CompleteCollection();
        }
    }

    private IEnumerator CollectWithRealityFlash()
    {
        interactionLocked = true;
        playerIsLooking = false;

        realityFlashEffect.PlayEffect();

        while (realityFlashEffect.IsPlaying)
        {
            yield return null;
        }

        CompleteCollection();
    }

    private void CompleteCollection()
    {
        collected = true;
        interactionLocked = false;

        HideOutline();
        HideInteractionMessage();

        if (gameManager != null)
        {
            gameManager.OnKeyObjectCollected(this);
        }

        if (!useRealityFlash && visualObject != null)
        {
            visualObject.SetActive(false);
        }
    }

    public void ShowOutline()
    {
        if (outlineObject != null &&
            !collected &&
            !interactionLocked)
        {
            outlineObject.SetActive(true);
        }
    }

    public void HideOutline()
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(false);
        }
    }

    private void ShowInteractionMessage()
    {
        if (interactionCanvas != null &&
            !interactionCanvas.activeSelf)
        {
            interactionCanvas.SetActive(true);
        }
    }

    private void HideInteractionMessage()
    {
        if (interactionCanvas != null &&
            interactionCanvas.activeSelf)
        {
            interactionCanvas.SetActive(false);
        }
    }

    private void OnDisable()
    {
        HideOutline();
        HideInteractionMessage();
    }
}