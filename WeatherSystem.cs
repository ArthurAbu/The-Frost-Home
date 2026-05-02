using UnityEngine;

// Главный скрипт управления погодой. Принимает WeatherProfile и плавно применяет его параметры к снегу, туману, освещению и влияет на потерю тепла игрока.
public class WeatherSystem : MonoBehaviour
{
    [Header("Стартовая погода")]
    [SerializeField] private WeatherProfile startingWeather; // Профиль с которого начинается день

    [Header("Ссылки на объекты сцены")]
    [SerializeField] private ParticleSystem snowParticles;   // Партикл снега
    [SerializeField] private Light sunLight;                 // Основной свет
    [SerializeField] private PlayerStats playerStats;        // Чтобы влиять на потерю тепла

    [Header("Скорость перехода между погодами")]
    [SerializeField] private float transitionDuration = 5f;  // Сколько секунд идёт плавный переход

    // Текущие значения
    private float currentSnowRate;
    private Color currentFogColor;
    private float currentFogDensity;
    private Color currentAmbientColor;
    private float currentSunIntensity;
    private float currentWarmthMultiplier = 1f;

    // Целевые значения (куда движутся текущие)
    private float targetSnowRate;
    private Color targetFogColor;
    private float targetFogDensity;
    private bool targetFogEnabled;
    private Color targetAmbientColor;
    private float targetSunIntensity;
    private float targetWarmthMultiplier;

    private WeatherProfile activeProfile;

    void Start()
    {
        // Применяем стартовую погоду мгновенно
        if (startingWeather != null)
        {
            ApplyProfileInstant(startingWeather);
        }
    }

    void Update()
    {
        // Плавно меняем все параметры к целевым значениям
        float t = Time.deltaTime / transitionDuration;

        currentSnowRate = Mathf.Lerp(currentSnowRate, targetSnowRate, t);
        currentFogColor = Color.Lerp(currentFogColor, targetFogColor, t);
        currentFogDensity = Mathf.Lerp(currentFogDensity, targetFogDensity, t);
        currentAmbientColor = Color.Lerp(currentAmbientColor, targetAmbientColor, t);
        currentSunIntensity = Mathf.Lerp(currentSunIntensity, targetSunIntensity, t);
        currentWarmthMultiplier = Mathf.Lerp(currentWarmthMultiplier, targetWarmthMultiplier, t);

        // Применяем интерполированные значения к реальным объектам сцены
        ApplyCurrentValues();
    }

    // Применить текущие значения к снегу, туману, освещению и игроку
    private void ApplyCurrentValues()
    {
        // Снег
        if (snowParticles != null)
        {
            var emission = snowParticles.emission;
            emission.rateOverTime = currentSnowRate;
        }

        // Туман. Меняем глобальные настройки RenderSettings
        RenderSettings.fog = targetFogEnabled;
        RenderSettings.fogColor = currentFogColor;
        RenderSettings.fogDensity = currentFogDensity;
        RenderSettings.fogMode = FogMode.ExponentialSquared;

        // Освещение. Меняем окружающий свет и яркость солнца
        RenderSettings.ambientLight = currentAmbientColor;
        if (sunLight != null) sunLight.intensity = currentSunIntensity;

        // Влияние на игрока который передаём множитель в PlayerStats
        if (playerStats != null)
        {
            playerStats.SetWarmthDecayMultiplier(currentWarmthMultiplier);
        }
    }

    // Плавная смена погоды
    public void SetWeather(WeatherProfile profile)
    {
        if (profile == null) return;

        activeProfile = profile;

        // Запоминаем целевые значения
        targetSnowRate = profile.snowEmissionRate;
        targetFogEnabled = profile.fogEnabled;
        targetFogColor = profile.fogColor;
        targetFogDensity = profile.fogDensity;
        targetAmbientColor = profile.ambientColor;
        targetSunIntensity = profile.sunIntensity;
        targetWarmthMultiplier = profile.warmthDecayMultiplier;
    }

    // Мгновенная смена погоды для старта сцены или загрузки
    public void ApplyProfileInstant(WeatherProfile profile)
    {
        if (profile == null) return;

        SetWeather(profile);
        currentSnowRate = targetSnowRate;
        currentFogColor = targetFogColor;
        currentFogDensity = targetFogDensity;
        currentAmbientColor = targetAmbientColor;
        currentSunIntensity = targetSunIntensity;
        currentWarmthMultiplier = targetWarmthMultiplier;

        ApplyCurrentValues();
    }

    // Активный профиль
    public WeatherProfile GetActiveProfile() => activeProfile;
}