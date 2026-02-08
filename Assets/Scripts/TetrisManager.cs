using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TetrisManager : MonoBehaviour
{
    // Needs to be public because other places in code need to access it. But CANNOT be accessed from Unity editor
    public int score { get; private set; }
    public bool gameOver { get; private set; }

    public UnityEvent OnScoreChanged;
    public UnityEvent OnGameOver;

    public float gameLength = 10f;
    float timeRemaining;
    bool timerIsRunning = false;
    public TextMeshProUGUI timeText;

    private void Start()
    {
        SetGameOver(false);
        timerIsRunning = true;
        timeRemaining = gameLength;
    }

    public int CalculateScore(int linesCleared)
    {
        switch (linesCleared)
        {
            case 1: return 100;
            case 2: return 300;
            case 3: return 500;
            case 4: return 800;
            default: return 0;
        }
    }

    public void ChangeScore(int amount)
    {
        score += amount;
        OnScoreChanged.Invoke();
    }

    public void SetGameOver(bool gameOver)
    {
        if(!gameOver)
        {
            score = 0;
            ChangeScore(0);
            timeRemaining = 0f;
        }    

        OnGameOver.Invoke();
    }


    private void Update()
    { 
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                OnGameOver.Invoke();
                timeRemaining = 10f;
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float seconds = Mathf.FloorToInt(timeToDisplay);

        timeText.text = $"Time Remaining: {seconds}";
    }
}
