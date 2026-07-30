using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text timerText;
    public GameObject gameOverPanel;
    public GameObject foundMessage;

    [Header("Timer")]
    public float timeRemaining = 60f;

    private bool timerRunning = true;

    void Start()
    {
        gameOverPanel.SetActive(false);
        foundMessage.SetActive(false);
    }

    void Update()
    {
        if (!timerRunning)
            return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimer();
        }
        else
        {
            timeRemaining = 0;
            timerRunning = false;
            UpdateTimer();

            gameOverPanel.SetActive(true);

            Time.timeScale = 0f;
        }
    }

    void UpdateTimer()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ShowFoundMessage()
    {
        foundMessage.SetActive(true);
    }
}