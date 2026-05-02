using UnityEngine;
using UnityEngine.UI;

// Этот скрипт отвечает за отображение показателей на экране
public class PlayerStatsUI : MonoBehaviour
{
    [Header("Ссылка на данные игрока")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Полоски показателей")]
    [SerializeField] private Image healthBar;   // Красная полоска здоровья
    [SerializeField] private Image hungerBar;   // Оранжевая полоска голода
    [SerializeField] private Image warmthBar;   // Синяя полоска тепла

    [Header("Цвета полосок")]
    // Нормальный цвет (полная полоска)
    [SerializeField] private Color healthColorNormal = Color.red; // Красный
    [SerializeField] private Color hungerColorNormal = new Color(1f, 0.5f, 0f); // Оранжевый
    [SerializeField] private Color warmthColorNormal = new Color(0.2f, 0.6f, 1f);  // Голубой
    [SerializeField] private Color criticalColor = Color.white; // Белый

    [Header("Мигание при критическом уровне")]
    [SerializeField] private float criticalThreshold = 0.25f; // При каком проценте полоска начинает мигать
    [SerializeField] private float blinkSpeed = 3f; // Скорость мигания при критическом состоянии

    void Update()
    {
        if (playerStats == null) return;

        // Обновляем каждую полоску каждый кадр
        UpdateBar(healthBar,  playerStats.GetHealthPercent(),  healthColorNormal);
        UpdateBar(hungerBar,  playerStats.GetHungerPercent(),  hungerColorNormal);
        UpdateBar(warmthBar,  playerStats.GetWarmthPercent(),  warmthColorNormal);
    }

    // Обновляет заполненность и цвет полоски
    private void UpdateBar(Image bar, float percent, Color normalColor)
    {
        if (bar == null) return;

        // Устанавливаем заполненность полоски
        bar.fillAmount = percent;

        // Если показатель критический то мигаем белым цветом
        if (percent <= criticalThreshold)
        {
            float blink = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            bar.color = Color.Lerp(criticalColor, normalColor, blink);
        }
        else
        {
            bar.color = normalColor;
        }
    }
}