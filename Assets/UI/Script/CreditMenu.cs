using UnityEngine;

public class CreditMenu : MonoBehaviour
{
    [Header("Referensi Panel Menu")]
    public GameObject creditMenuPanel;

    // Fungsi ini akan dipanggil oleh tombol Credit
    public void ToggleCreditMenu()
    {
        // Pastikan referensi panel tidak kosong
        if (creditMenuPanel != null)
        {
            // Cek apakah panel sedang aktif, lalu lakukan kebalikannya
            bool isActive = creditMenuPanel.activeSelf;
            creditMenuPanel.SetActive(!isActive);
        }
    }
}