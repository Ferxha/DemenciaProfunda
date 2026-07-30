using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class EndingVideoController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1.5f;

    [Header("Escena del menú")]
    [SerializeField] private string menuSceneName = "Menu";

    private bool videoFinished;
    private bool changingScene;

    private void Awake()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.interactable = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnEnable()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
    }

    private void OnDisable()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;
    }

    private IEnumerator Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError(
                "EndingVideoController: no se asignó el VideoPlayer.",
                this
            );

            yield break;
        }

        /*
         * Prepara el video antes de quitar
         * el fondo negro.
         */
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();

        /*
         * Fade de entrada:
         * negro a transparente.
         */
        yield return StartCoroutine(
            Fade(
                1f,
                0f,
                fadeInDuration
            )
        );
    }

    private void OnVideoFinished(
        VideoPlayer source)
    {
        if (videoFinished ||
            changingScene)
        {
            return;
        }

        videoFinished = true;

        StartCoroutine(
            FinishVideoSequence()
        );
    }

    private void OnVideoError(
        VideoPlayer source,
        string message)
    {
        Debug.LogError(
            $"Error al reproducir el video: {message}",
            this
        );

        if (videoFinished ||
            changingScene)
        {
            return;
        }

        videoFinished = true;

        StartCoroutine(
            FinishVideoSequence()
        );
    }

    private IEnumerator FinishVideoSequence()
    {
        changingScene = true;

        /*
         * Fade de salida:
         * transparente a negro.
         */
        yield return StartCoroutine(
            Fade(
                0f,
                1f,
                fadeOutDuration
            )
        );

        Time.timeScale = 1f;

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;

        SceneManager.LoadSceneAsync(
            menuSceneName
        );
    }

    private IEnumerator Fade(
        float startAlpha,
        float endAlpha,
        float duration)
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.alpha = startAlpha;

        if (duration <= 0f)
        {
            fadeCanvasGroup.alpha = endAlpha;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / duration
            );

            fadeCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                progress
            );

            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
    }
}