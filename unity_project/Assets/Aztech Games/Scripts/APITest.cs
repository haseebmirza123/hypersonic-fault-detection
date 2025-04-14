using UnityEngine;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class APITest : MonoBehaviour
{
    private readonly string apiUrl = "http://localhost:5000/predict"; // Flask API endpoint
    private readonly HttpClient httpClient = new HttpClient();

    void Start()
    {
        // Start the test when the script is initialized
        TestAPI();
    }

    private async void TestAPI()
    {
        FlightData testFlightData = new FlightData
        {
            Speed_Mach = 0.85f,
            Altitude_meters = 12000f,
            GPS_Signal_Strength_dBm = -65f,
            Inertial_Accel_m_s2 = 9.8f,
            Doppler_Shift_Hz = 1400f
        };

        string json = JsonConvert.SerializeObject(testFlightData);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                Debug.Log("✅ API Response: " + responseString);
            }
            else
            {
                Debug.LogError("❌ API request failed: " + response.StatusCode);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Error in API request: " + e.Message);
        }
    }

    // Data model for API communication
    public class FlightData
    {
        public float Speed_Mach;
        public float Altitude_meters;
        public float GPS_Signal_Strength_dBm;
        public float Inertial_Accel_m_s2;
        public float Doppler_Shift_Hz;
    }
}
