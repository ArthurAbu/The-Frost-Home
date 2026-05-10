using UnityEngine;

// Управление звуковыми эффектами. Хранит все аудио и воспроизводит их по запросу других систем.
public class AudioManager : MonoBehaviour
{
    [Header("Рубка деревьев")]
    [SerializeField] private AudioClip[] chopSounds;  // Удары топором
    [SerializeField] private AudioClip treeFallSound; // Звук падения дерева

    [Header("Добыча камня")]
    [SerializeField] private AudioClip[] mineSounds;    // Удары киркой
    [SerializeField] private AudioClip rockBreakSound;  // Камень разрушен

    [Header("Костёр")]
    [SerializeField] private AudioClip addFuelSound;  // Подброс бревна
    [SerializeField] private AudioClip fireLoopSound; // Зацикленный треск огня
    [SerializeField] private AudioClip fireExtinguishSound; // Костёр потух

    [Header("Инвентарь")]
    [SerializeField] private AudioClip pickupSound;   // Подбор предмета
    [SerializeField] private AudioClip dropSound;     // Выброс предмета
    [SerializeField] private AudioClip eatSound;      // Поедание еды

    [Header("UI")]
    [SerializeField] private AudioClip craftSound;    // Успешный крафт
    [SerializeField] private AudioClip errorSound;    // Ошибка (нет ингредиентов)
    [SerializeField] private AudioClip dayCompleteSound; // Задание выполнено
    [SerializeField] private AudioClip victoryMusic;     // Музыка победного экрана

    [Header("Голод")]
    [SerializeField] private AudioClip hungerWarningSound;  // Звук при критическом голоде
    [SerializeField] private float hungerWarningInterval = 8f; // Пауза между повторами

    [Header("Холод")]
    [SerializeField] private AudioClip coldWarningSound;    // Звук при критическом холоде
    [SerializeField] private float coldWarningInterval = 6f;

    [Header("Урон")]
    [SerializeField] private AudioClip[] hurtSounds;        // Звуки получения урона
    [SerializeField] private AudioClip deathSound;          // Звук смерти

    [Header("Ходьба")]
    [SerializeField] private AudioClip[] footstepSounds;    // Звуки шагов
    [SerializeField] private float footstepInterval = 0.45f; // Пауза между шагами
    [SerializeField] private float runFootstepInterval = 0.28f; // Шаги при беге

    [Header("Источники звука")]
    [SerializeField] private AudioSource sfxSource;   // Источник для коротких звуков
    [SerializeField] private AudioSource fireSource;  // Отдельный источник для костра (зацикленный звук)
    [SerializeField] private AudioSource footstepSource;    // Шаги
    [SerializeField] private AudioSource warningSource;     // Предупреждения выживания
    [SerializeField] private AudioSource musicSource;    // Отдельный источник для музыки

    // Таймеры
    private float hungerWarningTimer = 0f;
    private float coldWarningTimer   = 0f;
    private float footstepTimer      = 0f;

    // Отслеживаем последний процент здоровья чтобы засечь каждые 10%
    private int lastHealthTenPercent = 10;

    // Синглтон чтобы другие скрипты обращались без ссылки
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Таймеры предупреждений тикают всегда, сброс при вызове методов
        hungerWarningTimer += Time.deltaTime;
        coldWarningTimer   += Time.deltaTime;
        footstepTimer      += Time.deltaTime;
    }

    // Рубка деревьев Случайный звук удара
    public void PlayChopSound()
    {
        if (chopSounds.Length == 0) return;
        AudioClip clip = chopSounds[Random.Range(0, chopSounds.Length)];
        sfxSource.PlayOneShot(clip);
    }
    public void PlayTreeFall()
    {
        if (treeFallSound != null)
            sfxSource.PlayOneShot(treeFallSound);
    }

    // Добыча камня

    public void PlayMineSound()
    {
        if (mineSounds.Length == 0) return;
        sfxSource.PlayOneShot(mineSounds[Random.Range(0, mineSounds.Length)]);
    }
    public void PlayRockBreak()
    {
        if (rockBreakSound != null)
            sfxSource.PlayOneShot(rockBreakSound);
    }

    // Костёр
    public void PlayAddFuel()
    {
        if (addFuelSound != null)
            sfxSource.PlayOneShot(addFuelSound);
    }
    // Зацикленный треск огня
    public void StartFireLoop()
    {
        if (fireSource == null || fireLoopSound == null) return;
        if (fireSource.isPlaying) return;
        fireSource.clip = fireLoopSound;
        fireSource.loop = true;
        fireSource.Play();
    }

    public void StopFireLoop()
    {
        if (fireSource != null && fireSource.isPlaying)
            fireSource.Stop();
    }

    public void PlayFireExtinguish()
    {
        if (fireExtinguishSound != null)
            sfxSource.PlayOneShot(fireExtinguishSound);
    }

    //Инвентарь
    public void PlayPickup()
    {
        if (pickupSound != null)
            sfxSource.PlayOneShot(pickupSound);
    }

    public void PlayDrop()
    {
        if (dropSound != null)
            sfxSource.PlayOneShot(dropSound);
    }

    public void PlayEat()
    {
        if (eatSound != null)
            sfxSource.PlayOneShot(eatSound);
    }

    //Крафт и UI
    public void PlayCraft()
    {
        if (craftSound != null)
            sfxSource.PlayOneShot(craftSound);
    }

    public void PlayError()
    {
        if (errorSound != null)
            sfxSource.PlayOneShot(errorSound);
    }

    public void PlayDayComplete()
    {
        if (dayCompleteSound != null)
            sfxSource.PlayOneShot(dayCompleteSound);
    }

    // Голод 
    public void TryPlayHungerWarning()
    {
        if (hungerWarningSound == null) return;
        if (hungerWarningTimer < hungerWarningInterval) return;

        warningSource.PlayOneShot(hungerWarningSound);
        hungerWarningTimer = 0f;
    }
    // Сбрасываем таймер когда игрок поел чтобы звук не играл сразу после еды
    public void ResetHungerWarning()
    {
        hungerWarningTimer = hungerWarningInterval;
    }

    // Холод
    public void TryPlayColdWarning()
    {
        if (coldWarningSound == null) return;
        if (coldWarningTimer < coldWarningInterval) return;

        warningSource.PlayOneShot(coldWarningSound);
        coldWarningTimer = 0f;
    }
    public void ResetColdWarning()
    {
        coldWarningTimer = coldWarningInterval;
    }

    // Урон и смерть
    public void PlayHurt()
    {
        if (hurtSounds.Length == 0) return;
        sfxSource.PlayOneShot(hurtSounds[Random.Range(0, hurtSounds.Length)]);
    }
    public void PlayDeath()
    {
        if (deathSound != null) sfxSource.PlayOneShot(deathSound);
    }

    // Проигрывает звук при пересечении каждого порога в 10%
    public void CheckHealthThreshold(float healthPercent)
    {
        // Переводим в десятки
        int currentTen = Mathf.CeilToInt(healthPercent * 10);

        // Если пересекли порог вниз то играем звук
        if (currentTen < lastHealthTenPercent)
        {
            if (healthPercent > 0f)
                PlayHurt();
            else
                PlayDeath();

            lastHealthTenPercent = currentTen;
        }

        // Если здоровье восстановилось то обновляем порог
        if (currentTen > lastHealthTenPercent)
            lastHealthTenPercent = currentTen;
    }

    // Шаги
    public void TryPlayFootstep(bool isRunning)
    {
        if (footstepSounds.Length == 0) return;

        float interval = isRunning ? runFootstepInterval : footstepInterval;

        if (footstepTimer < interval) return;

        // Случайный звук из 4 вариантов
        AudioClip step = footstepSounds[Random.Range(0, footstepSounds.Length)];
        footstepSource.clip = step;
        footstepSource.Play();
        footstepTimer = 0f;
    }

    // Сбрасываем таймер когда игрок останавливается, чтобы первый шаг после остановки звучал сразу
    public void ResetFootstepTimer()
    {
        footstepTimer = 0f;
    }

    // Победный экран
    public void PlayVictoryMusic()
    {
        if (musicSource == null || victoryMusic == null) return;

        musicSource.Stop();
        musicSource.loop   = false;
        musicSource.clip   = victoryMusic;
        musicSource.volume = 0.8f;
        musicSource.Play();
    }
}