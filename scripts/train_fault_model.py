import numpy as np
import pandas as pd
import os
import joblib
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.ensemble import RandomForestClassifier
from sklearn.neural_network import MLPClassifier
from sklearn.metrics import accuracy_score, classification_report, confusion_matrix

# ✅ Step 1: Load the Dataset
DATA_PATH = "../data/split_datasets/"

print("📂 Loading datasets...")
train_df = pd.read_csv(os.path.join(DATA_PATH, "train_data.csv"))
val_df = pd.read_csv(os.path.join(DATA_PATH, "val_data.csv"))
test_df = pd.read_csv(os.path.join(DATA_PATH, "test_data.csv"))

# ✅ Step 2: Separate Features (X) and Labels (y)
FEATURES = ["Speed_Mach", "Altitude_meters", "GPS_Signal_Strength_dBm", "Inertial_Accel_m_s2", "Doppler_Shift_Hz"]
TARGET = "Fault_Event"

X_train, y_train = train_df[FEATURES], train_df[TARGET]
X_val, y_val = val_df[FEATURES], val_df[TARGET]
X_test, y_test = test_df[FEATURES], test_df[TARGET]

# ✅ Step 3: Standardize Features
scaler = StandardScaler()
X_train_scaled = scaler.fit_transform(X_train)
X_val_scaled = scaler.transform(X_val)
X_test_scaled = scaler.transform(X_test)

# ✅ Ensure models directory exists
MODELS_DIR = "../models/"
os.makedirs(MODELS_DIR, exist_ok=True)

# ✅ Save the trained scaler
SCALER_PATH = os.path.join(MODELS_DIR, "fault_scaler_v3.joblib")
joblib.dump(scaler, SCALER_PATH)
print(f"✅ Scaler saved at: {SCALER_PATH}")

# ✅ Step 4: Train the Random Forest Classifier
print("🌲 Training Random Forest Classifier...")
rf_model = RandomForestClassifier(n_estimators=100, random_state=42, class_weight="balanced")
rf_model.fit(X_train_scaled, y_train)

# ✅ Step 5: Train the Neural Network (MLPClassifier)
print("🧠 Training Neural Network Classifier...")
mlp_model = MLPClassifier(hidden_layer_sizes=(64, 32), activation="relu", solver="adam", max_iter=500, random_state=42)
mlp_model.fit(X_train_scaled, y_train)

# ✅ Step 6: Evaluate Both Models
def evaluate_model(model, X, y, model_name):
    y_pred = model.predict(X)
    accuracy = accuracy_score(y, y_pred)
    print(f"\n📊 {model_name} Accuracy: {accuracy:.4f}")
    print(classification_report(y, y_pred))

    # Confusion Matrix
    cm = confusion_matrix(y, y_pred)
    plt.figure(figsize=(6, 5))
    sns.heatmap(cm, annot=True, fmt="d", cmap="Blues", xticklabels=["No Fault", "Minor Fault", "Severe Fault"], yticklabels=["No Fault", "Minor Fault", "Severe Fault"])
    plt.xlabel("Predicted Label")
    plt.ylabel("True Label")
    plt.title(f"Confusion Matrix - {model_name}")
    plt.show()

print("\n🔍 Evaluating Random Forest on Validation Set...")
evaluate_model(rf_model, X_val_scaled, y_val, "Random Forest")

print("\n🔍 Evaluating Neural Network on Validation Set...")
evaluate_model(mlp_model, X_val_scaled, y_val, "Neural Network")

# ✅ Step 7: Compare & Save the Best Model
rf_accuracy = accuracy_score(y_val, rf_model.predict(X_val_scaled))
mlp_accuracy = accuracy_score(y_val, mlp_model.predict(X_val_scaled))

if rf_accuracy >= mlp_accuracy:
    best_model = rf_model
    model_name = "Random Forest"
    MODEL_PATH = os.path.join(MODELS_DIR, "fault_detection_model_rf_v3.joblib")
else:
    best_model = mlp_model
    model_name = "Neural Network"
    MODEL_PATH = os.path.join(MODELS_DIR, "fault_detection_model_mlp_v3.joblib")

# ✅ Save the best model with error handling
try:
    joblib.dump(best_model, MODEL_PATH)
    print(f"\n✅ Best model ({model_name}) saved at: {MODEL_PATH}")
except Exception as e:
    print(f"❌ ERROR: Failed to save the model! Reason: {e}")

# ✅ Step 8: Final Testing on the Test Set
print("\n🏁 Evaluating Best Model on Test Set...")
evaluate_model(best_model, X_test_scaled, y_test, model_name)

print("\n🚀 Training & Evaluation Complete! The best model is now ready for use.")
