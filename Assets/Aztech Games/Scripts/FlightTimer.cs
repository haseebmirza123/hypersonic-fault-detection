using UnityEngine;
using TMPro;
using AztechGames; // ✅ Ensure correct namespace is included

public class FlightTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // UI Timer Display
    public EndScreenManager endScreenManager; // Reference to End Screen Manager

    private float remainingTime;
    private bool timerRunning = true; // Timer starts running by default

    private void Start()
    {
        // Retrieve user-selected duration from PlayerPrefs (default to 100 if not found)
        remainingTime = PlayerPrefs.GetFloat("SelectedFlightDuration", 100f);
        UpdateTimerUI();
    }

    private void Update()
    {
        if (timerRunning && remainingTime > 0)
        {
            remainingTime -= Time.deltaTime; // Decrease time
            UpdateTimerUI();

            if (remainingTime <= 0)
            {
                remainingTime = 0;
                timerRunning = false; // Stop the timer when it reaches 0
                OnSimulationEnd(); // Call method to handle end of simulation
            }
        }
    }

    void UpdateTimerUI()
    {
        timerText.text = "Time Left: " + Mathf.Ceil(remainingTime).ToString() + "s";
    }

    void OnSimulationEnd()
    {
        Debug.Log("Simulation Ended: Time is up!");

        if (endScreenManager != null)
        {
            // ✅ Fetch fault count from AIFlightController
            AIFlightController aiFlightController = FindFirstObjectByType<AIFlightController>();
            int totalFaults = aiFlightController != null ? aiFlightController.GetFaultCount() : 0;

            // ✅ Updated to match correct method signature
            endScreenManager.ShowEndScreen("Flight Completed Successfully", totalFaults, PlayerPrefs.GetFloat("SelectedFlightDuration"));
        }
        else
        {
            Debug.LogError("EndScreenManager not assigned in FlightTimer!");
        }
    }
}
