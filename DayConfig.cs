using UnityEngine;

// ScriptableObject — конфигурация одного игрового дня.
[CreateAssetMenu(fileName = "Day_01", menuName = "Days/Day Config", order = 1)]
public class DayConfig : ScriptableObject
{
    [Header("Общее")]
    public string dayTitle = "День 1"; // Название дня для UI

    [Header("Погода")]
    public WeatherProfile weatherProfile; // Профиль погоды этого дня

    [Header("Задание")]
    public TaskType taskType; // Тип задания
    public ItemData requiredItem; // Предмет
    public int requiredAmount = 1; // Нужное количество
    [TextArea] public string taskDescription; // Текст задания для UI

    [Header("Стартовые предметы")]
    public ItemData[] startItems; // Предметы которые дают игроку в начале дня
    public int[] startAmounts; // Количества стартовых предметов
}

// Типы заданий
public enum TaskType
{
    CraftItem, // Скрафтить предмет
    ChopLogs, // Нарубить брёвен
    SurviveNearFire, // Простоять N секунд у костра
    CollectItems, // Подобрать N предметов
}