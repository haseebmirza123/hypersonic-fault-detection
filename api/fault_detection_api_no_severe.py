from flask import Flask, request, jsonify
import joblib
import numpy as np
import pandas as pd
import requests  # ✅ For forwarding debug data
import time  # ✅ For cooldown tracking

app = Flask(__name__)

# Load the trained model and scaler
MODEL_PATH = "../models/fault_detection_model_no_severe.joblib"
SCALER_PATH = "../models/fault_scaler_no_severe.joblib"

print("🔄 Loading trained model and scaler...")
model = joblib.load(MODEL_PATH)
scaler = joblib.load(SCALER_PATH)
print("✅ Model and scaler loaded successfully.")

# Define expected input features and labels
FEATURES = ["Speed_Mach", "Altitude_meters", "GPS_Signal_Strength_dBm", "Inertial_Accel_m_s2", "Doppler_Shift_Hz"]
LABELS = ["No Fault", "Sensor Drift"]  # Updated fault labels

# Debug API (for monitoring incoming data)
DEBUG_API_URL = "http://127.0.0.1:5001/debug"

# Cooldown & Fault Management
FAULT_COOLDOWN_SECONDS = 10  # Prevents spamming the same fault
CONFIRMATION_THRESHOLD = 3  # Must detect the fault 3 times before triggering
fault_history = {}  # Stores how many times a fault was detected
fault_cooldown_tracker = {}  # Stores last activation time for faults
current_active_fault = None  # Ensures only one fault at a time

@app.route("/predict", methods=["POST"])
def predict_fault():
    global current_active_fault
    try:
        data = request.get_json()

        # ✅ Forward request to Debug API
        try:
            requests.post(DEBUG_API_URL, json=data, timeout=0.5)
        except requests.exceptions.RequestException:
            print("⚠️ Debug API is not running or not responding.")

        # ✅ Check if input contains expected features
        if not all(feature in data for feature in FEATURES):
            return jsonify({"error": "Missing required features in input data"}), 400

        # Convert JSON to DataFrame and scale features
        input_df = pd.DataFrame([data], columns=FEATURES)
        scaled_input = scaler.transform(input_df)
        prediction_probs = model.predict_proba(scaled_input)[0]

        # Determine fault
        max_prob = np.max(prediction_probs)
        predicted_fault = np.argmax(prediction_probs).item() if max_prob > 0.5 else 0  # Default to "No Fault"
        fault_label = LABELS[predicted_fault]

        current_time = time.time()

        # ✅ Ensure only one fault is active at a time
        if current_active_fault and current_active_fault != fault_label:
            print(f"⚠️ '{fault_label}' detected but '{current_active_fault}' is still active. Ignoring.")
            return jsonify({
                "predicted_fault": 0,
                "fault_label": "No Fault",
                "prediction_probabilities": prediction_probs.tolist()
            })

        # ✅ Cooldown system prevents repeated activations
        if fault_label in fault_cooldown_tracker:
            time_since_last_activation = current_time - fault_cooldown_tracker[fault_label]
            if time_since_last_activation < FAULT_COOLDOWN_SECONDS:
                print(f"⏳ Cooldown Active: '{fault_label}' skipped (Last Activated {time_since_last_activation:.1f}s ago)")
                return jsonify({
                    "predicted_fault": 0,
                    "fault_label": "No Fault",
                    "prediction_probabilities": prediction_probs.tolist()
                })

        # ✅ Confirm fault before activation
        if fault_label not in fault_history:
            fault_history[fault_label] = 1
        else:
            fault_history[fault_label] += 1

        if fault_history[fault_label] < CONFIRMATION_THRESHOLD:
            print(f"🔍 Fault '{fault_label}' detected {fault_history[fault_label]}/{CONFIRMATION_THRESHOLD} times. Waiting for confirmation...")
            return jsonify({
                "predicted_fault": 0,
                "fault_label": "No Fault",
                "prediction_probabilities": prediction_probs.tolist()
            })

        # ✅ Fault confirmed: Activate and start cooldown
        fault_history[fault_label] = 0  # Reset confirmation count
        fault_cooldown_tracker[fault_label] = current_time  # Start cooldown
        current_active_fault = fault_label  # Lock active fault

        response = {
            "predicted_fault": int(predicted_fault),
            "fault_label": fault_label,
            "prediction_probabilities": prediction_probs.tolist()
        }

        print(f"🔹 API Response Sent: {response}")
        return jsonify(response)

    except Exception as e:
        print(f"❌ Error in API: {str(e)}")
        return jsonify({"error": str(e)}), 500

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=True)  # Keep the same port for Unity
