import pandas as pd
from sklearn.model_selection import train_test_split
import os

# ✅ Load the cleaned dataset
data_path = "../data/balanced_hypersonic_anomalies_v3.csv"  # Update path if necessary
df = pd.read_csv(data_path)

# ✅ Separate features and labels
X = df.drop(columns=["Fault_Event"])
y = df["Fault_Event"]

# ✅ Split into Training (70%) and Temp (30%)
X_train, X_temp, y_train, y_temp = train_test_split(
    X, y, test_size=0.3, stratify=y, random_state=42
)

# ✅ Split Temp into Validation (15%) and Test (15%)
X_val, X_test, y_val, y_test = train_test_split(
    X_temp, y_temp, test_size=0.5, stratify=y_temp, random_state=42
)

# ✅ Convert to DataFrames
train_df = pd.DataFrame(X_train, columns=X.columns)
train_df["Fault_Event"] = y_train

val_df = pd.DataFrame(X_val, columns=X.columns)
val_df["Fault_Event"] = y_val

test_df = pd.DataFrame(X_test, columns=X.columns)
test_df["Fault_Event"] = y_test

# ✅ Save the datasets
output_dir = "../data/split_datasets"
os.makedirs(output_dir, exist_ok=True)

train_df.to_csv(f"{output_dir}/train_data.csv", index=False)
val_df.to_csv(f"{output_dir}/val_data.csv", index=False)
test_df.to_csv(f"{output_dir}/test_data.csv", index=False)

# ✅ Print confirmation
print(f"✅ Dataset Split Completed!")
print(f"   Training Set: {train_df.shape[0]} samples")
print(f"   Validation Set: {val_df.shape[0]} samples")
print(f"   Test Set: {test_df.shape[0]} samples")
print(f"✅ Files saved in {output_dir}/")

