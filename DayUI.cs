using UnityEngine;
using UnityEngine.UI;

// Управляет всем UI связанным с системой дней: показ задания, прогресс, экран смерти, экран победы.
public class DayUI : MonoBehaviour
{
    [Header("Панель старта дня")]
    [SerializeField] private GameObject dayStartPanel;
    [SerializeField] private Text dayTitleText; // Название дня
    [SerializeField] private Text taskDescText; // Описание задания

    [Header("HUD задания")]
    [SerializeField] private GameObject taskHUD;
    [SerializeField] private Text taskHUDText; // Вывод описания задания
    [SerializeField] private Text taskProgressText; // прогресс

    [Header("Уведомления")]
    [SerializeField] private GameObject taskCompletedBanner; // "Задание выполнено! Иди спать."
    [SerializeField] private GameObject taskNotDoneBanner; // "Сначала выполни задание!"
    [SerializeField] private float bannerDuration = 3f;

    [Header("Финальные экраны")]
    [SerializeField] private GameObject deathScreen; // Экран смерти
    [SerializeField] private GameObject victoryScreen; // Экран победы

    void Awake()
    {
        // Скрываем всё при старте
        if (dayStartPanel) dayStartPanel.SetActive(false);
        if (taskCompletedBanner) taskCompletedBanner.SetActive(false);
        if (taskNotDoneBanner) taskNotDoneBanner.SetActive(false);
        if (deathScreen) deathScreen.SetActive(false);
        if (victoryScreen) victoryScreen.SetActive(false);
    }

    // Показываем всплывающее окно с заданием в начале дня
    public void ShowDayStart(DayConfig config)
    {
        if (dayStartPanel == null) return;

        if (dayTitleText) dayTitleText.text = config.dayTitle;
        if (taskDescText) taskDescText.text = config.taskDescription;

        // Обновляем HUD задания
        if (taskHUDText) taskHUDText.text = $"Задание: {config.taskDescription}";
        if (taskProgressText) taskProgressText.text = $"0 / {config.requiredAmount}";

        dayStartPanel.SetActive(true);
        // Автоматически скрываем панель через 5 секунд
        Invoke(nameof(HideDayStart), 5f);
    }

    private void HideDayStart()
    {
        if (dayStartPanel) dayStartPanel.SetActive(false);
    }

    // Обновляем прогресс в HUD
    public void UpdateProgress(float current, float total)
    {
        if (taskProgressText)
            taskProgressText.text = $"{Mathf.FloorToInt(current)} / {Mathf.FloorToInt(total)}";
    }

    // Задание выполнено
    public void ShowTaskComplete()
    {
        if (taskCompletedBanner)
        {
            taskCompletedBanner.SetActive(true);
            Invoke(nameof(HideTaskComplete), bannerDuration);
        }
    }

    private void HideTaskComplete()
    {
        if (taskCompletedBanner) taskCompletedBanner.SetActive(false);
    }

    // Попытка лечь спать без выполнения задания
    public void ShowTaskNotDone()
    {
        if (taskNotDoneBanner)
        {
            taskNotDoneBanner.SetActive(true);
            Invoke(nameof(HideTaskNotDone), bannerDuration);
        }
    }

    private void HideTaskNotDone()
    {
        if (taskNotDoneBanner) taskNotDoneBanner.SetActive(false);
    }

    // Экраны
    public void ShowDeathScreen()
    {
        if (deathScreen) deathScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowVictoryScreen()
    {
        if (victoryScreen) victoryScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        AudioManager.Instance?.PlayVictoryMusic();
    }

    // Вызывается кнопкой «В главное меню» на экране победы
    public void OnVictoryToMenuClicked()
    {
        PlayerPrefs.SetInt("CurrentDay", 0); 
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // Позволяет PauseMenu проверить не показан ли экран смерти/победы
    public bool IsEndScreenActive()
    {
        return (deathScreen != null && deathScreen.activeSelf) ||
            (victoryScreen != null && victoryScreen.activeSelf);
    }
}