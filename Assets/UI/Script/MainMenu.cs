    using UnityEngine;
    using UnityEngine.SceneManagement; 

    public class MainMenu : MonoBehaviour
    {

        public void PlayGame()
        {
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
