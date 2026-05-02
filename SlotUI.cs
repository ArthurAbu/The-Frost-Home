using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// UI-ячейка инвентаря. Отображает предмет и реализует Drag-and-Drop.

public class SlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image icon;         // Иконка
    [SerializeField] private Text countText;     // Количество
    [SerializeField] private Image background;   // Фон

    private CanvasGroup canvasGroup; // Переменная для хранения компонента прозрачности
    private ItemData item;
    private int count;
    private int index; // Порядковый номер этой ячейки
    private InventoryUI ui; // Ссылка на главный скрипт интерфейса

    private void Awake()
    {
        // Ищем компонент CanvasGroup на том же объекте, где висит иконка
        if (icon != null)
        {
            canvasGroup = icon.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = icon.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    // Инициализация ячейки
    public void Setup(int newIndex, InventoryUI newUI)
    {
        index = newIndex;
        ui = newUI;
    }

    // Устанавливаем предмет в ячейку
    public void SetItem(ItemData newItem, int newCount)
    {
        item = newItem;
        count = newCount;

        bool hasItem = item != null;

        // Управляем видимостью
        if (icon != null) icon.enabled = hasItem;
        if (countText != null) countText.text = hasItem && count > 1 ? count.ToString() : "";
        if (background != null)
        {
            // Прозрачный фон при пустом слоте, непрозрачный — при предмете
            background.color = hasItem 
                ? new Color(1f, 1f, 1f, 0.8f)  // полупрозрачный фон
                : new Color(0f, 0f, 0f, 0.2f);   // полностью прозрачный
        }

        // Устанавливаем спрайт, если есть
        if (hasItem && icon != null && item.icon != null)
        {
            icon.sprite = item.icon;
        }
    }
    public ItemData GetItem() => item;
    public int GetCount() => count;

    // Подсветка активного слота хотбара
    public void SetHighlight(bool isActive)
    {
        if (background != null)
        {
            // Если слот выбран, красим в желтый, если нет в обычный
            background.color = isActive ? Color.yellow : new Color(1f, 1f, 1f, 0.8f);
        }
    }

    // Логика Drag-and-Drop 

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null) return;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f; // Делаем полупрозрачным
            canvasGroup.blocksRaycasts = false; // Позволяем лучу мыши "видеть" слоты под иконкой
        }
        
        // Сообщаем UI, что мы начали тащить этот слот
        ui.OnStartDragging(index);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item == null) return;
        // Иконка следует за курсором
        icon.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f; // Возвращаем яркость
            canvasGroup.blocksRaycasts = true; // Снова делаем иконку "осязаемой" для мыши
        }
        // Возвращаем иконку на место (RefreshUI потом поставит её правильно)
        icon.transform.localPosition = Vector3.zero;
        // Проверяем, попали ли мы в какой-то объект UI
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // Если мы отпустили мышь не над UI, значит выбрасываем
            ui.OnDropOutside(index);
        }
        else
        {
            ui.RefreshUI();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Когда мы отпускаем предмет над этим слотом
        ui.OnDropOnSlot(index);
    }
}