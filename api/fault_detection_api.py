from flask import Flask, request, jsonify
import joblib
import numpy as np
import pandas as pd

app = Flask(__name__)

# ✅ Load the trained model & scaler
MODEL_PATH = "../models/fault_detection_model_rf_v4.joblib"
SCALER_PATH = "../models/fault_scaler_v4.joblib"

print("🔄 Loading trained model and scaler...")
model = joblib.load(MODEL_PATH)
scaler = joblib.load(SCALER_PATH)
print("✅ Model and scaler loaded successfully.")

# ✅ Define features & fault mapping
FEATURES = ["Speed_Mach", "Altitude_meters", "GPS_Signal_Strength_dBm", "Inertial_Accel_m_s2", "Doppler_Shift_Hz"]
FAULT_MAPPING = {0: "No Fault", 1: "Sensor Drift", 2: "Engine Shutdown"}

# ✅ Adjusted thresholds
OPTIMIZED_THRESHOLDS = np.array([0.5, 0.4, 0.95], dtype=np.float64)  # ⬆ Increased threshold for engine shutdown

# ✅ Cooldown system (Prevents spam)
fault_cooldown_tracker = {}
FAULT_COOLDOWN_SECONDS = 15  # ⬆ Increased cooldown to prevent too many repeated activations

# ✅ Fault Confirmation (Stops Instantaneous Faults)
FAULT_CONFIRMATION_THRESHOLD = 3
fault_history = {}

@app.route("/predict", methods=["POST"])
def predict_fault():
    try:
        data = request.get_json()

        # ✅ Ensure incoming JSON has the required features
        if not all(feature in data for feature in FEATURES):
            return jsonify({"error": "Missing required features in input data"}), 400

        # ✅ Convert input data into a DataFrame & apply feature scaling
        input_df = pd.DataFrame([data], columns=FEATURES)
        scaled_input = scaler.transform(input_df)

        # ✅ Get model predictions
        prediction_probs = model.predict_proba(scaled_input)[0]

        # 🔹 **Engine Shutdown Weight Reduction**
        prediction_probs[2] *= 0.3  # ⬇ Downweights engine shutdown predictions
        prediction_probs /= prediction_probs.sum()  # Re-normalize probabilities

        # ✅ Debugging: Print probability distributions
        print(f"📡 Incoming Flight Data: {data}")
        print(f"📊 Adjusted Probabilities (Downweighted Engine Shutdown): {prediction_probs}")
        print(f"📊 Optimized Thresholds: {OPTIMIZED_THRESHOLDS}")

        # ✅ Determine highest probability fault
        max_prob = np.max(prediction_probs)
        predicted_fault = np.argmax(prediction_probs).item() if max_prob > OPTIMIZED_THRESHOLDS[np.argmax(prediction_probs)] else 0
        fault_label = FAULT_MAPPING.get(predicted_fault, "Unknown Fault")

        # ✅ Cooldown Check (Prevents frequent reactivation of the same fault)
        current_time = pd.Timestamp.now().timestamp()
        if fault_label in fault_cooldown_tracker:
            time_since_last_activation = current_time - fault_cooldown_tracker[fault_label]

            if time_since_last_activation < FAULT_COOLDOWN_SECONDS:
                print(f"⏳ Cooldown Active: '{fault_label}' skipped ({time_since_last_activation:.1f}s ago)")
                return jsonify({
                    "predicted_fault": 0,
                    "fault_label": "No Fault",
                    "prediction_probabilities": prediction_probs.tolist()
                })

        # ✅ Consecutive Detection System: Only trigger fault if detected 3+ times in a row
        if fault_label not in fault_history:
            fault_history[fault_label] = 1
        else:
            fault_history[fault_label] += 1

        if fault_history[fault_label] < FAULT_CONFIRMATION_THRESHOLD:
            print(f"🔍 Fault '{fault_label}' detected {fault_history[fault_label]}/{FAULT_CONFIRMATION_THRESHOLD} times. Waiting for confirmation...")
            return jsonify({
                "predicted_fault": 0,
                "fault_label": "No Fault",
                "prediction_probabilities": prediction_probs.tolist()
            })

        # ✅ If confirmed, reset counter and trigger fault
        fault_history[fault_label] = 0
        fault_cooldown_tracker[fault_label] = current_time

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
    app.run(host="0.0.0.0", port=5000, debug=True)
