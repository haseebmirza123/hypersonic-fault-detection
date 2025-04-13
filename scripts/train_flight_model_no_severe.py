import pandas as pd
import numpy as np
import joblib
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import accuracy_score, classification_report, confusion_matrix

# ✅ Load the new dataset
data_path = r"C:\Users\hasee\OneDrive\Documents\UNIVERSITY\hypersonic_navigation_v2\data\new_flight_data_no_severe.csv"
df = pd.read_csv(data_path)

# ✅ Define features and labels
FEATURES = ["Speed_Mach", "Altitude_meters", "GPS_Signal_Strength_dBm", "Inertial_Accel_m_s2", "Doppler_Shift_Hz"]
X = df[FEATURES]
y = df["Fault_Event"]  # Labels (0 = Normal, 1 = Minor Fault)

# ✅ Split dataset (80% training, 20% validation)
X_train, X_val, y_train, y_val = train_test_split(X, y, test_size=0.2, random_state=42, stratify=y)

# ✅ Scale the features
scaler = StandardScaler()
X_train_scaled = scaler.fit_transform(X_train)
X_val_scaled = scaler.transform(X_val)

# ✅ Train Random Forest Model
model = RandomForestClassifier(n_estimators=100, random_state=42)
model.fit(X_train_scaled, y_train)

# ✅ Evaluate Model Performance
y_pred = model.predict(X_val_scaled)
accuracy = accuracy_score(y_val, y_pred)

print(f"\n📊 Validation Accuracy: {accuracy:.4f}")
print("\n📌 Classification Report:")
print(classification_report(y_val, y_pred))

print("\n📌 Confusion Matrix:")
print(confusion_matrix(y_val, y_pred))

# ✅ Save model & scaler
model_save_path = r"C:\Users\hasee\OneDrive\Documents\UNIVERSITY\hypersonic_navigation_v2\models\fault_detection_model_no_severe.joblib"
scaler_save_path = r"C:\Users\hasee\OneDrive\Documents\UNIVERSITY\hypersonic_navigation_v2\models\fault_scaler_no_severe.joblib"

joblib.dump(model, model_save_path)
joblib.dump(scaler, scaler_save_path)

print(f"\n✅ Model saved at: {model_save_path}")
print(f"✅ Scaler saved at: {scaler_save_path}")
