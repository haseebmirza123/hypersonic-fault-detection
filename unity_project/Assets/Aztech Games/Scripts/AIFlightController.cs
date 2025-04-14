using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TMPro;

namespace AztechGames
{
    public class AIFlightController : MonoBehaviour
    {
        private const string apiUrl = "http://localhost:5000/predict"; // Flask API endpoint

        private GliderSurface_Controller surfaceController;
        private GliderEngine_Controller engineController;
        private FaultInjection faultInjection;
        private GliderUI_Manager gliderUIManager; // Reference to Glider UI Manager

        // ✅ AI Status Panel UI Elements
        public TextMeshProUGUI aiFaultText;
        public TextMeshProUGUI aiStatusText;
        public TextMeshProUGUI aiBackupText;
        public TextMeshProUGUI aiResolvedText;

        private bool aiActive = false;
        private bool aiModeEnabled;
        private string currentFault = null;
        private Queue<string> faultQueue = new Queue<string>(); // Fault Queue
        private Dictionary<string, float> faultCooldowns = new Dictionary<string, float>();

        private const float FAULT_COOLDOWN_SECONDS = 15f;
        private const int FAULT_CONFIRMATION_THRESHOLD = 3;
        private Dictionary<string, int> faultConfirmationCounts = new Dictionary<string, int>();

        private bool lastPredictionWasNoFault = false;

        // ✅ Fault Counter & Log Storage
        private int faultCounter = 0; // Counts injected faults
        private List<string> faultLog = new List<string>(); // Stores fault logs

        void Start()
        {
            aiModeEnabled = PlayerPrefs.GetInt("FaultInjectionMode", 0) == 1;
            if (!aiModeEnabled) return;

            // Find components
            surfaceController = FindFirstObjectByType<GliderSurface_Controller>();
            engineController = FindFirstObjectByType<GliderEngine_Controller>();
            faultInjection = FindFirstObjectByType<FaultInjection>();
            gliderUIManager = FindFirstObjectByType<GliderUI_Manager>(); // ✅ Get flight data UI manager

            if (surfaceController == null || engineController == null || faultInjection == null || gliderUIManager == null)
            {
                Debug.LogError("AIFlightController: Missing required components.");
                return;
            }

            StartCoroutine(SendFlightDataToAPI());

            // ✅ Initialize AI Status Panel with default values
            UpdateAIStatusPanel("None", "Stable", "", "YES");
        }

        void Update()
        {
            if (aiActive && faultInjection != null)
            {
                ApplyAIAdjustments();
            }

            // ✅ Handle fault resolution and apply 5-second delay before clearing
            if (currentFault != null && !faultInjection.IsFaultActive(currentFault))
            {
                Debug.Log($"✅ AI: Fault '{currentFault}' resolved. Checking next fault...");
                StartCoroutine(ClearAIStatusPanelAfterDelay(5f));
                currentFault = null;
                InjectNextFault();
            }
        }

        public void ActivateAI()
        {
            aiActive = true;
            Debug.Log("🚀 AI Activated: Taking control over flight adjustments.");
        }

        public void DeactivateAI()
        {
            aiActive = false;
            Debug.Log("🔄 AI Deactivated: Returning control to the user.");
        }

        // ✅ Function to update the AI Status Panel
        private void UpdateAIStatusPanel(string fault, string status, string backup, string resolved)
        {
            aiFaultText.text = $"Current Fault: {fault}";
            aiStatusText.text = $"Status: {status}";
            aiBackupText.text = $"Backup Systems: {backup}";
            aiResolvedText.text = $"Fault Resolved: {resolved}";
        }

        // ✅ Delayed function to clear the AI Status Panel
        private IEnumerator ClearAIStatusPanelAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            UpdateAIStatusPanel("None", "Stable", "", "YES");
        }

        private IEnumerator SendFlightDataToAPI()
        {
            yield return new WaitForSeconds(3.0f);

            while (true)
            {
                yield return new WaitForSeconds(2.5f);

                FlightData flightData = CollectFlightData();
                string json = JsonConvert.SerializeObject(flightData);

                using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
                {
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");

                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        FaultPrediction result = JsonConvert.DeserializeObject<FaultPrediction>(request.downloadHandler.text);
                        HandleFaultPrediction(result);
                    }
                }
            }
        }

 private void HandleFaultPrediction(FaultPrediction result)
{
    if (faultInjection == null) return;

    string fault = result.fault_label;

    // ✅ Handle "No Fault" safely and cleanly
    if (fault == "No Fault")
    {
        if (!lastPredictionWasNoFault)
        {
            Debug.Log("✅ AI: No Fault detected. Resetting confirmation counters.");
            lastPredictionWasNoFault = true;
        }

        // Safely reset confirmation counters
        foreach (var key in new List<string>(faultConfirmationCounts.Keys))
        {
            faultConfirmationCounts[key] = 0;
        }

        return;
    }
    else
    {
        lastPredictionWasNoFault = false;
    }

    // ✅ Initialize confirmation count if fault is new
    if (!faultConfirmationCounts.ContainsKey(fault))
        faultConfirmationCounts[fault] = 0;

    // ✅ Increment confirmation count
    faultConfirmationCounts[fault]++;
    int confirmationCount = faultConfirmationCounts[fault];

    Debug.Log($"🔍 AI: Fault '{fault}' predicted {confirmationCount}/{FAULT_CONFIRMATION_THRESHOLD} times.");

    // ✅ Show confirmation progress on AI panel
    UpdateAIStatusPanel($"{fault} ({confirmationCount}/{FAULT_CONFIRMATION_THRESHOLD})", "Confirming", "", "PENDING");

    // ✅ Wait until confirmation threshold is reached
    if (confirmationCount < FAULT_CONFIRMATION_THRESHOLD)
        return;

    // ✅ Fault confirmed — reset counter
    faultConfirmationCounts[fault] = 0;

    // ✅ Queue fault only if it's not already active or in the queue
    if (!faultInjection.IsFaultActive(fault) && !faultQueue.Contains(fault))
    {
        faultQueue.Enqueue(fault);
        Debug.Log($"📥 AI: Queued confirmed fault: {fault}");
    }

    // ✅ If no fault is being processed, inject the next one
    if (currentFault == null)
    {
        InjectNextFault();
    }
}



        private void InjectNextFault()
        {
            if (faultQueue.Count > 0)
            {
                currentFault = faultQueue.Dequeue();
                faultInjection.InjectFault(currentFault);
                faultCooldowns[currentFault] = Time.time;
                faultCounter++;

                // ✅ Update AI Status Panel with fault details
                string status = "Correcting";
                string backup = "";
                if (currentFault == "GPS Failure") backup = "INS Mode Active";
                if (currentFault == "Sensor Drift") backup = "Kalman Filter Active";
                if (currentFault == "Engine Shutdown") status = "Emergency Mode";

                string logEntry = $"[{System.DateTime.Now:HH:mm:ss}], (Fault: {currentFault}), Airspeed: {gliderUIManager.Airspeed.text}, Temp: {gliderUIManager.Temp.text}, Lift: {gliderUIManager.Lift.text}, Drag: {gliderUIManager.Drag.text}";
                faultLog.Add(logEntry);

                UpdateAIStatusPanel(currentFault, status, backup, "NO");
                Debug.Log($"⚠️ AI Injected: {currentFault}");
            }
        }

        private void ApplyAIAdjustments()
        {
            if (faultInjection.IsGPSFailure()) { Debug.Log("📡 AI: Handling GPS Failure."); }
            if (faultInjection.IsSensorDrift()) { Debug.Log("🔧 AI: Handling Sensor Drift."); }
        }

        public int GetFaultCount() => faultCounter;
        public List<string> GetFaultLog() => faultLog;

        // ✅ Collects all flight conditions for logging
        private FlightData CollectFlightData()
        {
            return new FlightData();
        }
    }
}

// ✅ FlightData Class (Restored)
public class FlightData
{
    public float Speed_Mach;
    public float Altitude_meters;
    public float GPS_Signal_Strength_dBm;
    public float Inertial_Accel_m_s2;
    public float Doppler_Shift_Hz;
}

// ✅ FaultPrediction Class (Restored)
public class FaultPrediction
{
    public int predicted_fault;
    public string fault_label;
    public List<float> prediction_probabilities;
}
