import numpy as np
import pandas as pd
import os
import joblib
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.metrics import accuracy_score, classification_report, confusion_matrix, f1_score
from sklearn.preprocessing import StandardScaler

# ✅ Load the Dataset for Evaluation
DATA_PATH = "../data/split_datasets/"

print("📂 Loading datasets...")
val_df = pd.read_csv(os.path.join(DATA_PATH, "val_data.csv"))
test_df = pd.read_csv(os.path.join(DATA_PATH, "test_data.csv"))

FEATURES = ["Speed_Mach", "Altitude_meters", "GPS_Signal_Strength_dBm", "Inertial_Accel_m_s2", "Doppler_Shift_Hz"]
TARGET = "Fault_Event"

X_val, y_val = val_df[FEATURES], val_df[TARGET]
X_test, y_test = test_df[FEATURES], test_df[TARGET]

# ✅ Load Pretrained Scaler
SCALER_PATH = "../models/fault_scaler_v3.joblib"
scaler = joblib.load(SCALER_PATH)

X_val_scaled = scaler.transform(X_val)
X_test_scaled = scaler.transform(X_test)

# ✅ Load Best Model (Random Forest)
RF_MODEL_PATH = "../models/fault_detection_model_rf_v3.joblib"

print("📥 Loading trained model...")
rf_model = joblib.load(RF_MODEL_PATH)

# ✅ Function to Evaluate the Model
def evaluate_model(model, X, y, model_name, save_path):
    y_pred = model.predict(X)
    accuracy = accuracy_score(y, y_pred)
    f1 = f1_score(y, y_pred, average="weighted")
    
    print(f"\n📊 {model_name} Performance:")
    print(f"   Accuracy: {accuracy:.4f}")
    print(f"   F1-score: {f1:.4f}")
    print(classification_report(y, y_pred))

    # Confusion Matrix
    cm = confusion_matrix(y, y_pred)
    plt.figure(figsize=(6, 5))
    sns.heatmap(cm, annot=True, fmt="d", cmap="Blues", xticklabels=["No Fault", "Minor Fault", "Severe Fault"], yticklabels=["No Fault", "Minor Fault", "Severe Fault"])
    plt.xlabel("Predicted Label")
    plt.ylabel("True Label")
    plt.title(f"Confusion Matrix - {model_name}")

    # Save confusion matrix as image
    plt.savefig(save_path)
    plt.show()
    print(f"✅ Confusion matrix saved at: {save_path}")

# ✅ Evaluate Random Forest on Validation Set
print("\n🔍 Evaluating Random Forest on Validation Set...")
evaluate_model(rf_model, X_val_scaled, y_val, "Random Forest", "../results/confusion_matrix_rf.png")

# ✅ Evaluate Best Model on Test Set
print("\n🏁 Evaluating Best Model on Test Set...")
evaluate_model(rf_model, X_test_scaled, y_test, "Random Forest", "../results/confusion_matrix_best_model.png")

print("\n🚀 Evaluation Complete! The best model remains saved at:", RF_MODEL_PATH)
