using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; 

public class PauseMenu : MonoBehaviour
{
    
    public static bool GameIsPaused = false;

    [Header("Referensi UI")]
    public GameObject pauseMenuUI; 
    public GameObject pauseButton;
    void Start()
    {
        
        pauseMenuUI.SetActive(false);
        pauseButton.SetActive(true);
    }

    void Update()
    {
        
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

    
    public void Resume()
    {
        pauseMenuUI.SetActive(false); 
        pauseButton.SetActive(true); 
        Time.timeScale = 1f; 
        GameIsPaused = false;
        Debug.Log("Game Resumed!");
    }

    
    public void Pause()
    {
        pauseMenuUI.SetActive(true); 
        pauseButton.SetActive(false); 
        Time.timeScale = 0f; 
        GameIsPaused = true;
        Debug.Log("Game Paused!");
    }

    
    public void Restart()
    {
        GameIsPaused = false;
        pauseMenuUI.SetActive(false); 
        pauseButton.SetActive(true); 
        Time.timeScale = 1f; 
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Restarting Level...");
    }

    // Fungsi untuk kembali ke Menu Utama
    public void LoadMenu()
    {
        GameIsPaused = false;
        pauseMenuUI.SetActive(false); 
        pauseButton.SetActive(true);
        Time.timeScale = 1f; 
        
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Loading Main Menu...");
    }
}