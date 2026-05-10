using UnityEngine;

// Добыча камня. После добычи камень разрушается и выпадает лут.
public class MineableRock : MonoBehaviour
{
    [Header("Настройки добычи")]
    [SerializeField] private int hitsRequired = 4;      // Ударов до разрушения
    [SerializeField] private string requiredToolName = "Кирка"; // Имя нужного инструмента

    [Header("Выпадающие ресурсы")]
    [SerializeField] private ItemData stoneItemData;     // Камень
    [SerializeField] private int      stoneDropCount = 3;

    [SerializeField] private ItemData flintItemData;     // Кремень (необязательно)
    [SerializeField] private int      flintDropCount = 1;

    [SerializeField] private ItemData ironItemData;      // Железо (необязательно)
    [SerializeField] private int      ironDropCount  = 0;

    private int currentHits = 0;

    // Вызывается из PlayerInteraction при каждом ударе ЛКМ. Возвращает true если камень разрушен.
    public bool TryMine(PlayerInteraction interactor, string toolName)
    {
        // Проверяем что инструмент подходит
        if (toolName != requiredToolName) return false;

        currentHits++;

        AudioManager.Instance?.PlayMineSound(); // Звук удара

        if (currentHits >= hitsRequired)
        {
            AudioManager.Instance?.PlayRockBreak(); // Звук разрушения камня
            SpawnLootAndDestroy(interactor);
            return true;
        }

        return false;
    }

    // Геттер прогресса для UI или отладки
    public float GetMineProgress() => (float)currentHits / hitsRequired;

    private void SpawnLootAndDestroy(PlayerInteraction interactor)
    {
        // Точка появления ресурсов
        Vector3 dropPos = transform.position + Vector3.up * 0.3f;

        // Спавним камень
        SpawnDrop(interactor, stoneItemData, stoneDropCount, dropPos);
        // Спавним кремень
        SpawnDrop(interactor, flintItemData, flintDropCount, dropPos);
        // Спавним железо
        SpawnDrop(interactor, ironItemData,  ironDropCount,  dropPos);
        // Удаляем объект камня из сцены
        Destroy(gameObject);
    }

    // Спавн лута
    private void SpawnDrop(PlayerInteraction interactor, ItemData data, int count, Vector3 pos)
    {
        if (data == null || count <= 0) return;
        for (int i = 0; i < count; i++)
            interactor.SpawnItemInWorld(data, 1, pos);
    }

}