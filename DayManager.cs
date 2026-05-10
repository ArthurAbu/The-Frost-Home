using UnityEngine;
using UnityEngine.SceneManagement;

// Главный менеджер прогрессии. Управляет текущим днём, заданием, применяет погоду и следит за выполнением условий победы/поражения.
public class DayManager : MonoBehaviour
{
    [Header("Конфигурации дней")]
    [SerializeField] private DayConfig[] dayConfigs;

    [Header("Ссылки на системы")]
    [SerializeField] private WeatherSystem weatherSystem;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private DayUI dayUI;

    // Текущий индекс дня
    private int currentDayIndex;
    private DayConfig currentDay;

    // Прогресс задания
    private float taskProgress = 0f; // Текущее значение
    private bool taskCompleted = false; // Выполнено ли задание
    private float fireTimer = 0f; // Таймер для задания SurviveNearFire
    private bool isHandlingDeath = false; // Умер ли игрок 

    void Start()
    {
        // Читаем индекс текущего дня из PlayerPrefs
        currentDayIndex = PlayerPrefs.GetInt("CurrentDay", 0);

        // Все дни пройдены то показываем экран победы
        if (currentDayIndex >= dayConfigs.Length)
        {
            HandleVictory();
            return;
        }

        currentDay = dayConfigs[currentDayIndex];

        // Применяем погоду этого дня
        if (weatherSystem != null && currentDay.weatherProfile != null)
            weatherSystem.ApplyProfileInstant(currentDay.weatherProfile);

        // Выдаём стартовые предметы
        GiveStartItems();

        // Смерть игрока
        if (playerStats != null)
            playerStats.onPlayerDied.AddListener(OnPlayerDied);

        // Показываем UI задания
        if (dayUI != null)
            dayUI.ShowDayStart(currentDay);
    }

    void Update()
    {
        if (currentDay == null) return;

        if (taskCompleted || isHandlingDeath) return;

        // Задание "Выжить у костра" считаем время через Update
        if (currentDay.taskType == TaskType.SurviveNearFire)
        {
            if (playerStats != null && playerStats.IsNearFire())
            {
                fireTimer += Time.deltaTime;
                taskProgress = fireTimer;

                if (dayUI != null)
                {
                    dayUI.UpdateProgress(taskProgress, currentDay.requiredAmount);
                }

                if (fireTimer >= currentDay.requiredAmount)
                {
                    CompleteTask();
                }
            }
        }
        // Вызываем отладку отдельным методом
        HandleDebugInput();
    }

    // Проверка заданий
    // Вызывается из CraftingSystem когда игрок что-то скрафтил
    public void OnItemCrafted(ItemData item)
    {
        if (taskCompleted) return;
        if (currentDay.taskType != TaskType.CraftItem) return;
        if (item != currentDay.requiredItem) return;

        taskProgress++;

        if (dayUI != null)
            dayUI.UpdateProgress(taskProgress, currentDay.requiredAmount);

        if (taskProgress >= currentDay.requiredAmount)
            CompleteTask();
    }

    // Вызывается из PlayerInteraction когда игрок подобрал предмет
    public void OnItemCollected(ItemData item)
    {
        if (taskCompleted) return;
        if (item != currentDay.requiredItem) return;

        if (currentDay.taskType == TaskType.ChopLogs ||
            currentDay.taskType == TaskType.CollectItems)
        {
            taskProgress++;

            if (dayUI != null)
                dayUI.UpdateProgress(taskProgress, currentDay.requiredAmount);

            if (taskProgress >= currentDay.requiredAmount)
                CompleteTask();
        }
    }

    // Завершение
    public void CompleteTask()
    {
        taskCompleted = true;
        if (dayUI != null) dayUI.ShowTaskComplete();
        AudioManager.Instance?.PlayDayComplete(); // ← Звук выполнения задания
    }

    // Вызывается BedInteraction когда игрок ложится спать
    public bool TrySleep()
    {
        if (!taskCompleted)
        {
            // Если задание не выполнено то не пускаем спать
            if (dayUI != null) dayUI.ShowTaskNotDone();
            return false;
        }

        // Если это последний день то сразу победа, без перезагрузки
        if (currentDayIndex >= dayConfigs.Length - 1)
        {
            PlayerPrefs.SetInt("CurrentDay", 0); // Сбрасываем прогресс для следующего запуска
            PlayerPrefs.Save();
            HandleVictory(); // Показываем победный экран прямо сейчас
            return true;
        }
        // Обычный день
        // Сохраняем следующий день
        PlayerPrefs.SetInt("CurrentDay", currentDayIndex + 1);
        PlayerPrefs.Save();
        // Перезагружаем сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        return true;
    }

    // Смерть игрока — перезагружаем текущий день
    private void OnPlayerDied()
    {
        if (isHandlingDeath) return;
        isHandlingDeath = true;

        if (dayUI != null) dayUI.ShowDeathScreen();

        // Небольшая задержка перед перезагрузкой чтобы игрок увидел экран смерти
        Invoke(nameof(RestartCurrentDay), 3f);
    }

    private void RestartCurrentDay()
    {
        // Индекс дня не меняем и перезапускаем тот же день
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Все 10 дней пройдены
    private void HandleVictory()
    {
        PlayerPrefs.SetInt("CurrentDay", 0); // Сбрасываем прогресс
        PlayerPrefs.Save();
        //Разблокировка курсора
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (dayUI != null)
        {
            dayUI.ShowVictoryScreen();
        }
        else
        {
            Debug.LogError("DayUI не назначен в DayManager!");
        }
    }

    // Раздаём стартовые предметы
    private void GiveStartItems()
    {
        if (currentDay.startItems == null) return;

        for (int i = 0; i < currentDay.startItems.Length; i++)
        {
            if (currentDay.startItems[i] == null) continue;
            int amount = i < currentDay.startAmounts.Length ? currentDay.startAmounts[i] : 1;
            inventory.AddItem(currentDay.startItems[i], amount);
        }
    }

    // Геттеры для внешних систем
    public bool IsTaskCompleted() => taskCompleted;
    public DayConfig GetCurrentDay() => currentDay;
    public int GetCurrentDayNumber() => currentDayIndex + 1;


    // Горячие клавиши для отладки
        private void HandleDebugInput(){
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1)) DebugSkipDay(-1); // Предыдущий день
        if (Input.GetKeyDown(KeyCode.F2)) DebugSkipDay(1);  // Следующий день
        if (Input.GetKeyDown(KeyCode.F3)) CompleteTask();    // Выполнить задание мгновенно
        #endif
        }

        #if UNITY_EDITOR
        private void DebugSkipDay(int direction)
        {
            int newDay = Mathf.Clamp(currentDayIndex + direction, 0, dayConfigs.Length - 1);
            PlayerPrefs.SetInt("CurrentDay", newDay);
            PlayerPrefs.Save();
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
        [ContextMenu("Сбросить прогресс")]
        private void ResetProgress()
        {
            PlayerPrefs.SetInt("CurrentDay", 0);
            PlayerPrefs.Save();
            Debug.Log("Прогресс сброшен. Перезапусти Play Mode.");
        }
        #endif
}