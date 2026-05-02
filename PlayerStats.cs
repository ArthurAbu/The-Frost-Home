using UnityEngine;
using UnityEngine.Events;

// Этот скрипт хранит все показатели выживания игрока и управляет их изменением.
public class PlayerStats : MonoBehaviour
{
    // Настройки показателей
    [Header("Здоровье")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Голод")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float currentHunger = 100f;
    [SerializeField] private float hungerDecayRate = 0.5f; // Сколько единиц голода теряется в секунду

    [Header("Тепло")]
    [SerializeField] private float maxWarmth = 100f;
    [SerializeField] private float currentWarmth = 100f;
    [SerializeField] private float warmthDecayRate = 1f; // Сколько тепла теряется в секунду вдали от огня
    [SerializeField] private float warmthRecoveryRate = 5f; // Сколько тепла восстанавливается в секунду рядом с огнём

    [Header("Урон от критических состояний")]
    [SerializeField] private float hungerDamageRate = 2f; // Урон в секунду при нулевом голоде
    [SerializeField] private float coldDamageRate = 3f; // Урон в секунду при нулевом тепле

    [Header("Событие смерти")]
    public UnityEvent onPlayerDied; // Это событие сработает когда здоровье = 0.

    private float warmthDecayMultiplier = 1f;  // Множитель скорости потери тепла от погоды
    private bool isNearFire = false; // Находится ли игрок рядом с источником тепла
    private bool isDead = false; // Умер ли игрок чтобы не вызывать смерть повторно

    void Update()
    {
        if (isDead) return; // Если уже мёртв  ничего не делаем

        UpdateHunger();
        UpdateWarmth();
        CheckDamageFromStats();
    }

    // Уменьшаем голод каждую секунду
    private void UpdateHunger()
    {
        currentHunger -= hungerDecayRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger); // Не даём уйти ниже 0
    }

    // Уменьшаем тепло если далеко от огня, восстанавливаем если рядом
    private void UpdateWarmth()
    {
        if (isNearFire)
        {
            // Тепло
            currentWarmth += warmthRecoveryRate * Time.deltaTime;
        }
        else
        {
            // Холодно учитываем множитель погоды
            currentWarmth -= warmthDecayRate * warmthDecayMultiplier * Time.deltaTime;
        }

        currentWarmth = Mathf.Clamp(currentWarmth, 0f, maxWarmth);
    }

    // Наносит урон если голод или тепло достигли нуля
    private void CheckDamageFromStats()
    {
        if (currentHunger <= 0f) TakeDamage(hungerDamageRate * Time.deltaTime);

        if (currentWarmth <= 0f) TakeDamage(coldDamageRate * Time.deltaTime);
    }

    // Нанести урон игроку
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth <= 0f) Die();
    }

    // Восстановить здоровье
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    // Восстановить голод
    public void AddFood(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
    }

    // Вызывается скриптом костра
    public void SetNearFire(bool value)
    {
        isNearFire = value;
    }

    // Вызывается WeatherSystem для изменения скорости потери тепла от погоды
    public void SetWarmthDecayMultiplier(float multiplier)
    {
        warmthDecayMultiplier = multiplier;
    }

    // Геттеры для UI

    public float GetHealthPercent()  => currentHealth / maxHealth;
    public float GetHungerPercent()  => currentHunger / maxHunger;
    public float GetWarmthPercent()  => currentWarmth / maxWarmth;

    // Геттеры для точных значений для системы дней
    public float GetHealth() => currentHealth;
    public float GetHunger() => currentHunger;
    public float GetWarmth() => currentWarmth;
    public bool  IsDead() => isDead;

    // Геттер для DayManager
    public bool IsNearFire() => isNearFire;

    // Смерть игрока
    private void Die()
    {
        isDead = true;
        // Вызываем событие смерти
        onPlayerDied?.Invoke();
    }
}