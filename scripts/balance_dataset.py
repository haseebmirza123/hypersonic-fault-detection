import pandas as pd
from imblearn.over_sampling import SMOTE, ADASYN, BorderlineSMOTE
from imblearn.under_sampling import RandomUnderSampler
from collections import Counter
import os

# ✅ Load dataset
data_path = "../data/new_flight_data.csv"  # Now using the updated dataset
df = pd.read_csv(data_path)

# ✅ Separate features and labels
X = df.drop(columns=["Fault_Event"])
y = df["Fault_Event"]

# ✅ Check initial class distribution
print("Original class distribution:", Counter(y))

# ✅ Apply SMOTE + Borderline-SMOTE + ADASYN for better balance
smote = BorderlineSMOTE(sampling_strategy="auto", random_state=42)
X_resampled, y_resampled = smote.fit_resample(X, y)

adasyn = ADASYN(sampling_strategy="auto", random_state=42)
X_resampled, y_resampled = adasyn.fit_resample(X_resampled, y_resampled)

# ✅ Apply slight undersampling to avoid excessive overrepresentation
undersampler = RandomUnderSampler(sampling_strategy={2: int(0.8 * Counter(y_resampled)[2])}, random_state=42)
X_resampled, y_resampled = undersampler.fit_resample(X_resampled, y_resampled)

# ✅ Create a new DataFrame
df_balanced = pd.DataFrame(X_resampled, columns=X.columns)
df_balanced["Fault_Event"] = y_resampled

# ✅ Check new class distribution
print("Balanced class distribution:", Counter(y_resampled))

# ✅ Save balanced dataset
balanced_path = "../data/balanced_hypersonic_anomalies_v3.csv"
os.makedirs(os.path.dirname(balanced_path), exist_ok=True)
df_balanced.to_csv(balanced_path, index=False)

print(f"✅ Balanced dataset saved at: {balanced_path}")
