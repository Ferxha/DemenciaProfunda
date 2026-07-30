using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class DeepGameManager : MonoBehaviour
{
    public enum GameState
    {
        ElevatorDescending,
        FloorIntroduction,
        Exploring,
        ReturningToElevator,
        ChangingFloor,
        GameOver,
        Ending
    }

    [Header("Pisos")]
    [SerializeField] private FloorData[] floors;

    [Header("Elevador")]
    [SerializeField] private ElevatorDoors elevatorDoors;
    [SerializeField] private ElevatorCameraShake elevatorShake;

    [Header("Panel del elevador")]
    [SerializeField] private GameObject groundFloorPanel;

    [Header("Jugador")]
    [SerializeField] private GameObject player;
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private FirstPersonInteractionDetector interactionDetector;

    [Tooltip("Script que controla la cámara con el mouse.")]
    [SerializeField] private FirstPersonCameraLook cameraLookScript;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("Audio")]
    [SerializeField] private AudioSource voiceAudioSource;

    [Header("Secuencias")]
    [SerializeField] private float initialElevatorDuration = 3f;
    [SerializeField] private float floorChangeDelay = 2f;
    [SerializeField] private float elevatorShakeDuration = 2.5f;
    [SerializeField] private float finalElevatorDuration = 3f;
    [SerializeField] private float finalFadeDuration = 1.5f;

    [Header("Escenas")]
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private string videoSceneName = "EndingVideo";

    [Header("Efectos")]
    [SerializeField] private RealityFailureEffect failureEffect;
    [SerializeField] private RealityFlashEffect realityFlashEffect;
    [SerializeField] private HueShiftCycle levelThreeHueEffect;
    [SerializeField] private LastFloorPostProcessEffect levelOneEffect;

    private int currentFloorIndex;
    private float remainingTime;

    private bool timerRunning;
    private bool playerInsideElevator;
    private bool keyCollected;

    private GameState currentState;

    private const string ReturnObjective =
        "Vuelve al elevador";

    private void Start()
    {
        Time.timeScale = 1f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (gameplayPanel != null)
        {
            gameplayPanel.SetActive(true);
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.interactable = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        StartCoroutine(StartGameSequence());
    }

    private void Update()
    {
        UpdateTimer();
        CheckSuccessfulReturn();
    }

    private IEnumerator StartGameSequence()
    {
        DisablePlayerControl();
        DeactivateAllFloors();
        DeactivateAllFloorNumbers();

        currentFloorIndex = 0;

        yield return StartCoroutine(
            RunElevatorDescent(initialElevatorDuration)
        );

        yield return StartCoroutine(
            StartCurrentFloor()
        );
    }

    private IEnumerator StartCurrentFloor()
    {
        currentState = GameState.FloorIntroduction;

        keyCollected = false;
        playerInsideElevator = true;
        timerRunning = false;

        FloorData floor = floors[currentFloorIndex];

        DeactivateAllFloors();
        DeactivateAllFloorNumbers();

        if (floor.levelObject != null)
        {
            floor.levelObject.SetActive(true);
        }

        if (floor.panelNumberObject != null)
        {
            floor.panelNumberObject.SetActive(true);
        }

        if (floor.keyObject != null)
        {
            floor.keyObject.Initialize(this);
        }

        SetObjective(
            GetCurrentFloorObjective()
        );

        if (voiceAudioSource != null &&
            floor.introductionAudio != null)
        {
            voiceAudioSource.PlayOneShot(
                floor.introductionAudio
            );

            yield return new WaitForSeconds(
                floor.introductionAudio.length
            );
        }

        if (elevatorDoors != null)
        {
            elevatorDoors.OpenDoors();
        }

        remainingTime = floor.timeLimit;
        timerRunning = true;
        currentState = GameState.Exploring;

        UpdateTimerText();
        EnablePlayerControl();

        /*
         * Element 0 = Nivel 3.
         * Inicia el efecto de Hue Shift mientras
         * el jugador busca las llaves.
         */
        if (currentFloorIndex == 0)
        {
            if (levelThreeHueEffect != null)
            {
                levelThreeHueEffect.StartKeysSearchEffect();
            }
            else
            {
                Debug.LogWarning(
                    "Level Three Hue Effect no está asignado.",
                    this
                );
            }
        }

        /*
         * Element 2 = Nivel 1.
         * Activa Chromatic Aberration y Color Filter rosa.
         */
        if (currentFloorIndex == 2)
        {
            if (levelOneEffect != null)
            {
                levelOneEffect.ApplyLastFloorEffect();
            }
            else
            {
                Debug.LogWarning(
                    "Level One Effect no está asignado.",
                    this
                );
            }
        }

        if (floor.whisperAudio != null)
        {
            StartCoroutine(
                PlayWhisperAfterDelay(
                    floor.whisperAudio,
                    floor.whisperDelay
                )
            );
        }
    }

    private string GetCurrentFloorObjective()
    {
        /*
         * Element 0 = Nivel 3
         * Element 1 = Nivel 2
         * Element 2 = Nivel 1
         */
        switch (currentFloorIndex)
        {
            case 0:
                return "Encontrar las llaves";

            case 1:
                return "Encontrar la cámara";

            case 2:
                return "Encontrar el transmisor";

            default:
                return "Encontrar el objeto";
        }
    }

    private IEnumerator PlayWhisperAfterDelay(
        AudioClip clip,
        float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentState != GameState.Exploring)
        {
            yield break;
        }

        if (voiceAudioSource != null &&
            clip != null)
        {
            voiceAudioSource.PlayOneShot(clip);
        }
    }

    private void UpdateTimer()
    {
        if (!timerRunning)
        {
            return;
        }

        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(
            remainingTime,
            0f
        );

        UpdateTimerText();

        if (remainingTime <= 0f)
        {
            timerRunning = false;

            StartCoroutine(
                FailSequence()
            );
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds =
            Mathf.CeilToInt(remainingTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text =
            $"{minutes:00}:{seconds:00}";
    }

    public void OnKeyObjectCollected(
        InteractableKeyObject collectedObject)
    {
        if (currentState != GameState.Exploring)
        {
            return;
        }

        if (collectedObject !=
            floors[currentFloorIndex].keyObject)
        {
            return;
        }

        keyCollected = true;
        currentState = GameState.ReturningToElevator;

        /*
         * Detiene el Hue Shift cuando se recogen
         * las llaves del Nivel 3.
         */
        if (currentFloorIndex == 0 &&
            levelThreeHueEffect != null)
        {
            levelThreeHueEffect.StopKeysSearchEffect();
        }

        SetObjective(ReturnObjective);

        if (floors[currentFloorIndex].useRealityFlash &&
            realityFlashEffect != null)
        {
            realityFlashEffect.PlayEffect();
        }
    }

    public void SetPlayerInsideElevator(bool inside)
    {
        playerInsideElevator = inside;
    }

    private void CheckSuccessfulReturn()
    {
        if (currentState != GameState.ReturningToElevator)
        {
            return;
        }

        if (!keyCollected ||
            !playerInsideElevator ||
            remainingTime <= 0f)
        {
            return;
        }

        timerRunning = false;

        StartCoroutine(
            CompleteCurrentFloor()
        );
    }

    private IEnumerator CompleteCurrentFloor()
    {
        currentState = GameState.ChangingFloor;
        timerRunning = false;

        DisablePlayerControl();

        if (elevatorDoors != null)
        {
            elevatorDoors.CloseDoors();
        }

        yield return new WaitForSeconds(
            floorChangeDelay
        );

        currentFloorIndex++;

        /*
         * Ya no quedan pisos.
         * Las puertas ya están cerradas y comienza
         * la secuencia final hacia el video.
         */
        if (currentFloorIndex >= floors.Length)
        {
            yield return StartCoroutine(
                FinalElevatorSequence()
            );

            yield break;
        }

        yield return StartCoroutine(
            RunElevatorDescent(
                elevatorShakeDuration
            )
        );

        yield return StartCoroutine(
            StartCurrentFloor()
        );
    }

    private IEnumerator RunElevatorDescent(
        float duration)
    {
        currentState =
            GameState.ElevatorDescending;

        timerRunning = false;

        if (elevatorShake != null)
        {
            elevatorShake.PlayShake(duration);
        }

        yield return new WaitForSeconds(duration);
    }

    private IEnumerator FailSequence()
    {
        if (currentState == GameState.GameOver)
        {
            yield break;
        }

        currentState = GameState.GameOver;
        timerRunning = false;

        DisablePlayerControl();

        if (levelThreeHueEffect != null)
        {
            levelThreeHueEffect.StopKeysSearchEffect();
        }

        /*
         * Detiene la rotación de cámara con el mouse
         * para poder utilizar el cursor.
         */
        if (cameraLookScript != null)
        {
            cameraLookScript.enabled = false;
        }

        if (failureEffect != null)
        {
            yield return failureEffect.PlayFailure();
        }

        if (gameplayPanel != null)
        {
            gameplayPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator FinalElevatorSequence()
    {
        currentState = GameState.Ending;
        timerRunning = false;

        DisablePlayerControl();

        if (levelThreeHueEffect != null)
        {
            levelThreeHueEffect.StopKeysSearchEffect();
        }

        DeactivateAllFloorNumbers();

        if (groundFloorPanel != null)
        {
            groundFloorPanel.SetActive(true);
        }

        /*
         * Las puertas permanecen cerradas.
         * Se ejecuta el movimiento final del elevador.
         */
        yield return StartCoroutine(
            RunElevatorDescent(
                finalElevatorDuration
            )
        );

        /*
         * Fade a negro antes de cambiar
         * a la escena del video.
         */
        yield return StartCoroutine(
            FadeToBlack()
        );

        SceneManager.LoadSceneAsync(
            videoSceneName
        );
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = true;

        float startAlpha =
            fadeCanvasGroup.alpha;

        float elapsed = 0f;

        while (elapsed < finalFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / finalFadeDuration
            );

            fadeCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                1f,
                progress
            );

            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;

        SceneManager.LoadScene(
            menuSceneName
        );
    }

    private void SetObjective(string objective)
    {
        if (objectiveText != null)
        {
            objectiveText.text = objective;
        }
    }

    private void EnablePlayerControl()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        if (interactionDetector != null)
        {
            interactionDetector.enabled = true;
        }
    }

    private void DisablePlayerControl()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        if (interactionDetector != null)
        {
            interactionDetector.enabled = false;
        }
    }

    private void DeactivateAllFloors()
    {
        foreach (FloorData floor in floors)
        {
            if (floor.levelObject != null)
            {
                floor.levelObject.SetActive(false);
            }
        }
    }

    private void DeactivateAllFloorNumbers()
    {
        foreach (FloorData floor in floors)
        {
            if (floor.panelNumberObject != null)
            {
                floor.panelNumberObject.SetActive(false);
            }
        }

        if (groundFloorPanel != null)
        {
            groundFloorPanel.SetActive(false);
        }
    }
}