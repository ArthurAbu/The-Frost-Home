using UnityEngine;

// Данные одного предмета игры.

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data", order = 1)]
public class ItemData : ScriptableObject
{
    public string itemName;     // Имя предмета
    public Sprite icon;         // Иконка
    public int stackLimit = 1;  // Сколько можно положить в одну ячейку
    public ItemType type;       // Тип предмета
    public int weight = 1;      // Вес предмета
    public GameObject dropPrefab; // 3D-модель (префаб), которая появится при выбросе

    [Header("Еда (только для типа Food)")]
    public float foodValue = 0f;    // Сколько восстанавливает голода
    public float healValue = 0f;    // Сколько восстанавливает здоровья
}

// Типы предметов
public enum ItemType
{
    Resource,    //ресурс
    Tool,        //Инструмнт
    Food,        //Еда
}