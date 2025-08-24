using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIOver : MonoBehaviour
{
    public static GameUIOver Instance; // singleton

    public GameObject gameOverUI; // drag panel GameOver di Inspector

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GameOver()
    {
        gameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }
}
