using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioClip menuMusicClip; // drag musik menu di Inspector

    private void Start()
    {
        // Putar musik menu saat Main Menu terbuka
        if (AudioManager.instance != null && menuMusicClip != null)
        {
            AudioManager.instance.PlayMusic(menuMusicClip, true);
        }
    }

    public void PlayGame()
    {
        // Stop musik menu saat mulai game
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopMusic();
        }

        SceneManager.LoadScene("SampleMapScene");
    }

    // Fungsi untuk Quit
    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
        // Tidak akan terlihat di editor, tapi berfungsi di build
    }
}
