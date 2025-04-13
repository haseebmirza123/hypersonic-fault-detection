import pandas as pd
import random
import os

# Set random seed for reproducibility
random.seed(42)

# Define revised value ranges to prevent overfitting on Engine Shutdown
normal_range = {
    "Speed_Mach": (3.0, 8.0),
    "Altitude_meters": (30000, 45000),
    "GPS_Signal_Strength_dBm": (-110, -80),
    "Inertial_Accel_m_s2": (0.8, 3.0),
    "Doppler_Shift_Hz": (-20, 20)
}

minor_fault_range = {
    "Speed_Mach": (2.5, 9.0),
    "Altitude_meters": (28000, 47000),
    "GPS_Signal_Strength_dBm": (-120, -70),
    "Inertial_Accel_m_s2": (0.5, 4.0),
    "Doppler_Shift_Hz": (-30, 30)
}

severe_fault_range = {
    "Speed_Mach": (2.0, 9.5),  
    "Altitude_meters": (25000, 50000),  
    "GPS_Signal_Strength_dBm": (-135, -65),  
    "Inertial_Accel_m_s2": (0.2, 5.0),  
    "Doppler_Shift_Hz": (-50, 50)  
}

# Adjust dataset size: fewer severe faults, more minor faults
num_rows = 10000  # Total flight samples
num_normal = int(num_rows * 0.60)  # 60% normal cases
num_minor_faults = int(num_rows * 0.35)  # 35% minor faults
num_severe_faults = num_rows - num_normal - num_minor_faults  # 5% severe faults

# Function to generate synthetic flight data
def generate_data(count, value_ranges, fault_label):
    data = []
    for _ in range(count):
        data.append([
            random.uniform(*value_ranges["Speed_Mach"]),
            random.uniform(*value_ranges["Altitude_meters"]),
            random.uniform(*value_ranges["GPS_Signal_Strength_dBm"]),
            random.uniform(*value_ranges["Inertial_Accel_m_s2"]),
            random.uniform(*value_ranges["Doppler_Shift_Hz"]),
            fault_label  # Fault Event (0 = Normal, 1 = Minor, 2 = Severe)
        ])
    return data

# Generate new flight data
data_normal = generate_data(num_normal, normal_range, 0)
data_minor_fault = generate_data(num_minor_faults, minor_fault_range, 1)
data_severe_fault = generate_data(num_severe_faults, severe_fault_range, 2)

# Combine and shuffle data
new_flight_data = data_normal + data_minor_fault + data_severe_fault
random.shuffle(new_flight_data)

# Convert to DataFrame
df_new_flight = pd.DataFrame(new_flight_data, columns=[
    "Speed_Mach", "Altitude_meters", "GPS_Signal_Strength_dBm",
    "Inertial_Accel_m_s2", "Doppler_Shift_Hz", "Fault_Event"
])

# Define save path
save_path = r"C:\Users\hasee\OneDrive\Documents\UNIVERSITY\hypersonic_navigation_v2\data\new_flight_data_balanced.csv"

# Ensure the directory exists
os.makedirs(os.path.dirname(save_path), exist_ok=True)

# Save the dataset
df_new_flight.to_csv(save_path, index=False)

print(f"✅ New balanced flight dataset successfully saved at: {save_path}")
