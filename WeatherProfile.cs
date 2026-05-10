using UnityEngine;

// ScriptableObject — данные о погодных условиях.
[CreateAssetMenu(fileName = "NewWeather", menuName = "Weather/Weather Profile", order = 1)]
public class WeatherProfile : ScriptableObject
{
    [Header("Идентификация")]
    public string weatherName = "Название"; // Название погоды

    [Header("Снегопад")]
    [Range(0f, 500f)]
    public float snowEmissionRate = 0f; // Сколько снежинок в секунду

    [Header("Туман")]
    public bool fogEnabled = false; // Включён ли туман
    [ColorUsage(true, false)]
    public Color fogColor = Color.white; // Цвет тумана
    [Range(0f, 0.1f)]
    public float fogDensity = 0.01f; // Плотность тумана

    [Header("Освещение")]
    [ColorUsage(true, false)]
    public Color ambientColor = new Color(0.6f, 0.6f, 0.6f); // Цвет окружающего света
    [Range(0f, 2f)]
    public float sunIntensity = 1f; // Яркость основного источника света (солнца)

    [Header("Влияние на игрока")]
    [Range(0.5f, 5f)]
    public float warmthDecayMultiplier = 1f; // Множитель скорости потери тепла
}