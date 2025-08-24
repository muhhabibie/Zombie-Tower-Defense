using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsPanel;

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogError("⚠ settingsPanel belum di-assign di Inspector!");
            return;
        }

        settingsPanel.SetActive(true);
        Debug.Log("✅ OpenSettings dipanggil, aktifkan panel: " + settingsPanel.name +
                  " | ActiveSelf=" + settingsPanel.activeSelf);
    }

    public void CloseSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogError("⚠ settingsPanel belum di-assign di Inspector!");
            return;
        }

        settingsPanel.SetActive(false);
        Debug.Log("❌ CloseSettings dipanggil, matikan panel: " + settingsPanel.name);
    }
}
