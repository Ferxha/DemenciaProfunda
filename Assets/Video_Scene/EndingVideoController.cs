using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class EndingVideoController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Tooltip("Nombre exacto del archivo dentro de Assets/StreamingAssets.")]
    [SerializeField] private string videoFileName = "Cinematica.m4v";

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1.5f;

    [Header("Escena siguiente")]
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Preparación")]
    [SerializeField] private float preparationTimeout = 20f;

    private bool preparationFailed;
    private bool finishingVideo;

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

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
    }

    private void OnDisable()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;
    }

    private IEnumerator Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError(
                "EndingVideoController: no se encontró el VideoPlayer.",
                this
            );

            yield break;
        }

        string videoPath = Path.Combine(
            Application.streamingAssetsPath,
            videoFileName
        );

#if UNITY_WEBGL && !UNITY_EDITOR
        /*
         * En WebGL, StreamingAssets ya devuelve una URL
         * que el navegador puede solicitar.
         */
        videoPlayer.url = videoPath;
#else
        /*
         * En Windows y en el Editor se convierte la ruta
         * a una URL válida de tipo file:///.
         */
        try
        {
            videoPlayer.url = new Uri(videoPath).AbsoluteUri;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"No se pudo crear la URL del video: {exception.Message}",
                this
            );

            yield break;
        }
#endif

        videoPlayer.source = VideoSource.Url;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;

        preparationFailed = false;
        finishingVideo = false;

        Debug.Log(
            $"Ruta original del video: {videoPath}",
            this
        );

        Debug.Log(
            $"URL utilizada por VideoPlayer: {videoPlayer.url}",
            this
        );

#if !UNITY_WEBGL || UNITY_EDITOR
        /*
         * Comprueba que el archivo exista y que no esté vacío
         * cuando se prueba en el Editor o en Windows.
         */
        if (!File.Exists(videoPath))
        {
            Debug.LogError(
                $"No existe el archivo de video: {videoPath}",
                this
            );

            yield break;
        }

        FileInfo videoFile = new FileInfo(videoPath);

        if (videoFile.Length <= 0)
        {
            Debug.LogError(
                $"El archivo de video está vacío: {videoPath}",
                this
            );

            yield break;
        }

        Debug.Log(
            $"Tamaño del video: {videoFile.Length} bytes.",
            this
        );
#endif

        videoPlayer.Prepare();

        float elapsed = 0f;

        while (!videoPlayer.isPrepared &&
               !preparationFailed &&
               elapsed < preparationTimeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (preparationFailed)
        {
            Debug.LogError(
                "El VideoPlayer recibió un error y no pudo preparar el video.",
                this
            );

            yield break;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogError(
                $"El video no se preparó después de {preparationTimeout} segundos.",
                this
            );

            yield break;
        }

        videoPlayer.Play();

        yield return StartCoroutine(
            Fade(
                1f,
                0f,
                fadeInDuration
            )
        );
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log(
            "El video se preparó correctamente.",
            this
        );
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        if (finishingVideo)
        {
            return;
        }

        StartCoroutine(
            FinishVideoSequence()
        );
    }

    private void OnVideoError(
        VideoPlayer source,
        string message)
    {
        preparationFailed = true;

        Debug.LogError(
            $"Error al reproducir el video: {message}",
            this
        );
    }

    private IEnumerator FinishVideoSequence()
    {
        finishingVideo = true;

        yield return StartCoroutine(
            Fade(
                0f,
                1f,
                fadeOutDuration
            )
        );

        Cursor.lockState = CursorLockMode.None;
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
        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = false;

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