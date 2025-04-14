using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace AztechGames
{
    public class FaultInjection : MonoBehaviour
    {
        public TextMeshProUGUI faultStatusText;
        public TextMeshProUGUI faultLogText;

        private Dictionary<string, bool> activeFaults = new Dictionary<string, bool>()
        {
            { "GPS Failure", false },
            { "Sensor Drift", false },
            { "Aileron Stuck", false },
            { "Rudder Lock", false },
            { "Engine Shutdown", false }
        };

        private Dictionary<string, float> faultCooldowns = new Dictionary<string, float>();
        private const float FAULT_COOLDOWN_SECONDS = 15f;
        private const int FAULT_CONFIRMATION_THRESHOLD = 3;
        private const float COOLDOWN_REASSESS_TIME = FAULT_COOLDOWN_SECONDS / 2;

        private Dictionary<string, int> faultConfirmationCounts = new Dictionary<string, int>()
        {
            { "GPS Failure", 0 },
            { "Sensor Drift", 0 },
            { "Aileron Stuck", 0 },
            { "Rudder Lock", 0 },
            { "Engine Shutdown", 0 }
        };

        private GliderSurface_Controller surfaceController;
        private GliderEngine_Controller engineController;
        private AIFlightController aiController;
        private bool aiModeEnabled;

        void Start()
        {
            surfaceController = FindFirstObjectByType<GliderSurface_Controller>();
            engineController = FindFirstObjectByType<GliderEngine_Controller>();
            aiController = FindFirstObjectByType<AIFlightController>();

            if (surfaceController == null || engineController == null || aiController == null)
            {
                Debug.LogError("❌ FaultInjection: Missing required components!");
            }

            aiModeEnabled = PlayerPrefs.GetInt("FaultInjectionMode", 0) == 1;
            Debug.Log($"🔄 Fault Injection Mode Loaded: {(aiModeEnabled ? "AI Mode" : "Manual Mode")}");

            if (aiController != null)
            {
                if (aiModeEnabled)
                    aiController.ActivateAI();
                else
                    aiController.DeactivateAI();
            }
        }

        void Update()
        {
            if (!aiModeEnabled)
            {
                if (Input.GetKeyDown(KeyCode.Q)) InjectFault("GPS Failure");
                if (Input.GetKeyDown(KeyCode.W)) InjectFault("Sensor Drift");
                if (Input.GetKeyDown(KeyCode.E)) InjectFault("Aileron Stuck");
                if (Input.GetKeyDown(KeyCode.R)) InjectFault("Rudder Lock");
                if (Input.GetKeyDown(KeyCode.T)) InjectFault("Engine Shutdown");
            }
        }

        public void InjectFault(string faultType)
        {
            if (faultType == "No Fault")
            {
                Debug.Log("✅ AI: No fault detected. No action required.");
                return;
            }

            if (!activeFaults.ContainsKey(faultType))
            {
                Debug.LogWarning($"⚠️ Attempted to inject unknown fault: '{faultType}'. Ignoring.");
                return;
            }

            if (IsFaultActive(faultType))
            {
                Debug.Log($"⚠️ Fault '{faultType}' is already active. Skipping injection.");
                return;
            }

            if (faultCooldowns.ContainsKey(faultType))
            {
                float timeSinceLastActivation = Time.time - faultCooldowns[faultType];
                if (timeSinceLastActivation < FAULT_COOLDOWN_SECONDS)
                {
                    if (timeSinceLastActivation >= COOLDOWN_REASSESS_TIME)
                    {
                        Debug.Log($"⏳ Fault '{faultType}' is in cooldown, but reassessment allowed.");
                    }
                    else
                    {
                        Debug.Log($"⏳ Fault '{faultType}' still in cooldown ({timeSinceLastActivation:F1}s ago). Skipping.");
                        return;
                    }
                }
            }

            faultConfirmationCounts[faultType]++;
            if (faultConfirmationCounts[faultType] < FAULT_CONFIRMATION_THRESHOLD)
            {
                Debug.Log($"🔍 Fault '{faultType}' detected {faultConfirmationCounts[faultType]}/{FAULT_CONFIRMATION_THRESHOLD} times. Waiting for confirmation...");
                return;
            }

            faultConfirmationCounts[faultType] = 0;
            activeFaults[faultType] = true;
            faultCooldowns[faultType] = Time.time;

            ApplyFaultEffects(faultType);
            UpdateUI();
        }

        private void ApplyFaultEffects(string faultType)
        {
            switch (faultType)
            {
                case "GPS Failure":
                    Debug.Log("⚠️ GPS Failure: AI must use INS for navigation.");
                    break;
                case "Sensor Drift":
                    Debug.Log("⚠️ Sensor Drift: AI must correct with Kalman Filter.");
                    break;
                case "Aileron Stuck":
                    if (surfaceController != null) surfaceController.AileronAmount = 0;
                    Debug.Log("⚠️ Aileron Stuck: Rudder compensation required.");
                    break;
                case "Rudder Lock":
                    Debug.Log("⚠️ Rudder Lock: AI must adjust ailerons for yaw.");
                    break;
                case "Engine Shutdown":
                    if (engineController != null) engineController.Thrust = 0;
                    Debug.Log("⚠️ Engine Shutdown: Glider entering emergency glide mode.");
                    break;
                default:
                    Debug.LogWarning("Unknown fault type: " + faultType);
                    return;
            }
        }

        public void ResolveFault(string faultType)
        {
            if (!activeFaults.ContainsKey(faultType))
            {
                Debug.LogWarning($"⚠️ Attempted to resolve unknown fault: '{faultType}'. Ignoring.");
                return;
            }

            if (activeFaults[faultType])
            {
                activeFaults[faultType] = false;
                Debug.Log($"✅ Fault '{faultType}' resolved.");

                switch (faultType)
                {
                    case "Aileron Stuck":
                        if (surfaceController != null) surfaceController.AileronAmount = 1;
                        break;
                    case "Rudder Lock":
                        Debug.Log("✅ Rudder Lock Resolved.");
                        break;
                    case "Engine Shutdown":
                        if (engineController != null) engineController.Thrust = 50;
                        break;
                }

                UpdateUI();
            }
        }

        public bool IsFaultActive(string faultType)
        {
            return activeFaults.ContainsKey(faultType) && activeFaults[faultType];
        }

        // ✅ FIX: Added Missing Functions for AIFlightController
        public bool IsGPSFailure() => IsFaultActive("GPS Failure");
        public bool IsSensorDrift() => IsFaultActive("Sensor Drift");
        public bool IsAileronStuck() => IsFaultActive("Aileron Stuck");
        public bool IsRudderLock() => IsFaultActive("Rudder Lock");
        public bool IsEngineShutdown() => IsFaultActive("Engine Shutdown");

        void UpdateUI()
        {
            if (faultStatusText != null)
            {
                faultStatusText.text = "Active Faults:\n";
                foreach (var fault in activeFaults)
                {
                    if (fault.Value)
                    {
                        faultStatusText.text += $"- {fault.Key}\n";
                    }
                }
            }

            if (faultLogText != null)
            {
                faultLogText.text += $"[{System.DateTime.Now:HH:mm:ss}] {string.Join(", ", activeFaults)}\n";
            }
        }
    }
}
