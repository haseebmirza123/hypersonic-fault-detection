using UnityEngine;

namespace AztechGames
{
    public class GliderSurface_Controller : Singleton<GliderSurface_Controller>
    {
        [Header("Control Surfaces")]
        public Transform aileronLeft;
        public Transform aileronRight;
        public Transform elevator;
        public Transform slatsLeft;
        public Transform slatsRight;
        public Transform rudder; // ✅ Added Rudder

        [Header("Surfaces Max Angle")]
        public float aileronMaxAngle = 30f;
        public float elevatorMaxAngle = 30f;
        public float rudderMaxAngle = 25f; // ✅ Added Rudder Angle Limit
        public float slatMaxAngle = 0.22f;
        public float surfaceSpeed = 2f;

        private float _slatAmount;
        private float _elevatorAmount;
        private float _aileronAmount;
        private float _rudderAmount; // ✅ Added Rudder Variable

        private bool aiModeEnabled; // ✅ Tracks if AI mode is active
        private bool manualFaultModeEnabled; // ✅ Tracks if manual fault injection is allowed

        public bool allowUserInput = true; // ✅ Allows enabling/disabling user input globally

        public float SlatAmount
        {
            get => _slatAmount;
            set => _slatAmount = Mathf.Clamp(value, 0f, slatMaxAngle);
        }
        public float ElevatorAmount
        {
            get => Mathf.Clamp(_elevatorAmount, -elevatorMaxAngle, elevatorMaxAngle);
            set => _elevatorAmount = value;
        }
        public float AileronAmount
        {
            get => Mathf.Clamp(_aileronAmount, -aileronMaxAngle, aileronMaxAngle);
            set => _aileronAmount = value;
        }
        public float RudderAmount // ✅ Added Rudder Control Property
        {
            get => Mathf.Clamp(_rudderAmount, -rudderMaxAngle, rudderMaxAngle);
            set => _rudderAmount = value;
        }

        void Start()
        {
            // ✅ Load AI mode selection from PlayerPrefs
            aiModeEnabled = PlayerPrefs.GetInt("FaultInjectionMode", 0) == 1;
            manualFaultModeEnabled = !aiModeEnabled;

            // ✅ Disable user input if AI mode is enabled
            allowUserInput = !aiModeEnabled;

            Debug.Log($"🔄 Flight Mode: {(aiModeEnabled ? "AI-Controlled Faults" : "Manual Fault Injection")}");
        }

        public void GetInputs()
        {
            if (!allowUserInput) return; // ✅ Prevent ALL user control if disabled

            // ✅ Only allow fault injection, NOT aircraft movement
            if (manualFaultModeEnabled)
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
            Debug.Log($"⚠️ Fault Injected: {faultType}");
        }

        public void PlaneRotations()
        {
            // ✅ Added Rudder to Control Yaw
            transform.Rotate(new Vector3(ElevatorAmount / surfaceSpeed, RudderAmount / surfaceSpeed, -AileronAmount) * Time.deltaTime);
        }
    }
}
