using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Wajib ada untuk mengelola scene

public class PauseMenu : MonoBehaviour
{
    // Variabel statis agar skrip lain bisa tahu apakah game sedang dijeda
    public static bool GameIsPaused = false;

    [Header("Referensi UI")]
    public GameObject pauseMenuUI; // Masukkan panel menu jeda Anda di sini
    public GameObject pauseButton;
    void Start()
    {
        // Pastikan menu jeda tidak aktif saat game dimulai
        pauseMenuUI.SetActive(false);
        pauseButton.SetActive(true);
    }

    void Update()
    {
        // Cek input Escape menggunakan Input System yang baru
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // Fungsi untuk melanjutkan permainan
    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Sembunyikan menu jeda
        pauseButton.SetActive(true); // Tampilkan tombol jeda
        Time.timeScale = 1f; // Kembalikan kecepatan waktu ke normal
        GameIsPaused = false;
        Debug.Log("Game Resumed!");
    }

    // Fungsi untuk menjeda permainan
    public void Pause()
    {
        pauseMenuUI.SetActive(true); // Tampilkan menu jeda
        pauseButton.SetActive(false); // Sembunyikan tombol jeda
        Time.timeScale = 0f; // Hentikan semua pergerakan dan waktu
        GameIsPaused = true;
        Debug.Log("Game Paused!");
    }

    // Fungsi untuk mengulang level
    public void Restart()
    {
        GameIsPaused = false;
        pauseMenuUI.SetActive(false); // Sembunyikan menu jeda
        pauseButton.SetActive(true); // Tampilkan tombol jeda
        Time.timeScale = 1f; // Selalu kembalikan timeScale sebelum pindah scene
        // Memuat ulang scene yang sedang aktif saat ini
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Restarting Level...");
    }

    // Fungsi untuk kembali ke Menu Utama
    public void LoadMenu()
    {
        GameIsPaused = false;
        pauseMenuUI.SetActive(false); // Sembunyikan menu jeda
        pauseButton.SetActive(true);
        Time.timeScale = 1f; // Kembalikan timeScale
        // Ganti "MainMenu" dengan nama scene menu utama Anda
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Loading Main Menu...");
    }
}