# hypersonic-fault-detection
AI-powered fault detection and injection system for hypersonic flight simulation using Unity and Flask. Final year project. (CSI_6_CSP_2425) StudentID: 4119392

--

## 📚 Contents
- [Overview](#overview)
- [Folder Breakdown](#folder-breakdown)
- [Setup Instructions](#setup-instructions)
- [How to Use](#how-to-use)
- [Unity Project Notes](#unity-project-notes)
- [Original Repository](#original-repository)

--
## Overview
This proof-of-concept project demonstrates real-time AI-driven fault detection within a simulated hypersonic flight environment. The system combines:
- Python-based AI models for detecting flight anomalies,
- A Flask API for real-time fault predictions,
- A Unity 3D simulation environment for visualizing and responding to injected faults.

--
## 📁 Folder Breakdown

| Folder/File         | Description                                                                 |
|---------------------|-----------------------------------------------------------------------------|
| `data/`             | Generated datasets (raw, balanced, and split sets)                          |
| `scripts/`          | Python scripts for data generation, balancing, and training                 |
| `models/`           | Trained AI models and scalers (`.joblib`, `.npy`)                           |
| `api/`              | Flask API scripts for real-time prediction                                  |
| `unity_project/`    | Unity simulation project folder                                              |
| `.gitignore`        | Git rules to ignore Unity auto-generated and unnecessary files              |
| `README.md`         | Project documentation (this file)                                           |

---


## ⚙️ Setup Instructions
Navigate to the api/ directory.

Start the Flask server:

"python fault_detection_api.py" 

Use Postman or Unity to send input data to http://localhost:5000/predict.

## How to use

Send Input to the API
Example POST body (JSON):
{
  "Speed_Mach": 5.3,
  "Altitude_meters": 35000,
  "GPS_Signal_Strength_dBm": -85,
  "Inertial_Accel_m_s2": 1.8,
  "Doppler_Shift_Hz": 10
}

- Run Unity Simulation
  
Open unity_project/ using Unity 2021.3 LTS or newer.
Play the scene to observe real-time fault detection and response.


## Unity Project Notes

- Fault predictions are received via Flask API during runtime.

- Confirmed faults are visually and physically injected into the simulation.

- A fault log and summary panel are provided at the end of each flight session.

## 🐍 Python Environment

- **Python Version:** 3.9+
- **Install Dependencies:**
  ```bash
  pip install -r requirements.txt
 flask
   scikit-learn
   pandas
   numpy
   joblib
   matplotlib
   seaborn


##  📎 Original Repository
This GitHub repository is officially linked in the appendix of the final year dissertation submission for verification purposes.
''''
https://github.com/haseebmirza123/hypersonic-fault-detection
''''
