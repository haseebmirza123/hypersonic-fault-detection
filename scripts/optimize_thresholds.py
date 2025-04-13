import numpy as np
import pandas as pd
import os
import joblib
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.metrics import roc_curve, classification_report, confusion_matrix, accuracy_score, f1_score

# ✅ Load Dataset
DATA_PATH = "../data/split_datasets/"
print("📂 Loading validation dataset...")
val_df = pd.read_csv(os.path.join(DATA_PATH, "val_data.csv"))

FEATURES = ["Speed_Mach", "Altitude_meters", "GPS_Signal_Strength_dBm", "Inertial_Accel_m_s2", "Doppler_Shift_Hz"]
TARGET = "Fault_Event"

X_val, y_val = val_df[FEATURES], val_df[TARGET]

# ✅ Load Pretrained Scaler & Model
SCALER_PATH = "../models/fault_scaler_v3.joblib"
MODEL_PATH = "../models/fault_detection_model_rf_v3.joblib"

print("📥 Loading trained model & scaler...")
scaler = joblib.load(SCALER_PATH)
model = joblib.load(MODEL_PATH)

# Scale validation data
X_val_scaled = scaler.transform(X_val)

# ✅ Get Class Probabilities
y_probs = model.predict_proba(X_val_scaled)

# ✅ Compute Optimal Decision Thresholds
optimal_thresholds = {}

for class_idx in range(3):  # We have 3 classes (0, 1, 2)
    fpr, tpr, thresholds = roc_curve(y_val == class_idx, y_probs[:, class_idx])
    optimal_idx = np.argmax(tpr - fpr)  # Maximize TPR while minimizing FPR
    optimal_thresholds[class_idx] = thresholds[optimal_idx]

print("\n🔍 Optimal Decision Thresholds Found:")
for label, threshold in optimal_thresholds.items():
    print(f"   Class {label}: {threshold:.3f}")

# ✅ Apply New Thresholds
adjusted_preds = []
for i in range(len(y_probs)):
    probs = y_probs[i]
    class_prediction = np.argmax(probs >= [optimal_thresholds[0], optimal_thresholds[1], optimal_thresholds[2]])
    adjusted_preds.append(class_prediction)

# ✅ Evaluate New Model Performance
accuracy = accuracy_score(y_val, adjusted_preds)
f1 = f1_score(y_val, adjusted_preds, average="weighted")

print("\n📊 Model Performance After Threshold Adjustment:")
print(f"   Accuracy: {accuracy:.4f}")
print(f"   F1-score: {f1:.4f}")
print(classification_report(y_val, adjusted_preds))

# ✅ Confusion Matrix
cm = confusion_matrix(y_val, adjusted_preds)
plt.figure(figsize=(6, 5))
sns.heatmap(cm, annot=True, fmt="d", cmap="Blues", xticklabels=["No Fault", "Minor Fault", "Severe Fault"], yticklabels=["No Fault", "Minor Fault", "Severe Fault"])
plt.xlabel("Predicted Label")
plt.ylabel("True Label")
plt.title("Confusion Matrix After Threshold Adjustment")

# Save confusion matrix
threshold_cm_path = "../results/confusion_matrix_adjusted_thresholds.png"
plt.savefig(threshold_cm_path)
plt.show()
print(f"✅ Confusion matrix saved at: {threshold_cm_path}")

# ✅ Save Optimized Thresholds
thresholds_save_path = "../models/optimized_thresholds.npy"
np.save(thresholds_save_path, optimal_thresholds)
print(f"✅ Optimized thresholds saved at: {thresholds_save_path}")
