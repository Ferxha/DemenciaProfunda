using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject menuPanel;

    [Header("Panels")]
    public GameObject tutorialPanel;
    public GameObject volumePanel;
    public GameObject creditsPanel;

    void Start()
    {
        menuPanel.SetActive(true);

        tutorialPanel.SetActive(false);
        volumePanel.SetActive(false);
        creditsPanel.SetActive(false);
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