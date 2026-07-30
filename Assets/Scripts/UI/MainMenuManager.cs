using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject menuPanel;

    [Header("Panels")]
    public GameObject tutorialPanel;
    public GameObject volumePanel;
    public GameObject creditsPanel;

    [Header("Fade")]
    [Tooltip("Canvas Group de la imagen negra que cubre la pantalla.")]
    public CanvasGroup fadeCanvasGroup;

    [Tooltip("Duración del fade a negro.")]
    public float fadeDuration = 1.5f;

    private bool isChangingScene;

    void Start()
    {
        menuPanel.SetActive(true);

        tutorialPanel.SetActive(false);
        volumePanel.SetActive(false);
        creditsPanel.SetActive(false);

        // El fade comienza transparente.
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.interactable = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    // ---------- Play ----------
    public void PlayGame()
    {
        if (isChangingScene)
        {
            return;
        }

        StartCoroutine(PlayGameRoutine());
    }

    private IEnumerator PlayGameRoutine()
    {
        isChangingScene = true;

        if (fadeCanvasGroup != null)
        {
            // Bloquea la interfaz durante el cambio de escena.
            fadeCanvasGroup.blocksRaycasts = true;

            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                fadeCanvasGroup.alpha = Mathf.Clamp01(
                    elapsedTime / fadeDuration
                );

                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }

        SceneManager.LoadSceneAsync("EnvironmentDeep");
    }

    // ---------- Tutorial ----------
    public void OpenTutorial()
    {
        menuPanel.SetActive(false);
        tutorialPanel.SetActive(true);
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    // ---------- Volumen ----------
    public void OpenVolume()
    {
        menuPanel.SetActive(false);
        volumePanel.SetActive(true);
    }

    public void CloseVolume()
    {
        volumePanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    // ---------- Créditos ----------
    public void OpenCredits()
    {
        menuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }
}