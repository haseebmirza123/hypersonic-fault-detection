using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Add this for TextMeshPro support

public class StartMenu : MonoBehaviour
{
    public Slider durationSlider;
    public TextMeshProUGUI durationText; // Change from Text to TextMeshProUGUI
    private float maxFlightTime = 100f; // Set maximum flight duration to 100 seconds

    private void Start()
    {
        // Load last selected duration (or default to maxFlightTime)
        float savedDuration = PlayerPrefs.GetFloat("SelectedFlightDuration", maxFlightTime);

        if (durationSlider != null)
        {
            durationSlider.minValue = 1f; // Minimum flight duration is 1 second
            durationSlider.maxValue = maxFlightTime;
            durationSlider.value = savedDuration;

            UpdateSliderValue(); // Update UI text on start
        }
    }

    public void StartSimulation()
    {
        // Store the selected flight duration
        float selectedDuration = durationSlider.value;
        PlayerPrefs.SetFloat("SelectedFlightDuration", selectedDuration);
        PlayerPrefs.Save();

        // Load the Simulation Scene
        SceneManager.LoadScene("SimulationScene");
    }

    public void UpdateSliderValue()
    {
        // Update displayed duration when the user moves the slider
        if (durationText != null && durationSlider != null)
        {
            durationText.text = "Selected Flight Duration: " + durationSlider.value.ToString("F0") + "s";
        }
    }

    public void ExitSimulation()
    {
        Application.Quit();
    }
}
