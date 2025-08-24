using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIOver : MonoBehaviour
{
    public static GameUIOver Instance; // singleton

    public GameObject gameOverUI; // drag panel GameOver di Inspector
    public AudioClip gameOverSound; // drag audio clip GameOver di Inspector
    public AudioClip gameplayMusicClip; // drag musik gameplay di Inspector
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void GameOver()
    {
        gameOverUI.SetActive(true);
        Time.timeScale = 0f;

        // Play Game Over SFX
        if (gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        // Stop background music
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopMusic();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // Restart musik gameplay dari awal
        if (AudioManager.instance != null && gameplayMusicClip != null)
        {
            AudioManager.instance.PlayMusic(gameplayMusicClip, true);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;

        // Bisa ganti dengan musik menu
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopMusic();
            // AudioManager.instance.PlayMusic(menuMusicClip);
        }

        SceneManager.LoadScene("MainMenu");
    }
}
