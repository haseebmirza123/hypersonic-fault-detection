using UnityEngine;

namespace AztechGames
{
    public class LandingDetection : MonoBehaviour
    {
        public float maxSafeLandingSpeed = 5f; // Max vertical speed for safe landing
        public float maxSafeLandingAngle = 10f; // Max angle (pitch) for a smooth landing
        private Rigidbody rb;
        private bool hasLanded = false; // Prevents multiple triggers
        private float activationTime = 3f; // Time before landing detection activates
        private float startTime;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("❌ LandingDetection: Rigidbody missing! Ensure this object has a Rigidbody component.");
                return;
            }
            startTime = Time.time; // Store when the flight started
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (hasLanded) return; // Prevent duplicate detection

            // 🚀 Don't detect landing if the plane just spawned
            if (Time.time - startTime < activationTime) return;

            if (collision.gameObject.CompareTag("Ground")) // Ensure the ground has this tag
            {
                float verticalSpeed = Mathf.Abs(rb.linearVelocity.y); // ✅ Corrected to use 'velocity.y'
                float pitchAngle = transform.eulerAngles.x;

                // Normalize pitch angle (handles 360-degree rotation)
                if (pitchAngle > 180) pitchAngle -= 360;

                Debug.Log($"🔍 Landing Detection: Speed = {verticalSpeed} m/s, Pitch Angle = {pitchAngle}°");

                if (verticalSpeed <= maxSafeLandingSpeed && Mathf.Abs(pitchAngle) <= maxSafeLandingAngle)
                {
                    Debug.Log("✅ Emergency Landing Successful!");
                    HandleLandingResult("Emergency Landing Successful");
                }
                else
                {
                    Debug.Log("❌ Crash! Flight Unsuccessful.");
                    HandleLandingResult("Flight Unsuccessful - Aircraft Crashed");
                }

                hasLanded = true; // Prevent multiple detections
            }
        }

        void HandleLandingResult(string resultMessage)
        {
            // ✅ Get EndScreenManager
            EndScreenManager endScreen = FindFirstObjectByType<EndScreenManager>();

            if (endScreen != null)
            {
                // ✅ Fetch fault count from AIFlightController
                AIFlightController aiFlightController = FindFirstObjectByType<AIFlightController>();
                int totalFaults = aiFlightController != null ? aiFlightController.GetFaultCount() : 0;

                // ✅ Updated to match the correct method signature
                endScreen.ShowEndScreen(resultMessage, totalFaults, Time.timeSinceLevelLoad);
            }
            else
            {
                Debug.LogError("❌ EndScreenManager not found! Ensure it's added to the scene.");
            }
        }
    }
}
