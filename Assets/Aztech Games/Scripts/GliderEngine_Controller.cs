using UnityEngine;

namespace AztechGames
{
    public class GliderEngine_Controller : MonoBehaviour
    {
        public static GliderEngine_Controller Instance { get; private set; }

        [Tooltip("Acceleration rate of the glider engine.")]
        public float acceleration = 10f;

        [Tooltip("Initial speed of the aircraft at start.")]
        public float initialSpeed = 50f;

        private float thrust = 0f;
        private bool flightStarted = false;
        private float flightStartTime = 0f;
        private bool aiModeEnabled; // ✅ Tracks AI mode
        private bool manualFaultModeEnabled; // ✅ Tracks if manual fault injection is enabled

        public bool allowUserInput = true; // ✅ Allows enabling/disabling user input globally

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public float Thrust
        {
            get => Mathf.Clamp(thrust, 0f, 200f);
            set => thrust = value;
        }

        private void Start()
        {
            flightStarted = true;
            flightStartTime = Time.time;
            thrust = initialSpeed;

            // ✅ Load AI mode selection from PlayerPrefs
            aiModeEnabled = PlayerPrefs.GetInt("FaultInjectionMode", 0) == 1;
            manualFaultModeEnabled = !aiModeEnabled;

            // ✅ Disable user input if AI mode is enabled
            allowUserInput = !aiModeEnabled;

            Debug.Log($"🔄 Engine Mode: {(aiModeEnabled ? "AI-Controlled Faults" : "Manual Fault Injection")}");
        }

        private void FixedUpdate()
        {
            if (flightStarted)
            {
                transform.position += transform.forward * thrust * Time.deltaTime;
            }

            if (GliderSurface_Controller.Instance != null)
            {
                // ✅ Only allow user input if manual mode is enabled
                if (allowUserInput)
                {
                    GliderSurface_Controller.Instance.GetInputs();
                }
                GliderSurface_Controller.Instance.PlaneRotations();
            }
        }

        public void RecordFlightTime()
        {
            if (!flightStarted) return;

            float finalTime = GetFlightDuration();
            Debug.Log("Final Flight Time: " + finalTime + " seconds");

            PlayerPrefs.SetFloat("LastFlightDuration", finalTime);
            PlayerPrefs.Save();

            flightStarted = false;
        }

        public float GetFlightDuration()
        {
            return Time.time - flightStartTime;
        }
    }
}

