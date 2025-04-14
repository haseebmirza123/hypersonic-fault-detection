using UnityEngine;
using UnityEngine.UI;

public class FaultModeToggle : MonoBehaviour
{
    public Toggle modeToggle; // ✅ Assign this in the Inspector

    void Start()
    {
        // ✅ Auto-assign the Toggle if not manually assigned
        if (modeToggle == null)
        {
            modeToggle = GetComponent<Toggle>();
            if (modeToggle == null)
            {
                Debug.LogError("❌ FaultModeToggle: No Toggle component found on this GameObject!");
                return;
            }
        }

        // ✅ Load the previous selection (default to Manual Mode if not set)
        modeToggle.isOn = PlayerPrefs.GetInt("FaultInjectionMode", 0) == 1;

        // ✅ Add listener for changes
        modeToggle.onValueChanged.AddListener(delegate { SaveModeSelection(); });

        Debug.Log($"🔄 Fault Mode Toggle Initialized: {(modeToggle.isOn ? "AI Mode" : "Manual Mode")}");
    }

    void SaveModeSelection()
    {
        // ✅ Save the selected mode (1 = AI, 0 = Manual)
        PlayerPrefs.SetInt("FaultInjectionMode", modeToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"✅ Fault Injection Mode Saved: {(modeToggle.isOn ? "AI Mode" : "Manual Mode")}");
    }
}
