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

    [Header("Звуки погоды")]
    [SerializeField] private AudioSource weatherAudioSource; // Отдельный AudioSource для зацикленного амбиента
    [SerializeField] private float audioFadeSpeed = 1.5f;    // Скорость смены громкости
    [SerializeField] private AudioClip soundClear;    // Ясно  
    [SerializeField] private AudioClip soundCloudy;   // Облачно 
    [SerializeField] private AudioClip soundSnow;     // Снег  
    [SerializeField] private AudioClip soundStorm;    // Метель  
    [SerializeField] private AudioClip soundBuran;    // Буран 

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

    // Звуки
    private AudioClip targetClip;       // Клип к которому идём
    private float     targetVolume = 0f; // Громкость к которой идём

    private WeatherProfile activeProfile;

    void Start()
    {
        // Настраиваем AudioSource для амбиента
        if (weatherAudioSource != null)
        {
            weatherAudioSource.loop        = true;
            weatherAudioSource.playOnAwake = false;
            weatherAudioSource.volume      = 0f;
        }

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
        // Звуки
        UpdateWeatherAudio();
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
        SetWeatherAudio(profile);
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

        // Включаем нужный звук
        if (weatherAudioSource != null && targetClip != null)
        {
            weatherAudioSource.clip   = targetClip;
            weatherAudioSource.volume = targetVolume;
            weatherAudioSource.Play();
        }
    }

     // Определяет какой клип и громкость нужны для профиля
    private void SetWeatherAudio(WeatherProfile profile)
    {
        if (weatherAudioSource == null) return;

        // Подбираем клип по имени профиля
        AudioClip clip = profile.weatherName switch
        {
            "Ясно"    => soundClear,
            "Облачно" => soundCloudy,
            "Снег"    => soundSnow,
            "Метель"  => soundStorm,
            "Буран"   => soundBuran,
            _         => soundClear  // По умолчанию ясно
        };

        // Громкость зависит от интенсивности погоды
        float volume = profile.weatherName switch
        {
            "Ясно"    => 0.2f,
            "Облачно" => 0.3f,
            "Снег"    => 0.5f,
            "Метель"  => 0.75f,
            "Буран"   => 1.0f,
            _         => 0.3f
        };

        targetClip   = clip;
        targetVolume = volume;

        // Если клип сменился то перезапускаем AudioSource с новым клипом
        if (clip != null && weatherAudioSource.clip != clip)
        {
            weatherAudioSource.clip   = clip;
            weatherAudioSource.volume = 0f; // Начинаем с тишины
            weatherAudioSource.Play();
        }
    }

    // Плавно меняет громкость к целевой вызывается каждый кадр
    private void UpdateWeatherAudio()
    {
        if (weatherAudioSource == null) return;

        // Если клип не играет то запускаем
        if (!weatherAudioSource.isPlaying && weatherAudioSource.clip != null)
            weatherAudioSource.Play();

        // Плавно меняем громкость
        weatherAudioSource.volume = Mathf.MoveTowards(
            weatherAudioSource.volume,
            targetVolume,
            audioFadeSpeed * Time.deltaTime
        );
    }

    // Активный профиль
    public WeatherProfile GetActiveProfile() => activeProfile;
}