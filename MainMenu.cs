using UnityEngine;
using UnityEngine.SceneManagement;

// Управляет кнопками главного меню.

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "MainMap"; 

    public void PlayGame()
    {
        // Загружаем сцену игры
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        // Разблокируем курсор перед выходом
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Выход из игры
        Application.Quit();
    }
}