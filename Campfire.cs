using UnityEngine;

// Костёр имеет запас топлива (fuel), который убывает со временем. Если игрок находится в зоне триггера то он греется. В костёр можно подбрасывать дрова, чтобы продлить горение.
[RequireComponent(typeof(SphereCollider))]
public class Campfire : MonoBehaviour
{
    [Header("Топливо")]
    [SerializeField] private float maxFuel = 100f;        // Максимальный запас топлива
    [SerializeField] private float currentFuel = 50f;     // Текущий запас
    [SerializeField] private float fuelBurnRate = 1f;     // Сколько топлива сгорает в секунду
    [SerializeField] private float fuelPerLog = 25f;      // Сколько топлива даёт одно бревно

    [Header("Зона тепла")]
    [SerializeField] private float warmthRadius = 4f;     // Радиус зоны, в которой греется игрок

    [Header("Топливо префаб")]
    [SerializeField] private ItemData fuelItemData;       // Префаб Wood

    [Header("Визуал")]
    [SerializeField] private GameObject fireVisual;       // Визуал огня

    [Header("Свет")]
    [SerializeField] private Light fireLight;             // Свет от костра

    // Переменные
    private bool isLit = true;                            // Горит ли костёр сейчас
    private PlayerStats playerStatsInZone;                // Есть ли игрок рядом
    private SphereCollider triggerCollider;               // Триггер-зона тепла

    private void Awake()
    {
        // Настраиваем триггер-коллайдер автоматически
        triggerCollider = GetComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = warmthRadius;
    }

    private void Update()
    {
        if (!isLit) return; // Если костёр потух - ничего не делаем

        // Сжигаем топливо
        currentFuel -= fuelBurnRate * Time.deltaTime;

        // Лёгкое мерцание света
        if (fireLight != null)
        {
            // PingPong даёт значение от 0 до 1 и обратно — получаем мерцание интенсивности
            fireLight.intensity = 1.5f + Mathf.PingPong(Time.time * 2f, 0.5f);
        }

        // Если топливо закончилось — гасим костёр
        if (currentFuel <= 0f)
        {
            currentFuel = 0f;
            ExtinguishFire();
        }
    }

    // Когда что-то входит в зону тепла
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что вошёл именно игрок
        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats != null)
        {
            playerStatsInZone = stats;
            // Греем игрока только если костёр горит
            if (isLit)
            {
                stats.SetNearFire(true);
            }
        }
    }

    // Когда что-то выходит из зоны тепла
    private void OnTriggerExit(Collider other)
    {
        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats != null && stats == playerStatsInZone)
        {
            stats.SetNearFire(false);
            playerStatsInZone = null;
        }
    }

    // Гасим костёр
    private void ExtinguishFire()
    {
        isLit = false;

        // Отключаем визуал огня
        if (fireVisual != null) fireVisual.SetActive(false);
        if (fireLight != null) fireLight.enabled = false;

        // Если игрок был в зоне то он перестаёт греться
        if (playerStatsInZone != null)
        {
            playerStatsInZone.SetNearFire(false);
        }
    }

    // Зажигаем костёр снова после подбрасывания топлива
    private void RelightFire()
    {
        isLit = true;

        if (fireVisual != null) fireVisual.SetActive(true);
        if (fireLight != null) fireLight.enabled = true;

        // Если игрок в зоне то он снова греется
        if (playerStatsInZone != null)
        {
            playerStatsInZone.SetNearFire(true);
        }
    }

    // Подбрасывание бревен в костер
    public bool AddFuel(ItemData item)
    {
        // Проверяем, что подбрасываемый предмет это бревна
        if (item != fuelItemData)
        {
            return false;
        }

        // Проверяем, что в костре ещё есть место
        if (currentFuel >= maxFuel)
        {
            return false;
        }

        // Добавляем топливо но не больше максимума
        currentFuel = Mathf.Min(currentFuel + fuelPerLog, maxFuel);

        // Если костёр был потушен то разжигаем заново
        if (!isLit)
        {
            RelightFire();
        }
        return true;
    }

    // Геттеры для UI / системы заданий
    public float GetFuelPercent() => currentFuel / maxFuel;
    public bool IsLit() => isLit;

    // Отображает зону тепла в редакторе при выделении объекта
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, warmthRadius);
    }
}