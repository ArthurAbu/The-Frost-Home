using UnityEngine;

// Считает удары топором и при достижении лимита спавнит лут и уничтожает дерево.

public class ChoppableTree : MonoBehaviour
{
    [Header("Настройки рубки")]
    [SerializeField] private int hitsRequired = 3; // Сколько ударов нужно, чтобы срубить дерево

    [Header("Выпадающие ресурсы")]
    [SerializeField] private ItemData woodItemData;   
    [SerializeField] private ItemData stickItemData;
    [SerializeField] private ItemData foodItemData; 
    [SerializeField] private int woodDropCount = 2;   // Сколько брёвен выпадет
    [SerializeField] private int stickDropCount = 3;  // Сколько палок выпадет
    [SerializeField] private int foodDropCount = 4;  // Сколько яблок выпадет

    private int currentHits = 0; // Счётчик текущих ударов по дереву

    // Этот метод вызывается из PlayerInteraction каждый раз, когда игрок рубит дерево. Возвращает true, если дерево наконец срублено.
    public bool TryChop(PlayerInteraction interactor)
    {
        currentHits++;

        // Звук удара топором
        AudioManager.Instance?.PlayChopSound();

        if (currentHits >= hitsRequired)
        {
            // Звук падения дерева
            AudioManager.Instance?.PlayTreeFall();
            SpawnLootAndDestroy(interactor);
            return true; // Дерево срублено
        }

        return false; // Дерево ещё стоит
    }

    // Спавним ресурсы в точке дерева и удаляем его из сцены
    private void SpawnLootAndDestroy(PlayerInteraction interactor)
    {
        // Точка появления ресурсов чуть выше основания дерева
        Vector3 dropPos = transform.position + Vector3.up * 0.5f;
        // Спавним брёвна
        SpawnDrop(interactor, woodItemData,  woodDropCount,  dropPos);
        // Спавним палки
        SpawnDrop(interactor, stickItemData, stickDropCount, dropPos);
        // Спавним яблоки
        SpawnDrop(interactor, foodItemData,  foodDropCount,  dropPos);
        // Удаляем объект дерева из сцены
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