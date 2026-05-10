using System.Collections.Generic;
using UnityEngine;

// Создаёт и обновляет UI инвентаря: хотбар внизу экрана и основная сетка. Управляет Drag-and-Drop и выбором активного слота хотбара

public class InventoryUI : MonoBehaviour
{
    [Header("Ссылки на системы")]
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private GameObject inventoryPanel;

    [Header("Сетки для слотов")]
    [SerializeField] private Transform hotbarGrid;      // Хотбар
    [SerializeField] private Transform panelHotbarGrid; // Дубликат хотбара внутри инвентаря
    [SerializeField] private Transform mainGrid;        // Основной инвентарь

    [Header("Префабы")]
    [SerializeField] private GameObject slotPrefab;

    // Списки для хранения созданных UI-ячеек
    private List<SlotUI> hotbarSlots = new();        // Слот 0-9 (на экране)
    private List<SlotUI> panelHotbarSlots = new();  // Слот 0-9 (в инвентаре)
    private List<SlotUI> mainBagSlots = new();      // Слот 10-90

    private int activeHotbarIndex = 0;
    private int draggedSlotIndex = -1;

    void Start()
    {
        // Создаем все части интерфейса
        CreateHotbarUI();
        CreatePanelHotbarUI();
        CreateMainBagUI();
        
        RefreshUI();
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        // Открытие/закрытие инвентаря на I
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isOpening = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isOpening);
            
            // Скрываем игровой хотбар, когда открыт инвентарь
            hotbarGrid.gameObject.SetActive(!isOpening);

            Cursor.lockState = isOpening ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpening;
        }

        // Выбор слотов 1-9
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                activeHotbarIndex = i;
                RefreshUI();
            }
        }
        // Клавиша 0
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            activeHotbarIndex = 9;
            RefreshUI();
        }
    }

    // Создаем 10 слотов для хотбара на экране
    void CreateHotbarUI()
    {
        hotbarSlots.Clear();
        for (int i = 0; i < 10; i++)
        {
            var slot = Instantiate(slotPrefab, hotbarGrid).GetComponent<SlotUI>();
            slot.Setup(i, this);
            hotbarSlots.Add(slot);
        }
    }

    // Создаем 10 слотов-дубликатов внутри инвентаря
    void CreatePanelHotbarUI()
    {
        panelHotbarSlots.Clear();
        for (int i = 0; i < 10; i++)
        {
            var slot = Instantiate(slotPrefab, panelHotbarGrid).GetComponent<SlotUI>();
            slot.Setup(i, this); 
            panelHotbarSlots.Add(slot);
        }
    }

    // Создаем 80 слотов основного инвентаря
    void CreateMainBagUI()
    {
        mainBagSlots.Clear();
        for (int i = 0; i < 80; i++)
        {
            var slot = Instantiate(slotPrefab, mainGrid).GetComponent<SlotUI>();
            slot.Setup(i + 10, this); // Сдвиг на 10, так как первые 10 это хотбар
            mainBagSlots.Add(slot);
        }
    }

    public void RefreshUI()
    {
        // Проходим по всем данным в InventorySystem (все 90 слотов)
        for (int i = 0; i < inventory.slots.Count; i++)
        {
            var itemData = inventory.slots[i].item;
            var itemCount = inventory.slots[i].count;

            // Если это хотбар (0-9)
            if (i < 10)
            {
                // Обновляем ДВА места сразу
                hotbarSlots[i].SetItem(itemData, itemCount);
                panelHotbarSlots[i].SetItem(itemData, itemCount);
                
                // Подсвечиваем только активный
                hotbarSlots[i].SetHighlight(i == activeHotbarIndex);
                panelHotbarSlots[i].SetHighlight(i == activeHotbarIndex);
            }
            else // Если это инвентарь
            {
                mainBagSlots[i - 10].SetItem(itemData, itemCount);
            }
        }
    }

    public void OnStartDragging(int index) { draggedSlotIndex = index; }

    public void OnDropOnSlot(int dropIndex)
    {
        if (draggedSlotIndex != -1)
        {
            inventory.SwapSlots(draggedSlotIndex, dropIndex);
            draggedSlotIndex = -1;
            RefreshUI();
        }
    }

    // Получить индекс текущего выбранного слота в хотбаре
    public int GetActiveHotbarIndex() { return activeHotbarIndex; }

    // Вызывается когда предмет выброшен перетаскиванием за пределы UI
    public void OnDropOutside(int index)
    {
        InventorySlot slot = inventory.slots[index];
        if (slot.item == null) return;
        
        // Ссылка на камеру
        PlayerInteraction interactor = Camera.main.GetComponent<PlayerInteraction>();
        interactor.SpawnItemInWorld(slot.item, slot.count);
        
        inventory.RemoveFromSlot(index, slot.count);
        RefreshUI();
    }
}