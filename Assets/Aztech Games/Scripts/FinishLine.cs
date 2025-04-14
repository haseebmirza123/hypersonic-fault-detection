using UnityEngine;

namespace AztechGames // Ensure it matches the namespace in GliderEngine_Controller.cs
{
    public class FinishLine : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) // Ensure the plane is tagged as "Player"
            {
                Debug.Log("Plane has reached the finish line! Detected: " + other.gameObject.name);

                GliderEngine_Controller engineController = other.GetComponent<GliderEngine_Controller>();

                if (engineController != null)
                {
                    float flightDuration = engineController.GetFlightDuration();
                    Debug.Log("Flight Duration Recorded: " + flightDuration.ToString("F2") + " seconds");

                    // Save the flight duration for the Start Menu
                    PlayerPrefs.SetFloat("LastFlightDuration", flightDuration);
                    PlayerPrefs.Save();
                }
                else
                {
                    Debug.LogError("GliderEngine_Controller not found on the Player object!");
                }
            }
        }
    }
}
