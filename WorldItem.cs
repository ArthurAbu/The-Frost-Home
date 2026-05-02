using UnityEngine;

// Компонент предмета лежащего на земле. Хранит ссылку на ItemData и количество
public class WorldItem : MonoBehaviour
{
    public ItemData itemData; // Какой это предмет
    public int count = 1;     // Сколько штук
    
    private Outline outline; // Компонент обводки

    private void Awake()
    {
        // Пытаемся найти компонент Outline
        outline = GetComponent<Outline>();
    }

    private void Start()
    {
        // Если Outline был добавлен из другого скрипта позже, ищем его еще раз
        if (outline == null) outline = GetComponent<Outline>();

        if (outline != null) 
        {
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = Color.yellow;
            outline.OutlineWidth = 5f;
            outline.enabled = false; // Принудительно выключаем
        }
    }

    // Включаем подсветку
    public void ToggleHighlight(bool readyToPick)
    {
        if (outline != null && outline.enabled != readyToPick) 
        {
            outline.enabled = readyToPick;
        }
    }
}