using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using AztechGames; // ✅ Import AIFlightController namespace

public class EndScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject endScreenPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI faultSummaryText;
    public TextMeshProUGUI durationText;

    // ✅ New UI Elements for Fault Log Panel
    public GameObject faultLogPanel;
    public TextMeshProUGUI faultLogText;
    public GameObject viewLogButton; // Button to open the log panel

    private AIFlightController aiFlightController;

    private void Start()
    {
        endScreenPanel.SetActive(false);
        if (faultLogPanel != null) faultLogPanel.SetActive(false); // Hide fault log initially

        aiFlightController = FindFirstObjectByType<AIFlightController>();

        if (aiFlightController == null)
        {
            Debug.LogError("❌ EndScreenManager: AIFlightController not found!");
        }
    }

    /// <summary>
    /// Displays the end screen with results, total faults encountered, and flight time.
    /// </summary>
    public void ShowEndScreen(string result, int totalFaults, float flightTime)
    {
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);
        }

        if (resultText != null)
        {
            resultText.text = result;
        }

        if (faultSummaryText != null)
        {
            faultSummaryText.text = $"Faults Encountered: {totalFaults}";
        }

        if (durationText != null)
        {
            durationText.text = "Total Flight Time: " + flightTime.ToString("F1") + "s";
        }

        // ✅ Enable "View Log" button only if there are faults
        if (viewLogButton != null)
        {
            viewLogButton.SetActive(totalFaults > 0);
        }

        // ✅ Fetch fault log and update the panel
        UpdateFaultLogText();
    }

    /// <summary>
    /// Opens the fault log panel.
    /// </summary>
    public void OpenFaultLog()
    {
        if (faultLogPanel != null)
        {
            faultLogPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Closes the fault log panel.
    /// </summary>
    public void CloseFaultLog()
    {
        if (faultLogPanel != null)
        {
            faultLogPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Retrieves fault log from AIFlightController and updates the text display.
    /// </summary>
    private void UpdateFaultLogText()
    {
        if (faultLogText != null && aiFlightController != null)
        {
            List<string> logEntries = aiFlightController.GetFaultLog();
            if (logEntries.Count > 0)
            {
                faultLogText.text = "Fault Log:\n" + string.Join("\n", logEntries);
            }
            else
            {
                faultLogText.text = "No faults recorded.";
            }
        }
    }

    public void RestartSimulation()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }
}
