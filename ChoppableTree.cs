using UnityEngine;

// Считает удары топором и при достижении лимита спавнит лут и уничтожает дерево.

public class ChoppableTree : MonoBehaviour
{
    [Header("Настройки рубки")]
    [SerializeField] private int hitsRequired = 3; // Сколько ударов нужно, чтобы срубить дерево

    [Header("Выпадающие ресурсы")]
    [SerializeField] private ItemData woodItemData;   
    [SerializeField] private ItemData stickItemData;
    [SerializeField] private ItemData FoodItemData; 
    [SerializeField] private int woodDropCount = 2;   // Сколько брёвен выпадет
    [SerializeField] private int stickDropCount = 3;  // Сколько палок выпадет
    [SerializeField] private int FoodDropCount = 4;  // Сколько яблок выпадет

    private int currentHits = 0; // Счётчик текущих ударов по дереву

    // Этот метод вызывается из PlayerInteraction каждый раз, когда игрок рубит дерево. Возвращает true, если дерево наконец срублено.
    public bool TryChop(PlayerInteraction interactor)
    {
        currentHits++;

        if (currentHits >= hitsRequired)
        {
            SpawnLootAndDestroy(interactor);
            return true; // Дерево срублено
        }

        return false; // Дерево ещё стоит
    }

    // Спавним ресурсы в точке дерева и удаляем его из сцены
    private void SpawnLootAndDestroy(PlayerInteraction interactor)
    {
        // Точка появления ресурсов чуть выше основания дерева
        Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
        // Спавним брёвна
        if (woodItemData != null)
        {
            for (int i = 0; i < woodDropCount; i++)
                interactor.SpawnItemInWorld(woodItemData, 1, dropPosition);
        }

        // Спавним палки
        if (stickItemData != null)
        {
            for (int i = 0; i < stickDropCount; i++)
                interactor.SpawnItemInWorld(stickItemData, 1, dropPosition);
        }

        // Спавним яблоки
        if (FoodItemData != null)
        {
            for (int i = 0; i < FoodDropCount; i++)
                interactor.SpawnItemInWorld(FoodItemData, 1, dropPosition);
        }
        
        // Удаляем объект дерева из сцены
        Destroy(gameObject);
    }
}