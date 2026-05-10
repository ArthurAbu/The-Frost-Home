using UnityEngine;
using UnityEngine.SceneManagement;

// Меню паузы. Открывается по Escape во время игры
public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel; // Панель паузы

    [Header("Ссылки")]
    [SerializeField] private DayUI dayUI; // Для проверки открыт ли экран смерти или победы

    private bool isPaused = false;

    void Update()
    {
        // Открытие и закрытие паузы по Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Не открываем паузу если показан экран смерти или победы
            if (dayUI != null && dayUI.IsEndScreenActive()) return;

            if (isPaused) Resume();
            else Pause();
        }
    }

    // Открыть меню паузы
    private void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);

        // Останавливаем время и разблокируем курсор
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Продолжить игру вызывается кнопкой «Продолжить»
    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);

        // Возобновляем время и блокируем курсор
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Начать день заново
    public void RestartDay()
    {
        // Обязательно сбрасываем timeScale перед перезагрузкой
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Главное меню
    public void GoToMainMenu()
    {
        // Сбрасываем timeScale и курсор перед переходом
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MainMenu");
    }
}